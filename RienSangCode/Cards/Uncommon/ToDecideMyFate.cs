using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class ToDecideMyFate : RienSangCard
{
    public ToDecideMyFate() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaLoss", 5),
        new RepeatVar(0), // Uses X
        new("ExtraPlays", 1) // +1(+2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        if (x <= 0) return;

        int extra = (int)DynamicVars["ExtraPlays"].BaseValue;
        int totalPlays = x + extra;

        // Exhaust X random cards
        var hand = PileType.Hand.GetPile(Owner);
        if (hand != null && hand.Cards.Count > 0)
        {
            var rng = Owner.RunState.Rng.Niche;
            var cardsInHand = new List<CardModel>(hand.Cards);
            
            for (int i = 0; i < x; i++)
            {
                if (cardsInHand.Count == 0) break;
                
                var cardToExhaust = rng.NextItem(cardsInHand);
                if (cardToExhaust != null)
                {
                    cardsInHand.Remove(cardToExhaust);
                    await CardCmd.Exhaust(choiceContext, cardToExhaust);
                }
            }
        }
        
        // Draw X
        await CardPileCmd.Draw(choiceContext, x, Owner);
        
        // Play X + Extra random cards
        hand = PileType.Hand.GetPile(Owner);
        if (hand != null && hand.Cards.Count > 0)
        {
            var rng = Owner.RunState.Rng.Niche;
            var cardsInHand = new List<CardModel>(hand.Cards);
            
            for (int i = 0; i < totalPlays; i++)
            {
                if (cardsInHand.Count == 0) break;
                
                var cardToPlay = rng.NextItem(cardsInHand);
                if (cardToPlay != null)
                {
                    cardsInHand.Remove(cardToPlay);
                    await CardCmd.AutoPlay(choiceContext, cardToPlay, null);
                }
            }
        }

        // Lose Karma
        await PowerCmd.Apply<KarmicConsequence>(Owner.Creature, -DynamicVars["KarmaLoss"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ExtraPlays"].UpgradeValueBy(1);
    }
}
