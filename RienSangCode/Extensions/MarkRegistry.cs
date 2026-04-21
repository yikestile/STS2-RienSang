using System.Collections.Generic;
using System.Linq;
using BaseLib.Extensions;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Powers; 

namespace RienSang.RienSangCode.Extensions;

public static class MarkRegistry
{
    public static readonly SpireField<CardModel, bool> IsMarkedField = new SpireField<CardModel, bool>(() => false);

    public static bool HasSingleTurnMark(this CardModel card) => IsMarkedField.Get(card);
    
    public static void GiveSingleTurnMark(this CardModel card) => IsMarkedField.Set(card, true);
    
    public static void ClearSingleTurnMark(this CardModel card) => IsMarkedField.Set(card, false);
}

[HarmonyPatch(typeof(CardModel), "ExtraHoverTips", MethodType.Getter)]
public static class MarkedExtraTipsPatch
{
    public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance.HasSingleTurnMark())
        {
            var markTip = HoverTipFactory.FromPower<MarkofthePrescriptPower>();
            
            if (markTip != null)
            {
                __result = __result.Append(markTip);
            }
        }
    }
}