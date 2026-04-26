using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class ThickVapor : RienSangCard
{
    public ThickVapor() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(9, ValueProp.Move),
        new("PotencyCost", 10)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCPoisePower>()
    ];

    protected override bool IsPlayable => Owner?.Creature?.GetPower<LCPoisePower>()?.CanConsumePotency(DynamicVars["PotencyCost"].IntValue) ?? false;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var poise = Owner.Creature.GetPower<LCPoisePower>();
        if (poise != null)
        {
            await poise.ConsumePotency(DynamicVars["PotencyCost"].IntValue);
        }

        await CommonActions.CardBlock(this, cardPlay);

        await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}