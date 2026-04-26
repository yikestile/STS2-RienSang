using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using LimbusCore.LimbusCoreCode.Mechanics;
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

public class PainfulScar : RienSangPower, LCPanicPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        Flash();

        await LCBurnPower.Apply(choiceContext, Owner, 2, 3, Owner, null);
        await LCBleedPower.Apply(choiceContext, Owner, 2, 3, Owner, null);
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner)
        {
            return 1.1m;
        }
        return 1m;
    }
}