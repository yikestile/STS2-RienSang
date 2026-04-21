using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards.Ancient;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public class IndulgenceInPrescript : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && (cardSource is FuriosoReplica or FuriosoCrescendo or FuriosoLacrimosaCrescendo))
        {
            if (Owner.CombatState != null && Owner.CombatState.RunState.Rng.Niche.NextBool())
            {
                var poise = await PowerCmd.Apply<LCPoisePower>(Owner, 1m, Owner, cardSource);
                poise?.AddPotency(1);
            }
            else
            {
                if (target != null)
                {
                    var sinking = await PowerCmd.Apply<LCSinkingPower>(target, 1m, Owner, cardSource);
                    sinking?.AddPotency(1);
                }
            }
        }
    }

    public override async Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature applier, Creature? creature, CardModel? cardSource)
    {
        if (applier == Owner && (power is LCPoisePower or LCSinkingPower))
        {
            amount += 1m;
        }
        await Task.CompletedTask;
    }
    
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && (cardSource is FuriosoReplica or FuriosoCrescendo or FuriosoLacrimosaCrescendo))
        {
            return 1.10m;
        }
        return 1m;
    }
    
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}