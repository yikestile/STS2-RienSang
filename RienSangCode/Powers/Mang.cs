using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards.Ancient;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;
public class Mang : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("DamageBonus", 0m),
        new("TempStr", 0m)
    ];

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (cardSource is FuriosoReplica or FuriosoCrescendo or FuriosoLacrimosaCrescendo)
        {
            return 1m + (base.Amount * 0.33m);
        }
        return 1m;
    }
    
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;

        decimal actualGain = amount;
        if (power.Amount > 3m)
        {
            actualGain = Math.Max(0, 3m - (power.Amount - amount));
            power.SetAmount(3);
        }

        DynamicVars["DamageBonus"].BaseValue = power.Amount * 33;
        DynamicVars["TempStr"].BaseValue = power.Amount * 1;

        if (actualGain > 0 && amount > 0) 
        {
            await PowerCmd.Apply<StrengthPower>(Owner, (int)actualGain * 1, Owner, null);
        }
    }
    
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            Flash();
            await PowerCmd.Apply<StrengthPower>(Owner, -(base.Amount * 1), Owner, null);
            await PowerCmd.Remove( this);
        }
    }
}