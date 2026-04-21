using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public class TheIndexNursefatherPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public TheIndexNursefatherPower() : base()
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("WeakAmount", 1)
    ];

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        var combatState = player.Creature.CombatState;

        if (combatState != null)
        {
            var aliveEnemies = combatState.Enemies.Where(e => e.IsAlive).ToList();
            if (aliveEnemies.Count > 0)
            {
                var target = aliveEnemies.TakeRandom(1, player.RunState.Rng.CombatTargets).FirstOrDefault();
                if (target != null)
                {
                    await PowerCmd.Apply<LCWeakNextTurn>(target, (int)DynamicVars["WeakAmount"].BaseValue, Owner, null);
                }
            }
        }

        var unlockPower = Owner.GetPower<Unlock>();
        if (unlockPower != null && unlockPower.Amount >= 3)
        {
            await CardPileCmd.Draw(choiceContext, 1, player);
        }
    }
}