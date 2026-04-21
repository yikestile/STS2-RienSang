using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Cards.Curse;

[Pool(typeof(RienSangCardPool))]
public class Fpoon : RienSangCard
{
    public Fpoon() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("SinkingPotency", 20),
        new PowerVar<LCSinkingPower>(4) // Count
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCSinkingPower>()
    ];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            var sinking = await PowerCmd.Apply<LCSinkingPower>(Owner.Creature, DynamicVars[nameof(LCSinkingPower)].IntValue, Owner.Creature, this);
            sinking?.AddPotency((int)DynamicVars["SinkingPotency"].BaseValue);
        }
    }
}
