using System.Threading.Tasks;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Relics;

namespace RienSang.RienSangCode.Powers;

public class ThePrescriptsTarget : RienSangPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && dealer is { Player: not null })
        {
            bool isUpgraded = dealer.Player.Relics.Any(r => r is OraclesPrescript);
            return isUpgraded ? 1.10m : 1.05m;
        }

        return 1m;
    }
    
    public override async Task AfterSideTurnStart(CombatSide combatSide, ICombatState combatState)
    {
        if (combatSide == CombatSide.Enemy)
        {
            await PowerCmd.Remove( this);
        }
    }
}
