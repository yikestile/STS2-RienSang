using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Cards.Ancient;
using RienSang.RienSangCode.Cards.Basic;
using RienSang.RienSangCode.Cards.Common;
using RienSang.RienSangCode.Cards.Rare;
using RienSang.RienSangCode.Cards.Uncommon;

namespace RienSang.RienSangCode.Patches;

[HarmonyPatch(typeof(WhisperingEarring), nameof(WhisperingEarring.AfterAutoPrePlayPhaseEnteredLate))]
public static class VakuuExclude
{
    private static bool IsExcluded(CardModel card)
    {

        if (card is RienSangCard rienSangCard && rienSangCard.GainsKarma) return true;

        return card is Tradeoff 
            or AsThePrescriptOrdered 
            or Reconstruct 
            or ByUnpredictableWhim 
            or ByGodsWill 
            or TheWillOfHermes 
            or Obedience 
            or TheOraclesProxy;
    }

    [HarmonyPrefix]
    public static bool Prefix(WhisperingEarring __instance, PlayerChoiceContext choiceContext, Player player, ref Task __result)
    {
        if (player != __instance.Owner) return true;

        __result = RunCustomVakuuLoop(__instance, choiceContext, player);
        return false; 
    }

    private static async Task RunCustomVakuuLoop(WhisperingEarring relic, PlayerChoiceContext choiceContext, Player player)
    {
        ICombatState combatState = player.Creature.CombatState;
        if (combatState.RoundNumber > 1) return;

        relic.Flash();
        bool isMaxed;

        using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
        {
            int cardsPlayed;
            for (cardsPlayed = 0; cardsPlayed < 13; cardsPlayed++)
            {
                if (CombatManager.Instance.IsOverOrEnding || CombatManager.Instance.IsPlayerReadyToEndTurn(player))
                    break;

                CardPile hand = PileType.Hand.GetPile(player);
                
                CardModel? card = hand.Cards.FirstOrDefault(c => c.CanPlay() && !IsExcluded(c));

                if (card == null) break;

                Creature? target = (Creature?)AccessTools.Method(typeof(WhisperingEarring), "GetTarget")
                    .Invoke(relic, new object[] { card, combatState });

                await card.SpendResources();
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
            }
            isMaxed = cardsPlayed >= 13;
            if (cardsPlayed == 0) return;
        }

        LocString line = isMaxed 
            ? new LocString("relics", "WHISPERING_EARRING.warning") 
            : new LocString("relics", "WHISPERING_EARRING.approval");

        TalkCmd.Play(line, player.Creature, VfxColor.Purple);
    }
}