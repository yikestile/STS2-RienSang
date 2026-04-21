using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using LimbusCore.LimbusCoreCode.Mechanics;
using RienSang.RienSangCode.Mechanics;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch(typeof(CombatState), nameof(CombatState.CurrentSide), MethodType.Setter)]
public static class CaduceusResetPatch
{

    [HarmonyPostfix]
    public static void Postfix(CombatSide value)
    {
        if (value == CombatSide.Player)
        {
            CaduceusManager.ResetTurnCounters();
        }
    }
}