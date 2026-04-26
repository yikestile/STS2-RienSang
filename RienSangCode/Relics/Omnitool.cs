using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Cards;

namespace RienSang.RienSangCode.Relics;

[Pool(typeof(RienSangRelicPool))]
public class Omnitool : RienSangRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult results, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == Owner.Creature && cardSource is RienSangCard { CurrentDamageType: not LimbusDamageType.None } rienSangCard)
        {
            if (results.UnblockedDamage > 0)
            {
                IncrementCounter(target, rienSangCard.CurrentDamageType);
            }
        }
        await Task.CompletedTask;
    }

    private void IncrementCounter(Creature target, LimbusDamageType type)
    {
        switch (type)
        {
            case LimbusDamageType.Slash:
                DamageTypeTracker.SlashHitCount[target]++;
                if (DamageTypeTracker.SlashHitCount[target] % 5 == 0 && DamageTypeTracker.SlashFragilityReward[target] < 3)
                {
                    DamageTypeTracker.SlashFragilityReward[target]++;
                    Flash();
                }
                break;
            case LimbusDamageType.Blunt:
                DamageTypeTracker.BluntHitCount[target]++;
                if (DamageTypeTracker.BluntHitCount[target] % 5 == 0 && DamageTypeTracker.BluntFragilityReward[target] < 3)
                {
                    DamageTypeTracker.BluntFragilityReward[target]++;
                    Flash();
                }
                break;
            case LimbusDamageType.Pierce:
                DamageTypeTracker.PierceHitCount[target]++;
                if (DamageTypeTracker.PierceHitCount[target] % 5 == 0 && DamageTypeTracker.PierceFragilityReward[target] < 3)
                {
                    DamageTypeTracker.PierceFragilityReward[target]++;
                    Flash();
                }
                break;
        }
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            bool anyApplied = false;
            foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive))
            {
                int slash = DamageTypeTracker.SlashFragilityReward[enemy];
                int blunt = DamageTypeTracker.BluntFragilityReward[enemy];
                int pierce = DamageTypeTracker.PierceFragilityReward[enemy];

                if (slash > 0)
                {
                    await PowerCmd.Apply<LCSlashFragility>(choiceContext, enemy, (decimal)slash, Owner.Creature, null);
                    anyApplied = true;
                }
                if (blunt > 0)
                {
                    await PowerCmd.Apply<LCBluntFragility>(choiceContext, enemy, (decimal)blunt, Owner.Creature, null);
                    anyApplied = true;
                }
                if (pierce > 0)
                {
                    await PowerCmd.Apply<LCPierceFragility>(choiceContext, enemy, (decimal)pierce, Owner.Creature, null);
                    anyApplied = true;
                }
            }
            if (anyApplied) Flash();
        }
    }
}