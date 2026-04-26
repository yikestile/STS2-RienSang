using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Localization;
using RienSang.RienSangCode.Cards.Uncommon;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch]
public static class IntentModPatch
{
    public static readonly SpireField<Creature, bool> PendingRandomize = new SpireField<Creature, bool>(() => false);

    [HarmonyPatch(typeof(MonsterMoveStateMachine), nameof(MonsterMoveStateMachine.RollMove))]
    [HarmonyPostfix]
    public static void Postfix(MonsterMoveStateMachine __instance, ref MoveState __result, IEnumerable<Creature> targets, Creature owner, Rng rng)
    {
        if (PendingRandomize[owner])
        {
            PendingRandomize[owner] = false;
        
            var currentId = __result.Id;

            var validStates = __instance.States.Values.OfType<MoveState>().ToList();
        
            if (validStates.Count > 1)
            {
                validStates.RemoveAll(s => s.Id == currentId);
            }

            if (validStates.Count > 0)
            {
                var newState = validStates[rng.NextInt(validStates.Count)];
            
                __result = newState;
                __instance.ForceCurrentState(newState);
            }
        }
    }
}
