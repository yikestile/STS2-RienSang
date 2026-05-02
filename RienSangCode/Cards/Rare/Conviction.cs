
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
public class Conviction : RienSangCard
{
    public Conviction() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var allCards = Owner.Piles.SelectMany(p => p.Cards).ToList();
        int totalBlindFaith = allCards.Count(c => c.Tags.Contains(RienSangTags.BlindFaith));

        if (totalBlindFaith > 0 && Owner.Creature.CombatState != null)
        {
            var newCards = new List<CardModel>();
            ICombatState combatState = Owner.Creature.CombatState;
            for (int i = 0; i < totalBlindFaith; i++)
            {
                newCards.Add(combatState.CreateCard<BlindFaith>(Owner));
            }

            await CardPileCmd.Add(newCards, PileType.Draw, CardPilePosition.Bottom);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

