using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace RienSang.RienSangCode.Patches;

public static class EnforcingPrescriptTracker
{
    public static readonly HashSet<Creature> ModifiedCreatures = new();
}

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
    typeof(PlayerChoiceContext), 
    typeof(IEnumerable<Creature>), 
    typeof(decimal), 
    typeof(ValueProp), 
    typeof(Creature), 
    typeof(CardModel)
})]
public static class EnforcingPrescriptDamagePatch
{
    private static bool _isDuplicating = false;

    [HarmonyPrefix]
    public static bool Prefix(
        PlayerChoiceContext choiceContext, 
        IEnumerable<Creature> targets, 
        decimal amount, 
        ValueProp props, 
        Creature? dealer, 
        CardModel? cardSource,
        ref Task<IEnumerable<DamageResult>> __result)
    {
        if (_isDuplicating) return true;
        if (dealer == null) return true;

        if (EnforcingPrescriptTracker.ModifiedCreatures.Contains(dealer))
        {
            _isDuplicating = true;
            
            decimal str = dealer.GetPower<StrengthPower>()?.Amount ?? 0m;
            decimal expectedModified = amount + str;
            decimal targetPerHit = expectedModified / 2m;
            decimal newBase = targetPerHit - str;
            
            int finalNewBase = Math.Max(0, (int)newBase);
            if (targetPerHit > 0 && finalNewBase <= 0) 
            {
                finalNewBase = 1;
            }

            __result = RunDuplicatedDamage(choiceContext, targets, (decimal)finalNewBase, props, dealer, cardSource);
            return false; 
        }
        
        return true;
    }

    private static async Task<IEnumerable<DamageResult>> RunDuplicatedDamage(
        PlayerChoiceContext choiceContext, 
        IEnumerable<Creature> targets, 
        decimal amount, 
        ValueProp props, 
        Creature? dealer, 
        CardModel? cardSource)
    {
        try 
        {
            var combinedResults = new List<DamageResult>();

            var r1 = await CreatureCmd.Damage(choiceContext, targets, amount, props, dealer, cardSource);
            if (r1 != null) combinedResults.AddRange(r1);
            
            var r2 = await CreatureCmd.Damage(choiceContext, targets, amount, props, dealer, cardSource);
            if (r2 != null) combinedResults.AddRange(r2);
            
            return combinedResults;
        } 
        finally 
        {
            _isDuplicating = false;
        }
    }
}