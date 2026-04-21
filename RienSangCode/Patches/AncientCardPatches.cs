using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using RienSang.RienSangCode.Cards.Ancient;
using RienSang.RienSangCode.Cards.Basic;
using RienSang.RienSangCode.Character;
using System.Collections.Generic;
using System.Linq;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch]
public static class AncientCardPatches
{
    [HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceStarterCard")]
    [HarmonyPostfix]
    public static void ArchaicToothStarterPatch(Player player, ref CardModel? __result)
    {
        if (player?.Deck?.Cards == null) return;
        
        if (__result == null)
        {
            __result = player.Deck.Cards.FirstOrDefault(c => c is ByUnpredictableWhim);
        }
    }

    [HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceTransformedCard")]
    [HarmonyPrefix]
    public static bool ArchaicToothTransformPatch(ArchaicTooth __instance, CardModel starterCard, ref CardModel __result)
    {
        if (starterCard is ByUnpredictableWhim && starterCard.Owner?.RunState != null)
        {
            CardModel cardModel = starterCard.Owner.RunState.CreateCard<ByGodsWill>(starterCard.Owner);
            if (starterCard.IsUpgraded)
            {
                cardModel.UpgradeInternal();
            }
            __result = cardModel;
            return false;
        }
        return true;
    }
    
    [HarmonyPatch(typeof(DustyTome), nameof(DustyTome.SetupForPlayer))]
    [HarmonyPrefix]
    public static bool DustyTomeSetupPatch(DustyTome __instance, Player player)
    {
        if (player?.Character is RienSang.RienSangCode.Character.RienSang)
        {
            __instance.AncientCard = ModelDb.GetId<GloomInRuins>();
            return false;
        }

        return true;
    }
}
