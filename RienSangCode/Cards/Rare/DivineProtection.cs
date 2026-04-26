using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class DivineProtection : RienSangCard
{
    public DivineProtection() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmicConsequence", 15m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>(),
        HoverTipFactory.FromPower<DivineProtectionPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Gain 15(10) Karmic Consequence
        await Owner.Creature.ApplyKarma(choiceContext, DynamicVars["KarmicConsequence"].BaseValue, Owner.Creature, this);

        // Double Block and Evade gained this turn
        await PowerCmd.Apply<DivineProtectionPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KarmicConsequence"].UpgradeValueBy(-5);
    }
}