using BaseLib.Abstracts;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Extensions;
using System;

namespace RienSang.RienSangCode.Character;

public class RienSangCardPool : CustomCardPoolModel
{
    public override string Title => RienSang.CharacterId; 

    public override float H => 1f; 
    public override float S => 1f; 
    public override float V => 1f; 
    
    public override Texture2D CustomFrame(CustomCardModel cardModel)
    {
        return PreloadManager.Cache.GetTexture2D("card_portraits/skill_frame.png".ImagePath());
    }
    
    public override Color DeckEntryCardColor => new("31c2ee");

    public override bool IsColorless => false;
    
    public override string? BigEnergyIconPath => "res://RienSang/images/ui/combat/limbus_energy_icon.png";
    public override string? TextEnergyIconPath => "res://RienSang/images/ui/combat/text_limbus_energy_icon.png";

    [HarmonyPatch(typeof(CardModel), "PortraitBorderPath", MethodType.Getter)]
    public static class ForceSkillBorderShapePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CardModel __instance, ref string __result)
        {
            if (__instance.Pool is RienSangCardPool)
            {
                if (__instance.Type == CardType.Attack || __instance.Type == CardType.Power)
                {
                    __result = ImageHelper.GetImagePath("atlases/ui_atlas.sprites/card/card_portrait_border_skill_s.tres");
                    return false;
                }
            }
            return true;
        }
    }
}