using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class UndertakePrescripts : RienSangCard
{
    public UndertakePrescripts() : base(2, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new RepeatVar(2),
        new("KarmaLoss", 5m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = DynamicVars.Repeat.IntValue;

        for (int i = 0; i < count; i++)
        {
            var drawPile = PileType.Draw.GetPile(Owner);
            if (drawPile == null) break;

            var validCards = drawPile.Cards.Where(c => !c.Keywords.Contains(CardKeyword.Unplayable)).ToList();
            var card = validCards.StableShuffle(Owner.RunState.Rng.Shuffle).FirstOrDefault();
            
            if (card == null)
            {
                 card = drawPile.Cards.ToList().StableShuffle(Owner.RunState.Rng.Shuffle).FirstOrDefault();
            }

            if (card != null)
            {
                await CardCmd.AutoPlay(choiceContext, card, null);
            }
        }

        await Owner.Creature.ModifyKarma(choiceContext, -DynamicVars["KarmaLoss"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
        DynamicVars["KarmaLoss"].UpgradeValueBy(2);
    }
}