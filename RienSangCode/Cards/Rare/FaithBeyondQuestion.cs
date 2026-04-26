using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class FaithBeyondQuestion : RienSangCard
{
    public FaithBeyondQuestion() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) 
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, RienSangKeywords.Caduceus];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FaithBeyondQuestionPower.Apply(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
