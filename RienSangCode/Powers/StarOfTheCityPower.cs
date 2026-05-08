using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public sealed class StarOfTheCityPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool IsInstanced => false;

    public const int SinkingPotencyPerStack = 5;
    public const int SinkingCountPerStack = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new("SinkingPotency", 0m),
        new("SinkingCount", 0m)
    };
    
    public StarOfTheCityPower()
    {
    }

    public StarOfTheCityPower(int amount)
    {
        SetAmount(amount);
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        UpdateDynamicVars();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        UpdateDynamicVars();
        return Task.CompletedTask;
    }

    private void UpdateDynamicVars()
    {
        DynamicVars["SinkingPotency"].BaseValue = Amount * SinkingPotencyPerStack;
        DynamicVars["SinkingCount"].BaseValue = Amount * SinkingCountPerStack;
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }

        Flash();
        int totalCount = (int)DynamicVars["SinkingCount"].BaseValue;
        int totalPotency = (int)DynamicVars["SinkingPotency"].BaseValue;

        var enemies = combatState.HittableEnemies;
        foreach (var enemy in enemies)
        {
            await LCSinkingPower.Apply(new ThrowingPlayerChoiceContext(), enemy, totalCount, totalPotency, Owner, null);
        }
    }
}
