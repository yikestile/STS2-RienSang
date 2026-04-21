using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using LimbusCore.LimbusCoreCode.Mechanics;
using RienSang.RienSangCode.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using Godot;
using MegaCrit.Sts2.Core.Hooks;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Runs;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class PanicRegistrationPatch
{
    [HarmonyPostfix]
    public static void Postfix(IRunState runState, CombatState? combatState)
    {
        if (combatState == null) return;

        foreach (var player in combatState.Players)
        {
            var charId = player.Character.Id.Entry;

            bool isRienSang = charId.Contains("RIEN", System.StringComparison.OrdinalIgnoreCase) && 
                              charId.Contains("SANG", System.StringComparison.OrdinalIgnoreCase);

            if (isRienSang)
            {
                ModelDb.Inject(typeof(PainfulScar));
                
                var data = SanityManager.GetData(player);
                data.PanicPowerType = typeof(PainfulScar);
            }
        }
    }
}
