using System.Collections.Generic;
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
public class Belief : RienSangCard
{
    public Belief() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(7, ValueProp.Move),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Block.UpgradeValueBy(1);
    }
}