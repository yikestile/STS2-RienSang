using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class ToWhereThePrescriptPoints : RienSangCard
{
    public ToWhereThePrescriptPoints() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("FragileAmount", 1),
        new DamageVar(8, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RienSangKeywords.Caduceus
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        decimal baseDamage = DynamicVars.Damage.BaseValue;

        if (target != null && target.IsAlive)
        {
            await CaduceusManager.Execute(this, target, baseDamage, choiceContext, 0); 
            await PowerCmd.Apply<LCFragileNextTurn>(choiceContext, target, (int)DynamicVars["FragileAmount"].BaseValue, Owner.Creature, this);
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["FragileAmount"].UpgradeValueBy(1);
    }
}