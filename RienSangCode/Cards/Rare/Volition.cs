
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
public class Volition : RienSangCard
{
    public Volition() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, RienSangKeywords.Caduceus];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile == null) return;

        var cardsToPlay = discardPile.Cards
            .Where(c => c.Tags.Contains(RienSangTags.BlindFaith))
            .ToList();

        foreach (var card in cardsToPlay)
        {
            if (Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding) break;
            
            await CardPileCmd.Add(card, PileType.Play);
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
