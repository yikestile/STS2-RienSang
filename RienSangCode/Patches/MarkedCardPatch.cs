using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using RienSang.RienSangCode.Extensions;
using System.Reflection;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Linq;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch]
public static class MarkedCardPatch
{
    private static readonly string _markIconPath = "res://RienSang/images/ui/combat/mark_icon.png";

    [HarmonyPatch(typeof(CardModel), "ShouldGlowGoldInternal", MethodType.Getter)]
    [HarmonyPostfix]
    public static void AddMarkedGlow(CardModel __instance, ref bool __result)
    {
        if (__instance.HasSingleTurnMark())
        {
            __result = true;
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.EndOfTurnCleanup))]
    [HarmonyPostfix]
    public static void ClearMarkOnTurnEnd(CardModel __instance)
    {
        if (__instance.HasSingleTurnMark())
        {
            __instance.ClearSingleTurnMark();
        }
    }
    
    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
    [HarmonyPostfix]
    public static void AddMarkIcon(NCard __instance)
    {
        if (__instance.Model == null) return;

        bool isMarked = __instance.Model.HasSingleTurnMark();
    
        var energyIcon = __instance.GetNodeOrNull<TextureRect>("%EnergyIcon") ?? 
                         __instance.GetChildren().OfType<TextureRect>().FirstOrDefault(c => c.Name.ToString().Contains("Energy"));

        if (energyIcon == null) return;
    
        var markNode = __instance.GetNodeOrNull<TextureRect>("PrescriptMark");

        if (isMarked)
        {
            if (markNode == null)
            {
                var texture = ResourceLoader.Load<Texture2D>(_markIconPath);
                if (texture == null) return;

                markNode = new TextureRect
                {
                    Name = "PrescriptMark",
                    Texture = texture,
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                    StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                    MouseFilter = Control.MouseFilterEnum.Ignore, 
                    Visible = false
                };
            
                __instance.AddChild(markNode);
            
                markNode.CustomMinimumSize = new Vector2(120, 120);
                markNode.Size = markNode.CustomMinimumSize;
            }
        
            float moveRight = 3f; 
            float moveUp = 5f;
        
            Vector2 offset = new Vector2(moveRight, -moveUp);

            markNode.Position = energyIcon.Position + (energyIcon.Size / 2f) - (markNode.Size / 2f) + offset;

            if (!markNode.Visible)
            {
                markNode.Visible = true;
                StartBlinkingEffect(markNode);
            }
        }
        else if (markNode != null && markNode.Visible)
        {
            markNode.Visible = false;
            StopBlinkingEffect(markNode);
        }
    }

    private static void StartBlinkingEffect(TextureRect icon)
    {
        StopBlinkingEffect(icon);

        var tween = icon.CreateTween().SetLoops();
    
        tween.TweenProperty(icon, "modulate:a", 0.6f, 1f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
         
        tween.TweenProperty(icon, "modulate:a", 1.0f, 1f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        icon.SetMeta("BlinkTween", tween);
    }

    private static void StopBlinkingEffect(TextureRect icon)
    {
        if (icon.HasMeta("BlinkTween"))
        {
            var tween = icon.GetMeta("BlinkTween").As<Tween>();
            if (tween != null && tween.IsValid())
            {
                tween.Kill();
            }
            icon.RemoveMeta("BlinkTween");
        }

        icon.Modulate = new Color(1, 1, 1, 1);
    }
}