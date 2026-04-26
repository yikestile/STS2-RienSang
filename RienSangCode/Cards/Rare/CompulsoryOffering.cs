using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class CompulsoryOffering : RienSangCard
{
    public CompulsoryOffering() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmicConsequence", 20m),
        new("PotencyCost", 20)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCPoisePower>(),
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override bool IsPlayable => Owner?.Creature?.GetPower<LCPoisePower>()?.CanConsumePotency(DynamicVars["PotencyCost"].IntValue) ?? false;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        
        var poise = creature.GetPower<LCPoisePower>();
        if (poise != null)
        {
            await poise.ConsumePotency(DynamicVars["PotencyCost"].IntValue);
        }
        await creature.ApplyKarma(choiceContext, DynamicVars["KarmicConsequence"].BaseValue, creature, this);

        var drawPile = PileType.Draw.GetPile(player);
        var powerInDraw = drawPile?.Cards.FirstOrDefault(c => c.Type == CardType.Power);
        bool foundPower = powerInDraw != null;

        if (foundPower)
        {
            await CardPileCmd.Draw(choiceContext, 1m, player);
            
            var powersInDraw = drawPile.Cards.Where(c => c.Type == CardType.Power).ToList();
            if (powersInDraw.Any())
            {
                var powerToDraw = powersInDraw.StableShuffle(player.RunState.Rng.Shuffle).First();
                await CardPileCmd.Add(powerToDraw, PileType.Hand);
            }
        }
        else
        {
            await CardPileCmd.Draw(choiceContext, 2m, player);
        }

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1) { PretendCardsCanBePlayed = true };

        var hand = PileType.Hand.GetPile(player);
        bool handHasPower = hand.Cards.Any(c => c.Type == CardType.Power && c != this);

        var selected = await CardSelectCmd.FromHand(
            choiceContext, 
            player, 
            prefs,
            (CardModel c) => (handHasPower ? c.Type == CardType.Power : c.Type == CardType.Skill) && c != this,
            this
        );
        
        var choice = selected.FirstOrDefault();
        if (choice != null)
        {
            var dupe = choice.CreateDupe();
            dupe.SetToFreeThisTurn();
            await CardCmd.AutoPlay(choiceContext, dupe, null);
            
            await CardCmd.AutoPlay(choiceContext, choice, null);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}