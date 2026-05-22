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

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class DeepBreath : RienSangCard
{
    public DeepBreath() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2),
        new("DrawAmount", 3),
        new("PotencyCost", 20)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCPoisePower>()
    ];

    protected override bool IsPlayable => Owner?.Creature?.GetPower<LCPoisePower>()?.CanConsumePotency(IsUpgraded ? 10 : 20) ?? false;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var poise = Owner.Creature.GetPower<LCPoisePower>();
        if (poise != null)
        {
            await poise.ConsumePotency(IsUpgraded ? 10 : 20);
        }

        await PlayerCmd.GainEnergy(2, Owner);
        await CardPileCmd.Draw(choiceContext, 3, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PotencyCost"].UpgradeValueBy(-10);
    }
}