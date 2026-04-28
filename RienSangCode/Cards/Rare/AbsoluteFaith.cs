
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Cards.Common;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class AbsoluteFaith : RienSangCard
{
    public AbsoluteFaith() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, RienSangKeywords.Caduceus];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile == null) return;

        var blindFaiths = discardPile.Cards.Where(c => c.Tags.Contains(RienSangTags.BlindFaith)).ToList();

        if (blindFaiths.Any())
        {
            await CardPileCmd.Add(blindFaiths, PileType.Draw);

            if (IsUpgraded)
            {
                var drawPile = PileType.Draw.GetPile(Owner);
                var oneToHand = drawPile.Cards.FirstOrDefault(c => c.Tags.Contains(RienSangTags.BlindFaith));
                if (oneToHand != null)
                {
                    await CardPileCmd.Add(oneToHand, PileType.Hand);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}

