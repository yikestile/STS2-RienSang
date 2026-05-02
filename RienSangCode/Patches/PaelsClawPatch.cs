using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using RienSang.RienSangCode.Cards.Basic; // Import your DefendRienSang card
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch(typeof(PaelsClaw), nameof(PaelsClaw.AfterObtained))]
public static class PaelsClawPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PaelsClaw __instance)
    {
        Task.Run(async () =>
        {
            if (__instance.Owner?.Creature == null) return;

            var deck = PileType.Deck.GetPile(__instance.Owner).Cards.ToList();
            foreach (CardModel card in deck)
            {
                if (card is DefendRienSang)
                {
                    CardCmd.Enchant<Goopy>(card, 1m);
                    NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(NCardEnchantVfx.Create(card));
                }
            }
        });

        return false;
    }
}