using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using RienSang.RienSangCode.Relics;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), "GetUpgradedStarterRelic")]
internal static class RienSangTouchOfOrobasPatch
{
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is PrescriptDevice)
        {
            __result = ModelDb.Relic<OraclesPrescript>().ToMutable();
        }
    }
}
