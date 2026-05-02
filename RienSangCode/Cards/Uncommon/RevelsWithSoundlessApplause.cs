using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;

namespace RienSang.RienSangCode.Cards.Uncommon;


[Pool(typeof(RienSangCardPool))]
public class RevelWithSoundlessApplause() : RienSangCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new RepeatVar(3)
        
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int repeatCount = base.DynamicVars.Repeat.IntValue;
        decimal baseDamage = base.DynamicVars.Damage.BaseValue;
        var target = cardPlay.Target;

        for (int i = 0; i < repeatCount; i++)
        {
            if (target != null && target.IsAlive)
            {
                await CaduceusManager.Execute(this, target, baseDamage, choiceContext, i, LimbusDamageType.Pierce,
                    0.75f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}