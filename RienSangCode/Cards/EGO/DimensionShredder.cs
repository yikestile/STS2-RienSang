using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions; // Added for TakeRandom
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Cards.EGO;

[Pool(typeof(RienSangCardPool))]
public class DimensionShredder : RienSangCard
{
    public DimensionShredder() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
        CurrentDamageType = LimbusDamageType.Pierce;
    }

    public override bool IsEgoCard => true;
    public override int SpCost => 25;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(20, ValueProp.Move),
        new("BonusDamage", 20)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        float currentSP = SanityManager.GetSanity(Owner);
        float chance = 50f + currentSP;
        
        int damage = (int)DynamicVars.Damage.BaseValue;
        
        if (Owner.Creature.CombatState != null)
        {
            float roll = Owner.Creature.CombatState.RunState.Rng.Niche.NextFloat() * 100f;
            if (roll < chance)
            {
                damage += (int)DynamicVars["BonusDamage"].BaseValue;
            }
        }

        if (Owner.Creature.CombatState != null)
        {
            var potentialTargets = Owner.Creature.CombatState.HittableEnemies;
            if (potentialTargets.Count > 0)
            {
                var target = potentialTargets.TakeRandom(1, Owner.RunState.Rng.CombatTargets).FirstOrDefault();
                if (target != null)
                {
                    DamageTypeTracker.LastDamageType[target] = CurrentDamageType;
                    await DamageCmd.Attack(damage).FromCard(this).Targeting(target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10);
    }
}