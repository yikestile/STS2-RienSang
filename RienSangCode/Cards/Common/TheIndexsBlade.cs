using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;

namespace RienSang.RienSangCode.Cards.Common;


[Pool(typeof(RienSangCardPool))]
public class TheIndexsBlade() : RienSangCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal baseDamage = base.DynamicVars.Damage.BaseValue;
        var combatState = base.CombatState;

        if (combatState != null)
        {
            var targets = combatState.HittableEnemies;
            foreach (var target in targets)
            {
                if (target != null && target.IsAlive)
                {
                    await CaduceusManager.Execute(this, target, baseDamage, choiceContext, 0); 
                }
            }
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}