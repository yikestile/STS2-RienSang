using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Cards.Curse;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Powers;

public class KarmicConsequence : RienSangPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("KarmaAmount", 0m)
    };

    public override int DisplayAmount => DynamicVars["KarmaAmount"].IntValue;

    protected override bool IsVisibleInternal => true;

    public KarmicConsequence() : base() { } 
    
    public KarmicConsequence(int amount) : this()
    {
        DynamicVars["KarmaAmount"].BaseValue = amount;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Amount != 1)
        {
            SetAmount(1, silent: true);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;
        
        if (Amount != 1)
        {
            SetAmount(1, silent: true);
        }

        int diff = (int)amount;
        if (diff == 0) return;

        int currentKarma = DynamicVars["KarmaAmount"].IntValue;
        int newKarma = Math.Clamp(currentKarma + diff, 0, 100);

        if (newKarma != currentKarma)
        {
            DynamicVars["KarmaAmount"].BaseValue = newKarma;
            InvokeDisplayAmountChanged();
            await HandleThresholds(currentKarma, newKarma);
        }
    }

    private async Task HandleThresholds(int oldKarma, int newKarma)
    {
        var oldMilestones20 = oldKarma / 20;
        var newMilestones20 = newKarma / 20;
        if (newMilestones20 > oldMilestones20)
        {
            await PowerCmd.Apply<LCFragilePower>(Owner, newMilestones20 - oldMilestones20, Owner, null);
        }

        if (oldKarma < 40 && newKarma >= 40)
        {
            await ApplyRandomCurse(true);
        }

        if (oldKarma < 80 && newKarma >= 80)
        {
            await ApplyRandomCurse(false);
        }

        if (newKarma >= 100)
        {
            Flash();
            
            if (Owner.Player?.Character is Character.RienSang character)
            {
                character.PlayAnimation(Owner, "dead");
            }
            
            await CreatureCmd.Kill(Owner);
        }
    }

    private async Task ApplyRandomCurse(bool temporary)
    {
        if (Owner.CombatState == null || Owner.Player == null) return;

        var rng = Owner.CombatState.RunState.Rng.Niche;
        var roll = rng.NextInt(0, 3);
        CardModel card;

        switch (roll)
        {
            case 0: card = Owner.CombatState.CreateCard<Spork>(Owner.Player); break;
            case 1: card = Owner.CombatState.CreateCard<Fpoon>(Owner.Player); break;
            default: card = Owner.CombatState.CreateCard<PrescriptIncomplianceRisk>(Owner.Player); break;
        }

        if (temporary)
        {
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, true);
        }
        else
        {
            if (card is Spork) await CardPileCmd.AddCurseToDeck<Spork>(Owner.Player);
            else if (card is Fpoon) await CardPileCmd.AddCurseToDeck<Fpoon>(Owner.Player);
            else if (card is PrescriptIncomplianceRisk) await CardPileCmd.AddCurseToDeck<PrescriptIncomplianceRisk>(Owner.Player);
        }
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player == Owner.Player && DynamicVars["KarmaAmount"].IntValue >= 60)
        {
            return Math.Max(0m, count - 1m);
        }
        return count;
    }

    public override Task AfterModifyingHandDraw()
    {
        if (DynamicVars["KarmaAmount"].IntValue >= 60) Flash();
        return Task.CompletedTask;
    }
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var milestones = DynamicVars["KarmaAmount"].IntValue / 20;
        if (milestones > 0)
        {
            await PowerCmd.Apply<LCFragilePower>(Owner, milestones, Owner, null);
        }
    }
}
