using System.Collections.Generic;
using System.Linq;
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
public class RecitePrescript : RienSangCard
{
    public RecitePrescript() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new RepeatVar(2),
        new("KarmaLoss", 10)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // "Plays 2(3) random cards in hand and exhaust them."
        var hand = PileType.Hand.GetPile(Owner);
        if (hand == null || hand.Cards.Count == 0)
        {
            return;
        }

        int count = DynamicVars.Repeat.IntValue;
     
        var rng = Owner.RunState.Rng.Niche;
        var cardsInHand = new List<CardModel>(hand.Cards.Where(c => c != this).ToList());
        
        for (int i = 0; i < count; i++)
        {
            if (cardsInHand.Count == 0) break;
            
            var cardToPlay = rng.NextItem(cardsInHand);
            if (cardToPlay != null) // Null check
            {
                cardsInHand.Remove(cardToPlay);
                cardToPlay.ExhaustOnNextPlay = true;
                await CardCmd.AutoPlay(choiceContext, cardToPlay, null);
            }
        }

        // Lose Karma
        await PowerCmd.Apply<KarmicConsequence>(Owner.Creature, -DynamicVars["KarmaLoss"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
