using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public sealed class SorsImmanisPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldTakeExtraTurn(Player player)
    {
        if (player.Creature == Owner && Amount > 0)
        {
            return true;
        }
        return false;
    }

    public override async Task AfterTakingExtraTurn(Player player)
    {
        if (player.Creature == Owner)
        {
            Flash();
            await PowerCmd.Decrement(this);
        }
    }
}