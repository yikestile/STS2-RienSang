using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using System.Linq;

namespace RienSang.RienSangCode.Relics;

[Pool(typeof(RienSangRelicPool))]
public class ProvidenceOfThePrescript : RienSangRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        Flash();
        var rng = player.RunState.Rng.Niche;
        if (rng.NextFloat() < 0.5f)
        {
            await LCPoisePower.Apply(choiceContext, player.Creature, 1, 3, player.Creature, null);
        }
        else
        {
            await LCPoisePower.Apply(choiceContext, player.Creature, 2, 1, player.Creature, null);
        }
    }

    [HarmonyPatch(typeof(LCPoisePower), nameof(LCPoisePower.ModifyDamageMultiplicative))]
    public static class PoiseCritBoostPatch
    {
        [HarmonyPostfix]
        public static void Postfix(LCPoisePower __instance, ref decimal __result)
        {
            if (__result > 1m && __instance.Owner?.Player != null)
            {
                var player = __instance.Owner.Player;
                if (player.Relics.Any(r => r is ProvidenceOfThePrescript) && __instance.Owner.CombatState?.RoundNumber >= 4)
                {
                    __result = 1.35m;
                }
            }
        }
    }
}