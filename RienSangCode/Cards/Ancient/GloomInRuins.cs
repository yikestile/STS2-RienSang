using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Cards.Ancient;

[Pool(typeof(RienSangCardPool))]
public class GloomInRuins : RienSangCard
{
    public GloomInRuins() : base(1, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCGloomingPower>(1),
        new DynamicVar("SinkingDamage", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCGloomingPower>(),
        HoverTipFactory.FromPower<LCSinkingPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<LCGloomingPower>(choiceContext, Owner.Creature, DynamicVars[nameof(LCGloomingPower)].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}
