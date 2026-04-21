using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class AsThePrescriptOrdered : RienSangCard
{
    public AsThePrescriptOrdered() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile != null)
        {
            var attackCards = drawPile.Cards.Where(c => c.Type == CardType.Attack).ToList();
            if (attackCards.Count > 0)
            {
                var card = Owner.RunState.Rng.Niche.NextItem(attackCards);
                
                if (card != null)
                {
                    await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, null, false);
                    card.SetToFreeThisTurn();
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}