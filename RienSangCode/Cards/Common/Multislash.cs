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
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class Multislash : RienSangCard
{
    public Multislash() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2, ValueProp.Move),
        new RepeatVar(2),
        new("DrawAmount", 1),
        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Unlock>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int repeatCount = DynamicVars.Repeat.IntValue;
        decimal baseDamage = DynamicVars.Damage.BaseValue;
        var target = cardPlay.Target;

        for (int i = 0; i < repeatCount; i++)
        {
            if (target != null && target.IsAlive)
            {
                await CaduceusManager.Execute(this, target, baseDamage, choiceContext, i);
            }
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawAmount"].BaseValue, Owner);

        var unlockPower = Owner.Creature.GetPower<Unlock>();
        if (unlockPower != null && unlockPower.Amount >= 3)
        {
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);

        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["DrawAmount"].UpgradeValueBy(1);
    }
}
