using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class BlindFaith : RienSangCard
{
    public BlindFaith() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2, ValueProp.Move), 
        new("CopyAmount", 1)
    ];
    
    public CardModel GetClone() => CreateClone();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus];

    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { RienSangTags.BlindFaith };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target != null)
        {
            await CaduceusManager.Execute(this, play.Target, DynamicVars.Damage.BaseValue, choiceContext, 0);
        }

        var drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile != null)
        {
            var copies = drawPile.Cards.Where(c => c is BlindFaith).ToList(); 
            
            foreach (var copy in copies)
            {
                await CardPileCmd.Add(copy, PileType.Hand);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["CopyAmount"].UpgradeValueBy(1);
    }
    
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeSideTurnStart))]
    public static class BlindFaithStartPatch 
    {
        [HarmonyPostfix]
        public static void Postfix(ICombatState combatState, CombatSide side)
        {
            if (side != CombatSide.Player) return;
            if (combatState.RoundNumber != 1) return; 

            foreach (var player in combatState.Players)
            {
                var drawPile = PileType.Draw.GetPile(player);
                if (drawPile == null) continue;

                var blindFaiths = drawPile.Cards.Where(c => c is BlindFaith).ToList();
                
                if (blindFaiths.Count > 0)
                {
                    var newCards = new List<CardModel>();
                    
                    foreach (var bf in blindFaiths)
                    {
                        if (bf is BlindFaith blindFaithCard)
                        {
                            int count = bf.DynamicVars["CopyAmount"].IntValue;
                            for (int i = 0; i < count; i++)
                            {
                                 var copy = blindFaithCard.GetClone();
                                 newCards.Add(copy);
                            }
                        }
                    }
                    
                    if (newCards.Count > 0)
                    {
                        TaskHelper.RunSafely(CardPileCmd.AddGeneratedCardsToCombat(newCards, PileType.Draw, player, CardPilePosition.Random));
                    }
                }
            }
        }
    }
}