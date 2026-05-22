using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class SanguinePointillism : RienSangCard
{
    public SanguinePointillism() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move),
        new("Chance", 20),
        new("BonusChance", 10)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RienSangKeywords.Caduceus
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        int extraHits = 0;

        if (target != null && target.IsAlive)
        {
            float chanceVal = (float)DynamicVars["Chance"].BaseValue;
            float bonusVal = (float)DynamicVars["BonusChance"].BaseValue;
            
            int debuffCount = target.Powers.Count(p => p.Type == PowerType.Debuff);
            float totalChance = chanceVal + (bonusVal * debuffCount);
            
            var rng = Owner.RunState.Rng.Niche;
            
            if (rng.NextFloat() * 100 < totalChance)
            {
                extraHits++;
                if (rng.NextFloat() * 100 < totalChance)
                {
                    extraHits++;
                }
            }
        }
        
        for (int i = 0; i < 1 + extraHits; i++)
        {
            if (target != null && target.IsAlive)
            {
                await CaduceusManager.Execute(this, target, DynamicVars.Damage.BaseValue, choiceContext, i);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["Chance"].UpgradeValueBy(15);
        DynamicVars["BonusChance"].UpgradeValueBy(10);
    }
}