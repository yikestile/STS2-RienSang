using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
public static class PreviewPatch
{
    public static bool IsPreviewCalculation { get; private set; } = false;

    [HarmonyPrefix]
    public static void Prefix(CardPreviewMode previewMode)
    {
        if (previewMode != CardPreviewMode.None)
        {
            IsPreviewCalculation = true;
        }
    }

    [HarmonyPostfix]
    public static void Postfix()
    {
        IsPreviewCalculation = false;
    }
}