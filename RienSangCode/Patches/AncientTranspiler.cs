using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using RienSang.RienSangCode.Cards;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Linq;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using System;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch]
public static class AncientTranspiler
{
    public static bool IsAncientOrEgo(CardModel model)
    {
        if (model == null) return false;
        if (model.Rarity == CardRarity.Ancient) return true;
        return model is RienSangCard rienCard && rienCard.IsEgoCard;
    }

    [HarmonyPatch(typeof(NCard), "Reload")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var getRarityMethod = typeof(CardModel).GetProperty(nameof(CardModel.Rarity))?.GetGetMethod();
        var helperMethod = typeof(AncientTranspiler).GetMethod(nameof(IsAncientOrEgo));
        int ancientValue = (int)CardRarity.Ancient;

        int patchCount = 0;

        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].Calls(getRarityMethod))
            {
                if (i + 1 < codes.Count && codes[i + 1].LoadsConstant(ancientValue))
                {
                    if (i + 2 < codes.Count)
                    {
                        var nextOp = codes[i + 2].opcode;
                        
                        if (nextOp == OpCodes.Ceq)
                        {
                            codes[i] = new CodeInstruction(OpCodes.Call, helperMethod);
                            codes.RemoveRange(i + 1, 2);
                            patchCount++;
                        }
                        else if (nextOp == OpCodes.Bne_Un || nextOp == OpCodes.Bne_Un_S)
                        {
                            codes[i] = new CodeInstruction(OpCodes.Call, helperMethod);
                            codes.RemoveAt(i + 1);
                            codes[i + 1].opcode = OpCodes.Brfalse; 
                            patchCount++;
                        }
                        else if (nextOp == OpCodes.Beq || nextOp == OpCodes.Beq_S)
                        {
                            codes[i] = new CodeInstruction(OpCodes.Call, helperMethod);
                            codes.RemoveAt(i + 1);
                            codes[i + 1].opcode = OpCodes.Brtrue;
                            patchCount++;
                        }
                    }
                }
            }
        }
        return codes.AsEnumerable();
    }

    [HarmonyPatch(typeof(CardModel), "AncientTextBgPath", MethodType.Getter)]
    [HarmonyPrefix]
    public static bool FixAncientTextBgPath(CardModel __instance, ref string __result)
    {
        if (__instance is RienSangCard { IsEgoCard: true } rienCard)
        {
            CardType cardType = rienCard.Type switch
            {
                CardType.Attack => CardType.Attack,
                CardType.Skill => CardType.Skill,
                CardType.Power => CardType.Power,
                CardType.Quest => CardType.Quest,
                _ => CardType.Skill
            };
            __result = ImageHelper.GetImagePath("atlases/compressed.sprites/card_template/ancient_card_text_bg_" + cardType.ToString().ToLowerInvariant() + ".tres");
            return false;
        }
        return true;
    }
}