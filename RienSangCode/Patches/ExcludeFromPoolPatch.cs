using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Cards.Rare;
using RienSang.RienSangCode.Cards;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch]
public static class ExcludeFromPoolPatch
{
    [HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards))]
    [HarmonyPostfix]
    public static void PostfixGlobal(ref IEnumerable<CardModel> __result)
    {
        __result = __result.Where(c => c is not UnlockII and not UnlockIII);
    }

    [HarmonyPatch(typeof(CardCreationOptions), nameof(CardCreationOptions.GetPossibleCards))]
    [HarmonyPostfix]
    public static void PostfixPerPlayerRewards(Player player, ref IEnumerable<CardModel> __result)
    {
        FilterEgoCards(player, ref __result);
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyMerchantCardPool))]
    [HarmonyPostfix]
    public static void PostfixPerPlayerMerchant(Player player, ref IEnumerable<CardModel> __result)
    {
        FilterEgoCards(player, ref __result);
    }

    private static void FilterEgoCards(Player player, ref IEnumerable<CardModel> cards)
    {
        if (player == null || player.Deck == null || cards == null) return;

        var existingEgoIds = player.Deck.Cards
            .OfType<RienSangCard>()
            .Where(c => c.IsEgoCard)
            .Select(c => c.Id)
            .ToHashSet();

        if (existingEgoIds.Count > 0)
        {
            cards = cards.Where(c =>
                !(c is RienSangCard { IsEgoCard: true } ego) || !existingEgoIds.Contains(ego.Id)
            );
        }
    }
}
