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

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new("SinkingCount", 2m),
        new("SinkingPotency", 5m)
    };
    
    public StarOfTheCityPower()
    {
    }

    public StarOfTheCityPower(int amount)
    {
        SetAmount(amount);
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }

        Flash();
        int stack = (int)base.Amount;
        int count = 2 * stack;
        int potency = 5 * stack;

        var enemies = combatState.HittableEnemies;
        foreach (var enemy in enemies)
        {
            await LCSinkingPower.Apply(new ThrowingPlayerChoiceContext(), enemy, count, potency, Owner, null);
        }
    }
}