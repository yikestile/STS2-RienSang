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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class BindingChain : RienSangCard
{
    public BindingChain() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCEvadePower>(2),
        new("StrLoss", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Unlock];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCEvadePower>(),
        HoverTipFactory.FromPower<Unlock>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<LCEvadePower>(Owner.Creature, DynamicVars[nameof(LCEvadePower)].IntValue, Owner.Creature, this);

        var target = cardPlay.Target;
        if (target != null && target.IsAlive)
        {
            int strLoss = (int)DynamicVars["StrLoss"].BaseValue;
            var unlockPower = Owner.Creature.GetPower<Unlock>();
            if (unlockPower != null && unlockPower.Amount >= 3)
            {
                strLoss += 2;
            }
            await PowerCmd.Apply<StrengthPower>(target, -strLoss, Owner.Creature, this);
            await PowerCmd.Apply<LCStrengthNextTurn>(target, strLoss, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LCEvadePower)].UpgradeValueBy(2);
        DynamicVars["StrLoss"].UpgradeValueBy(1);
    }
}
