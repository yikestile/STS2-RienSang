using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public sealed class PoisedWardingPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("PoiseGain", 0m),
        new("BlockGain", 0m)
    ];

    public PoisedWardingPower() : base() { }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            UpdateDynamicVars();
        }
        await Task.CompletedTask;
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        UpdateDynamicVars();
        await Task.CompletedTask;
    }

    private void UpdateDynamicVars()
    {
        DynamicVars["PoiseGain"].BaseValue = Amount * 3m;
        DynamicVars["BlockGain"].BaseValue = Amount * 3m;
    }

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            Flash();
            await LCPoisePower.Apply(new ThrowingPlayerChoiceContext(), Owner, 1, (int)DynamicVars["PoiseGain"].BaseValue, Owner, null);
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterAttack))]
    public static class PoisedWardingCritHook
    {
        [HarmonyPostfix]
        public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, AttackCommand command)
        {
            if (command.Attacker == null) return;
            var player = command.Attacker;

            var power = player.GetPower<PoisedWardingPower>();
            if (power != null && CritRegistry.WasLastAttackCrit[player])
            {
                TaskHelper.RunSafely(power.GrantBlock(choiceContext));
            }
        }
    }

    private async Task GrantBlock(PlayerChoiceContext context)
    {
        Flash();
        await CreatureCmd.GainBlock(Owner, DynamicVars["BlockGain"].BaseValue, ValueProp.Unpowered, null);
    }
}