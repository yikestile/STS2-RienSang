using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Helpers; // TaskHelperのために追加
using RienSang.RienSangCode.Cards.Ancient;
using RienSang.RienSangCode.Cards.Basic; 
using RienSang.RienSangCode.Cards.Rare; 
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Relics;
using BaseLib.Utils;

namespace RienSang.RienSangCode.Powers;

public sealed class MarkofthePrescriptPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    
    private static readonly SpireField<Creature, int> _totalMissedThisCombat = new SpireField<Creature, int>(() => 0);

    public bool HermesConsumedThisTurn = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("HermesThresholdCount", 0m)
    };
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<GraceofthePrescriptPower>();
            yield return HoverTipFactory.FromPower<ProcurationHermes>();
            yield return HoverTipFactory.FromPower<KarmicConsequence>();
        }
    }

    private bool IsUpgraded => Owner != null && Owner.Player != null && Owner.Player.Relics.Any(r => r is OraclesPrescript);
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player != Owner?.Player) return;

        HermesConsumedThisTurn = false;

        var hand = PileType.Hand.GetPile(Owner.Player);
        if (hand == null) return; 

        var availableCards = hand.Cards.Where(c => 
            !c.HasSingleTurnMark() && 
            !c.Keywords.Contains(CardKeyword.Unplayable) &&
            c.GetType().Name != "ByUnpredictableWhim" &&
            c.GetType().Name != "ByGodsWill" &&
            c.GetType().Name != "Tradeoff" &&
            c.GetType().Name != "Reconstruct" 
        ).ToList();

        availableCards = availableCards.Where(c => {
            if (c is GodsBlessing godsBlessing) {
                return godsBlessing.MeetConditions;
            }
            return true;
        }).ToList();

        if (!availableCards.Any()) return;

        var targetMarkCount = IsUpgraded ? 3 : 2;
        var markedCount = 0;
        var cardsToMark = new List<CardModel>();

        var furiosoCards = availableCards.Where(c => c is FuriosoCrescendo or FuriosoLacrimosaCrescendo or FuriosoReplica).ToList();
        foreach (var furiosoCard in furiosoCards)
        {
            if (markedCount < targetMarkCount)
            {
                cardsToMark.Add(furiosoCard);
                markedCount++;
            }
        }

        if (markedCount < targetMarkCount)
        {
            var remainingCards = availableCards.Except(cardsToMark).ToList();
            if (Owner.CombatState != null)
            {
                var rng = Owner.CombatState.RunState.Rng.Niche;
                var randomCards = remainingCards.OrderBy(_ => rng.NextInt()).Take(targetMarkCount - markedCount).ToList();
                cardsToMark.AddRange(randomCards);
            }
        }

        foreach (var card in cardsToMark)
        {
            card.GiveSingleTurnMark();
        }
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var playedCard = cardPlay.Card; 
        if (playedCard.Owner != Owner?.Player) return Task.CompletedTask;

        if (playedCard.HasSingleTurnMark())
        {
            playedCard.ClearSingleTurnMark();
            
            TaskHelper.RunSafely(ApplyPrescriptRewards(playedCard));
        }
        
        return Task.CompletedTask;
    }

    private async Task ApplyPrescriptRewards(CardModel sourceCard)
    {
        await PowerCmd.Apply<GraceofthePrescriptPower>(Owner, 1m, Owner, null);
        await PowerCmd.Apply<ProcurationHermes>(Owner, 1m, Owner, null);
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Owner?.Player == null || side != Owner.Side) return;
        if (Owner.HasPower<TheOraclesProxyPower>()) return;

        var hand = PileType.Hand.GetPile(Owner.Player);
        if (hand == null) return;

        int currentMissed = hand.Cards.Count(c => c.HasSingleTurnMark());
        if (currentMissed > 0)
        {
            decimal totalKarma;
            if (IsUpgraded)
            {
                totalKarma = 5m * currentMissed;
            }
            else
            {
                int totalMissedBefore = _totalMissedThisCombat[Owner];
                int penaltyPerCard = 5 + (totalMissedBefore * 5);
                totalKarma = penaltyPerCard * currentMissed;

                _totalMissedThisCombat[Owner] = totalMissedBefore + currentMissed;
            }

            await PowerCmd.Apply<KarmicConsequence>(Owner, totalKarma, Owner, null);
        }
    }
    
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && power.Amount <= 0m && Owner?.Player != null)
        {
            var allPiles = Owner.Player.Piles;
            foreach (var pile in allPiles)
            {
                foreach (var card in pile.Cards)
                {
                    if (card.HasSingleTurnMark())
                    {
                        card.ClearSingleTurnMark();
                    }
                }
            }
        }
        await Task.CompletedTask;
    }
}