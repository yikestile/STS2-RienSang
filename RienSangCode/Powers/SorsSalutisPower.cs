using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public sealed class SorsSalutisPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override bool IsInstanced => false;

    public SorsSalutisPower() : base() { }

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && amount >= 10m)
        {
            Flash();
            decimal half = amount / 2m;
            
            // Give half as Karma
            TaskHelper.RunSafely(Owner.ApplyKarma(new ThrowingPlayerChoiceContext(), half, Owner, null));
            
            return half;
        }
        return amount;
    }
}