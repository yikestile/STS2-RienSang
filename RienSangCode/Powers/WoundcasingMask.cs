using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public class WoundcasingMask : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;


    public bool FuriosoPlayedThisCombat;

    public WoundcasingMask()
    {
    }

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return target == Owner ? amount * 0.95m : amount;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        
        if (FuriosoPlayedThisCombat)
        {
            await PowerCmd.Apply<SizzlingWound>(Owner, 1, Owner, null);
            await PowerCmd.Remove(this);
        }
    }
}