using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class Atonement : RienSangCard
{
    public Atonement() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("Potency", 5)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<AtonementPower>(),
        HoverTipFactory.FromPower<KarmicConsequence>(),
        HoverTipFactory.FromPower<MarkofthePrescriptPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int gain = IsUpgraded ? 2 : 1;
        await PowerCmd.Apply<AtonementPower>(choiceContext, Owner.Creature, (decimal)gain, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Potency"].UpgradeValueBy(5);
    }
}