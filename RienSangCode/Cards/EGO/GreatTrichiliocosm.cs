using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.EGO;

[Pool(typeof(RienSangCardPool))]
public class GreatTrichiliocosm : RienSangCard
{
    public GreatTrichiliocosm() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override bool IsEgoCard => true;
    public override int SpCost => 30;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move),
        new RepeatVar(4),
        new PowerVar<LCPoisePower>(3),
        new("PoisePotency", 7),
        new PowerVar<LCSinkingPower>(3),
        new PowerVar<LCBurnPower>(2),
        new("BurnPotency", 7)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCPoisePower>(),
        HoverTipFactory.FromPower<LCSinkingPower>(),
        HoverTipFactory.FromPower<LCBurnPower>(),
        HoverTipFactory.FromPower<ShatteredWorld>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var myPoise = await PowerCmd.Apply<LCPoisePower>(Owner.Creature, (int)DynamicVars[nameof(LCPoisePower)].BaseValue, Owner.Creature, this);
        myPoise?.AddPotency((int)DynamicVars["PoisePotency"].BaseValue);

        if (Owner.Creature.CombatState == null) return;

        var enemies = Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive).ToList();
        
        bool bonusDamage = enemies.Any(e => 
            (e.GetPower<LCSinkingPower>()?.Potency ?? 0) >= 10 || 
            (e.GetPower<LCBurnPower>()?.Potency ?? 0) >= 10);

        int baseDmg = (int)DynamicVars.Damage.BaseValue + (bonusDamage ? 1 : 0);
        int hits = DynamicVars.Repeat.IntValue;

        for (int i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(baseDmg).FromCard(this).TargetingAllOpponents(Owner.Creature.CombatState).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        }

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            await PowerCmd.Apply<ShatteredWorld>(enemy, 1, Owner.Creature, this);
            await PowerCmd.Apply<LCSinkingPower>(enemy, (int)DynamicVars[nameof(LCSinkingPower)].BaseValue, Owner.Creature, this); 
            
            var burn = await PowerCmd.Apply<LCBurnPower>(enemy, (int)DynamicVars[nameof(LCBurnPower)].BaseValue, Owner.Creature, this); 
            burn?.AddPotency((int)DynamicVars["BurnPotency"].BaseValue); 
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterAttack))]
    public static class GreatTrichiliocosmCritHook
    {
        [HarmonyPostfix]
        public static void Postfix(CombatState combatState, AttackCommand command)
        {
            if (command.Attacker == null || command.Attacker.Player == null) return;
            var player = command.Attacker.Player;
            var creature = command.Attacker;

            if (command.ModelSource is GreatTrichiliocosm && player.Creature == creature && CritRegistry.WasLastAttackCrit[creature])
            {
                TaskHelper.RunSafely(ApplyBurnActivation(command, creature));
            }
        }

        private static async Task ApplyBurnActivation(AttackCommand command, Creature creature)
        {
            foreach (var result in command.Results)
            {
                var target = result.Receiver;
                if (target == null) continue;

                var burnPower = target.GetPower<LCBurnPower>();
                if (burnPower != null && burnPower.Count > 0 && burnPower.Potency > 0)
                {
                    await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, burnPower.Potency, ValueProp.Unpowered, creature, null);
                    
                    await PowerCmd.Decrement(burnPower);
                }
            }
        }
    }
}
