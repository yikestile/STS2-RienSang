using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;
using BaseLib.Extensions;

namespace RienSang.RienSangCode.Relics;

[Pool(typeof(RienSangRelicPool))]
public class SomeonesDevice : RienSangRelic
{
    private static readonly SpireField<ICombatState, bool> _sinkingNextTurn = new SpireField<ICombatState, bool>(() => false);

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult results, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == Owner.Creature && target.HasPower<ThePrescriptsTarget>())
        {
            if (results.UnblockedDamage > 0)
            {
                if (Owner.Creature.CombatState != null)
                {
                    _sinkingNextTurn[Owner.Creature.CombatState] = true;
                }
            }
        }
        await Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == CombatSide.Player && _sinkingNextTurn[combatState])
        {
            Flash();
            var target = combatState.Enemies.FirstOrDefault(e => e.IsAlive && e.HasPower<ThePrescriptsTarget>());
            if (target != null)
            {
                await PowerCmd.Apply<LCSinkingPower>(new ThrowingPlayerChoiceContext(), target, 2m, Owner.Creature, null);
            }
            _sinkingNextTurn[combatState] = false;
        }
    }
}