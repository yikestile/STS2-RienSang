using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;
using System.Reflection;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch]
public static class TargetIconPatch
{
    private static readonly string _targetIconPath = "res://RienSang/images/ui/combat/prescript_target_icon.png";

    [HarmonyPatch(typeof(NCreature), "_Ready")]
    [HarmonyPostfix]
    public static void AddTargetIcon(NCreature __instance)
    {
        if (__instance.Entity == null) return;

        var iconNode = new TextureRect
        {
            Name = "PrescriptTargetIcon",
            Texture = ResourceLoader.Load<Texture2D>(_targetIconPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Visible = false
        };

        __instance.AddChild(iconNode);
        
        UpdateIconVisibility(__instance);
    }

    [HarmonyPatch(typeof(NCreature), "UpdateBounds", typeof(Node))]
    [HarmonyPostfix]
    public static void UpdateIconPosition(NCreature __instance)
    {
        var iconNode = __instance.GetNodeOrNull<TextureRect>("PrescriptTargetIcon");
        if (iconNode == null || !iconNode.Visible) return;

        var hitbox = __instance.Hitbox;
        if (hitbox != null)
        {
            float size = Mathf.Min(hitbox.Size.X, hitbox.Size.Y) * 0.5f;
            iconNode.Size = new Vector2(size, size);
            iconNode.CustomMinimumSize = iconNode.Size;

            iconNode.Position = hitbox.Position + (hitbox.Size / 2f) - (iconNode.Size / 2f);
            iconNode.ZIndex = 2;
        }
    }

    [HarmonyPatch(typeof(Creature), "ApplyPowerInternal")]
    [HarmonyPostfix]
    public static void OnPowerApplied(Creature __instance)
    {
        RefreshCreatureNode(__instance);
    }

    [HarmonyPatch(typeof(Creature), "RemovePowerInternal")]
    [HarmonyPostfix]
    public static void OnPowerRemoved(Creature __instance)
    {
        RefreshCreatureNode(__instance);
    }

    private static void RefreshCreatureNode(Creature entity)
    {
        if (NCombatRoom.Instance == null) return;
        var node = NCombatRoom.Instance.GetCreatureNode(entity);
        if (node != null)
        {
            UpdateIconVisibility(node);
        }
    }

    private static void UpdateIconVisibility(NCreature node)
    {
        var iconNode = node.GetNodeOrNull<TextureRect>("PrescriptTargetIcon");
        if (iconNode == null) return;

        bool hasTargetPower = node.Entity.Powers.Any(p => p is ThePrescriptsTarget);
    
        if (hasTargetPower && !iconNode.Visible)
        {
            iconNode.Visible = true;
            StartBlinkingEffect(iconNode);
        }
        else if (!hasTargetPower && iconNode.Visible)
        {
            iconNode.Visible = false;
            StopBlinkingEffect(iconNode);
        }
    
        if (hasTargetPower)
        {
            UpdateIconPosition(node);
        }
    }
    private static void StartBlinkingEffect(TextureRect icon)
    {
        StopBlinkingEffect(icon);
        var tween = icon.CreateTween().SetLoops();
    
        tween.TweenProperty(icon, "modulate:a", 0.4f, 1.2f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
         
        tween.TweenProperty(icon, "modulate:a", 1.0f, 1.2f)
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