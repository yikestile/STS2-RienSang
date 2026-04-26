using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class Tradeoff : RienSangCard
{
    public Tradeoff() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override bool GainsKarma => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaGain", 5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner);
        if (hand == null || hand.Cards.Count == 0)
        {
            return;
        }

        var upgradableCards = hand.Cards.Where(c => c.IsUpgradable && c != this).ToList();
        if (upgradableCards.Count > 0)
        {
            var selectedCards = await CardSelectCmd.FromHand(
                choiceContext, 
                Owner, 
                new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 0, upgradableCards.Count),
                (CardModel c) => c.IsUpgradable && c != this,
                this
            );

            int upgradedCount = 0;
            if (selectedCards != null)
            {
                foreach (var card in selectedCards)
                {
                    CardCmd.Upgrade(card);
                    upgradedCount++;
                }
            }

            if (upgradedCount > 0)
            {
                decimal karma = DynamicVars["KarmaGain"].BaseValue * upgradedCount;
                await Owner.Creature.ApplyKarma(choiceContext, karma, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars["KarmaGain"].UpgradeValueBy(-1);
    }
}