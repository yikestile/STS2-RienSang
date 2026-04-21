using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;
public class ShinFate : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await PlayerCmd.GainEnergy(1, player);
    }
    
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner)
        {
            return 1.05m;
        }
        return 1m;
    }

    public override async Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature applier, Creature? creature, CardModel? cardSource)
    {
        if (applier == Owner && power is LCPoisePower && cardSource != null)
        {
            amount += 1m;
        }
        await Task.CompletedTask;
    }
}