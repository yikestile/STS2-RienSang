using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Cards.EGO;

[Pool(typeof(RienSangCardPool))]
public class CrowsEyeView : RienSangCard
{
    public CrowsEyeView() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    public override bool IsEgoCard => true;
    public override int SpCost => 10;

    protected override IEnumerable<DynamicVar> CanonicalVars => [ 
        new DamageVar(10, ValueProp.Move),
        new PowerVar<WeakPower>(1m),
        new PowerVar<StrengthPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        GD.Print($"[LimbusCore] Crow's Eye View played. Current SP: {SanityManager.GetSanity(Owner)}");

        await CommonActions.CardAttack(this, play.Target).Execute(choiceContext);

        if (play.Target != null)
        {
            await PowerCmd.Apply<WeakPower>(play.Target, 1m, Owner.Creature, this);
            await PowerCmd.Apply<StrengthPower>(play.Target, -1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
