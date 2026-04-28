using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Cards.EGO;

[Pool(typeof(RienSangCardPool))]
public class FourthMatchFlame : RienSangCard
{
    public FourthMatchFlame() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        CurrentDamageType = LimbusDamageType.Slash;
    }

    public override bool IsEgoCard => true;
    public override int SpCost => 20;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new PowerVar<LCBurnPower>(2),
        new("BurnPotency", 4)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCBurnPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (Owner.Creature.CombatState == null) return;

        var enemies = Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in enemies)
        {
            DamageTypeTracker.LastDamageType[enemy] = CurrentDamageType;
        }

        await DamageCmd.Attack((int)DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(Owner.Creature.CombatState).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

        int potency = (int)DynamicVars["BurnPotency"].BaseValue;
        int count = (int)DynamicVars[nameof(LCBurnPower)].BaseValue;

        foreach (var enemy in enemies)
        {
            await LCBurnPower.Apply(choiceContext, enemy, count, potency, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BurnPotency"].UpgradeValueBy(6);
        DynamicVars[nameof(LCBurnPower)].UpgradeValueBy(1);
    }
}