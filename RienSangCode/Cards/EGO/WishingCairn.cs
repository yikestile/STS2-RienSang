using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Cards.EGO;

[Pool(typeof(RienSangCardPool))]
public class WishingCairn : RienSangCard
{
    public WishingCairn() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override bool IsEgoCard => true;
    public override int SpCost => 20;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(22, ValueProp.Move),
        new PowerVar<LCParalyzePower>(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCParalyzePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target != null)
        {
            await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);
            
            int paralyze = DynamicVars[nameof(LCParalyzePower)].IntValue;
            await PowerCmd.Apply<LCParalyzePower>(choiceContext, play.Target, paralyze, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LCParalyzePower)].UpgradeValueBy(1);
    }
}
