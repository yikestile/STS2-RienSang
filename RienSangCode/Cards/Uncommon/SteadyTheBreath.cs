using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class SteadyTheBreath : RienSangCard
{
    public SteadyTheBreath() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCProtectionPower>(2),
        new("PotencyCost", 20m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCPoisePower>(),
        HoverTipFactory.FromPower<LCProtectionPower>()
    ];

    protected override bool IsPlayable => Owner?.Creature?.GetPower<LCPoisePower>()?.CanConsumePotency((int)DynamicVars["PotencyCost"].BaseValue) ?? false;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var poise = Owner.Creature.GetPower<LCPoisePower>();
        if (poise != null)
        {
            await poise.ConsumePotency((int)DynamicVars["PotencyCost"].BaseValue);
        }

        int amount = DynamicVars[nameof(LCProtectionPower)].IntValue;
        await PowerCmd.Apply<LCProtectionPower>(choiceContext, Owner.Creature, (decimal)amount, Owner.Creature, this);
        await PowerCmd.Apply<LCProtectionNextTurn>(choiceContext, Owner.Creature, (decimal)amount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PotencyCost"].UpgradeValueBy(-10);
    }
}