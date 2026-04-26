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
                await LCPoisePower.Apply(choiceContext, Owner, 1, 1, Owner, cardSource);
            }
            else
            {
                if (target != null)
                {
                    await LCSinkingPower.Apply(choiceContext, target, 1, 1, Owner, cardSource);
                }
            }
        }
    }

    public override async Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
    {
        if (applier == Owner && (power is LCPoisePower or LCSinkingPower)) { }
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