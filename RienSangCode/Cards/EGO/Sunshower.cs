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
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.EGO;

[Pool(typeof(RienSangCardPool))]
public class Sunshower : RienSangCard
{
    public Sunshower() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        CurrentDamageType = LimbusDamageType.Pierce;
    }

    public override bool IsEgoCard => true;
    public override int SpCost => 35;
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(17, ValueProp.Move),
        new("BonusDamage", 15),
        new PowerVar<LCProtectionPower>(3) 
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCProtectionPower>(),
        HoverTipFactory.FromPower<LCProtectionNextTurn>()
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

        int damage = (int)DynamicVars.Damage.BaseValue + (success ? (int)DynamicVars["BonusDamage"].BaseValue : 0);

        if (Owner.Creature.CombatState != null)
        {
            foreach (var enemy in Owner.Creature.CombatState.Enemies)
            {
                if (enemy.IsAlive) DamageTypeTracker.LastDamageType[enemy] = CurrentDamageType;
            }
            await DamageCmd.Attack(damage).FromCard(this).TargetingAllOpponents(Owner.Creature.CombatState).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        }

        if (Owner.Creature.CombatState != null)
        {
            var teammates = Owner.Creature.CombatState.GetTeammatesOf(Owner.Creature).ToList();
            
            if (!teammates.Contains(Owner.Creature))
            {
                teammates.Add(Owner.Creature);
            }

            int protectionStacks = DynamicVars[nameof(LCProtectionPower)].IntValue;

            foreach (var ally in teammates)
            {
                if (ally == null || !ally.IsAlive) continue;

                await PowerCmd.Apply<LCProtectionPower>(choiceContext, ally, protectionStacks, Owner.Creature, this);
                await PowerCmd.Apply<LCProtectionNextTurn>(choiceContext, ally, protectionStacks, Owner.Creature, this);

                if (success && ally.Player != null)
                {
                    var modelId = ally.Player.Character.Id.Entry;
                    if (modelId.StartsWith("RienSang", System.StringComparison.OrdinalIgnoreCase) || modelId.StartsWith("Limbus", System.StringComparison.OrdinalIgnoreCase))
                    {
                        SanityManager.ModifySanity(ally.Player, 15f);
                    }
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}