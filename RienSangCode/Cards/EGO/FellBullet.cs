using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.EGO;

[Pool(typeof(RienSangCardPool))]
public class FellBullet : RienSangCard
{
    public FellBullet() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    public override bool IsEgoCard => true;
    public override int SpCost => 25;
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar("AllyDamage", 5, ValueProp.Move), 
        new DamageVar("AllyHighDamage", 10, ValueProp.Move), 
        new DamageVar("EnemyDamage", 15, ValueProp.Move),
        new DamageVar("EnemyHighDamage", 30, ValueProp.Move),
        new("BleedPotency", 3),
        new PowerVar<LCBleedPower>(1),
        new("PoisePotency", 3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCBleedPower>(),
        HoverTipFactory.FromPower<LCPoisePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        float currentSP = SanityManager.GetSanity(Owner);
        float chance = 50f + currentSP;
        bool success = false;

        if (Owner.Creature.CombatState != null)
        {
            float roll = Owner.Creature.CombatState.RunState.Rng.Niche.NextFloat() * 100f;
            success = roll < chance;
        }

        int allyDamage = (int)(success ? DynamicVars["AllyHighDamage"].BaseValue : DynamicVars["AllyDamage"].BaseValue);
        int enemyDamage = (int)(success ? DynamicVars["EnemyHighDamage"].BaseValue : DynamicVars["EnemyDamage"].BaseValue);

        if (play.Target != null)
        {
            await CreatureCmd.Damage(choiceContext, play.Target, allyDamage, ValueProp.Unpowered, Owner.Creature, this);
        }

        if (Owner.Creature.CombatState != null)
        {
            var enemies = Owner.Creature.CombatState.Enemies.Where(e => e.IsAlive).ToList();
            foreach (var enemy in enemies)
            {
                bool shouldTriggerFatal = enemy.Powers.All((PowerModel p) => p.ShouldPowerBeRemovedAfterOwnerDeath() || MegaCrit.Sts2.Core.Hooks.Hook.ShouldPowerBeRemovedOnDeath(p));

                AttackCommand attackCommand = await DamageCmd.Attack(enemyDamage).FromCard(this).Targeting(enemy).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

                if (shouldTriggerFatal && attackCommand.Results.Any((DamageResult r) => r.WasTargetKilled))
                {
                    var poise = await PowerCmd.Apply<LCPoisePower>(Owner.Creature, 1, Owner.Creature, this);
                    poise?.AddPotency((int)DynamicVars["PoisePotency"].BaseValue);
                }

                var bleed = await PowerCmd.Apply<LCBleedPower>(enemy, (int)DynamicVars[nameof(LCBleedPower)].BaseValue, Owner.Creature, this);
                bleed?.AddPotency((int)DynamicVars["BleedPotency"].BaseValue);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AllyDamage"].UpgradeValueBy(-2); 
        DynamicVars["AllyHighDamage"].UpgradeValueBy(-4); 
        DynamicVars["EnemyDamage"].UpgradeValueBy(5); 
    }
}
