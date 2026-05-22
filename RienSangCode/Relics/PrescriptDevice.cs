using System; // Added for Math.Round
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Relics;
using RienSang.RienSangCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves; // Added for [SavedProperty]
using LimbusCore.LimbusCoreCode.Mechanics;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace RienSang.RienSangCode.Relics;

[Pool(typeof(RienSangRelicPool))]
public class PrescriptDevice : RienSangRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    private bool _unlockReachedThisTurn = false;
    private bool _lastTurnWasClear = false;

    private static readonly string ScrambleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?/";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<MarkofthePrescriptPower>(1m),
        new PowerVar<ThePrescriptsTarget>(1m),
        new PowerVar<WoundcasingMask>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<MarkofthePrescriptPower>(),
        HoverTipFactory.FromPower<ThePrescriptsTarget>(),
        HoverTipFactory.FromPower<WoundcasingMask>(),
    ];

    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature == null) return;

        Flash();

        await PowerCmd.Apply<MarkofthePrescriptPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);
        await PowerCmd.Apply<WoundcasingMask>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);

        TriggerOverlay(GenerateRandomString(), true);
        _lastTurnWasClear = false;
        _unlockReachedThisTurn = false;
        
        
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;

        if (_unlockReachedThisTurn)
        {
            TriggerOverlay("_CLEAR._", false);
            _unlockReachedThisTurn = false;
            _lastTurnWasClear = true;
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        var unlock = Owner.Creature.GetPower<Unlock>();
        if (_lastTurnWasClear && (unlock == null || unlock.Amount < 3))
        {
            TriggerOverlay(GenerateRandomString(), true);
            _lastTurnWasClear = false;
        }

        var hermes = Owner.Creature.GetPower<ProcurationHermes>();
        if (hermes != null && hermes.DisplayAmount == 9)
        {
            TriggerOverlay(GenerateRandomString(), true);
        }
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is Unlock && amount > 0)
        {
            _unlockReachedThisTurn = true;
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Id.Entry.Contains("FURIOSO"))
        {
            _unlockReachedThisTurn = true;
        }
    }

    private void TriggerOverlay(string text, bool isRandom)
    {
        var visuals = Owner?.Creature?.GetCreatureNode()?.Visuals as NRiensang;
        if (visuals != null)
        {
            visuals.TriggerPrescriptOverlay(text, isRandom);
        }
    }

    private string GenerateRandomString()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        string result = "";
        for (int i = 0; i < 8; i++)
        {
            result += ScrambleChars[rng.RandiRange(0, ScrambleChars.Length - 1)];
        }
        return result;
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner?.Creature == null) return;

        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var potentialTargets = combatState.HittableEnemies;

        if (potentialTargets.Count > 0)
        {
            var target = potentialTargets.OrderBy(_ => Owner.RunState.Rng.Niche.NextFloat()).First();
            await PowerCmd.Apply<ThePrescriptsTarget>(choiceContext, target, 1m, Owner.Creature, null);
        }
    }
}
