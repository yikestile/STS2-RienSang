using BaseLib.Utils;
using HarmonyLib;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using RienSang.RienSangCode.Character;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class PrecognitionReplica : RienSangCard
{
    protected override bool HasEnergyCostX => true;

    public PrecognitionReplica() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCEvadePower>(7), 
        new("PoisePotency", 6),
        new PowerVar<LCPoisePower>(2), 
        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCEvadePower>(),
        HoverTipFactory.FromPower<LCPoisePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int x = ResolveEnergyXValue();
        if (x > 0)
        {
            int evadeAmount = DynamicVars[nameof(LCEvadePower)].IntValue * x;
            await PowerCmd.Apply<LCEvadePower>(Owner.Creature, evadeAmount, Owner.Creature, this);
            
            EvadeRegistry.EvadeEffectStacks[Owner.Creature] += 1;
            EvadeRegistry.PendingPoisePotency[Owner.Creature] += (int)DynamicVars["PoisePotency"].BaseValue;
            
            int countVal = DynamicVars[nameof(LCPoisePower)].IntValue;
            if (countVal > EvadeRegistry.PendingPoiseCount[Owner.Creature])
            {
                EvadeRegistry.PendingPoiseCount[Owner.Creature] = countVal;
            }

            EvadeRegistry.PendingEnergy[Owner.Creature] += DynamicVars.Energy.IntValue;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LCEvadePower)].UpgradeValueBy(3);
        DynamicVars["PoisePotency"].UpgradeValueBy(3);
        DynamicVars[nameof(LCPoisePower)].UpgradeValueBy(1);
    }

    [HarmonyPatch]
    public static class PrecognitionHooks
    {
        [HarmonyPatch(typeof(LCEvadePower), nameof(LCEvadePower.ModifyHpLostBeforeOsty))]
        [HarmonyPostfix]
        public static void EvadePowerTriggerPostfix(LCEvadePower __instance, decimal __result)
        {
            if (__result == 0m && __instance.Owner != null)
            {
                var creature = __instance.Owner;
                int energyToGrant = EvadeRegistry.PendingEnergy[creature];
                
                if (energyToGrant > 0)
                {
                    TaskHelper.RunSafely(PowerCmd.Apply<EnergyNextTurnPower>(creature, energyToGrant, creature, null));
                    EvadeRegistry.PendingEnergy[creature] = 0;
                }
            }
        }

        [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeSideTurnStart))]
        [HarmonyPostfix]
        public static void BeforeSideTurnStartPostfix(CombatState combatState, CombatSide side)
        {
            if (side != CombatSide.Player) return;
            TaskHelper.RunSafely(ProcessOnEvadeRewards(combatState));
        }

        private static async Task ProcessOnEvadeRewards(CombatState combatState)
        {
            foreach (var player in combatState.Players)
            {
                var creature = player.Creature;
                if (EvadeRegistry.EvadeEffectStacks[creature] > 0)
                {
                    if (LCEvadePower.HasEvadedThisTurn[creature])
                    {
                        int potency = EvadeRegistry.PendingPoisePotency[creature];
                        int count = EvadeRegistry.PendingPoiseCount[creature];
                        if (count > 0)
                        {
                            var poise = await PowerCmd.Apply<LCPoisePower>(creature, count, creature, null);
                            poise?.AddPotency(potency);
                        }

                        int draw = EvadeRegistry.PendingDraw[creature];
                        if (draw > 0)
                        {
                            await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), draw, player);
                        }

                        int sinkingPotency = EvadeRegistry.PendingSinkingPotencyAll[creature];
                        if (sinkingPotency > 0)
                        {
                            foreach (var enemy in combatState.Enemies)
                            {
                                if (enemy.IsAlive)
                                {
                                    var sinking = await PowerCmd.Apply<LCSinkingPower>(enemy, 1, creature, null);
                                    sinking?.AddPotency(sinkingPotency);
                                }
                            }
                        }
                    }

                    EvadeRegistry.EvadeEffectStacks[creature] = 0;
                    EvadeRegistry.PendingPoisePotency[creature] = 0;
                    EvadeRegistry.PendingPoiseCount[creature] = 0;
                    EvadeRegistry.PendingEnergy[creature] = 0;
                    EvadeRegistry.PendingDraw[creature] = 0;
                    EvadeRegistry.PendingSinkingPotencyAll[creature] = 0;
                }
            }
        }
    }
}