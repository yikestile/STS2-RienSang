using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class AsThePrescriptDemands : RienSangCard
{
    public AsThePrescriptDemands() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new RepeatVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // "Discard your hand"
        var hand = PileType.Hand.GetPile(Owner);
        int discardedCount = 0;
        
        if (hand != null && hand.Cards.Count > 0)
        {
            var cardsToDiscard = new List<CardModel>(hand.Cards);
            discardedCount = cardsToDiscard.Count;
            // Discard and draw equal amount
            await CardCmd.DiscardAndDraw(choiceContext, cardsToDiscard, discardedCount);
        }
        
        // "Plays 3 random cards." (from the hand, presumably the newly drawn ones)
        // Need to fetch hand again as it has new cards
        hand = PileType.Hand.GetPile(Owner);
        if (hand != null && hand.Cards.Count > 0)
        {
            int playCount = DynamicVars.Repeat.IntValue;
            var rng = Owner.RunState.Rng.Niche;
            var cardsInHand = new List<CardModel>(hand.Cards);
            
            for (int i = 0; i < playCount; i++)
            {
                if (cardsInHand.Count == 0) break;
                
                var cardToPlay = rng.NextItem(cardsInHand);
                if (cardToPlay != null)
                {
                    cardsInHand.Remove(cardToPlay);
                    // Play the card
                    await CardCmd.AutoPlay(choiceContext, cardToPlay, null);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}