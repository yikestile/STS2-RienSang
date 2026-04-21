using BaseLib.Abstracts;
using BaseLib.Extensions;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Extensions;
using System;

namespace RienSang.RienSangCode.Powers;

public sealed class ShatteredWorld : RienSangPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner)
        {
            decimal multiplier = 1m;

            if (props.HasFlag(ValueProp.Unpowered) && cardSource == null && dealer == null)
            {

                multiplier *= 1.2m;
            }

            if (dealer != null && dealer.Player != null)
            {
                var charId = dealer.Player.Character.Id.Entry;
                if (charId.StartsWith("RienSang", StringComparison.OrdinalIgnoreCase))
                {
                    multiplier *= 1.1m;
                }
            }

            return multiplier;
        }
        return 1m;
    }
}