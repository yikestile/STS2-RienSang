using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;


namespace RienSang.RienSangCode.Cards.Basic;
[Pool(typeof(RienSangCardPool))]
public class FollowingThePrescript : RienSangCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move)
    ];
    public FollowingThePrescript() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RienSangKeywords.Caduceus
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            await CaduceusManager.Execute(this, cardPlay.Target, DynamicVars.Damage.BaseValue, choiceContext, 0);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}