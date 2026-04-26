using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public sealed class AtonementPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaReduction", 0m)
    ];

    public AtonementPower() : base() { }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            DynamicVars["KarmaReduction"].BaseValue = Amount * 5m;
        }
        await Task.CompletedTask;
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        DynamicVars["KarmaReduction"].BaseValue = Amount * 5m;
        await Task.CompletedTask;
    }
}