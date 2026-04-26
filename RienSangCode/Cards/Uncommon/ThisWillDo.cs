using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using Godot;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class ThisWillDo : RienSangCard
{
    public ThisWillDo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("PotencyCost", 20m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCPoisePower>()
    ];

    protected override bool IsPlayable => Owner?.Creature?.GetPower<LCPoisePower>()?.CanConsumePotency((int)DynamicVars["PotencyCost"].BaseValue) ?? false;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Creature;
        var poise = player.GetPower<LCPoisePower>();
        if (poise != null)
        {
            await poise.ConsumePotency((int)DynamicVars["PotencyCost"].BaseValue);
        }

        var handPile = PileType.Hand.GetPile(Owner);
        
        var validTargets = handPile?.Cards.Where(c => c.Type == CardType.Skill && c != this).ToList();

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext, 
            Owner, 
            new CardSelectorPrefs(SelectionScreenPrompt, 1) { PretendCardsCanBePlayed = true },
            (CardModel c) => c.Type == CardType.Skill && c != this,
            this
        );
        
        var choice = selectedCards?.FirstOrDefault();

        if (choice != null)
        {
            for (int i = 0; i < 2; i++)
            {
                await CardCmd.AutoPlay(choiceContext, choice, null);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PotencyCost"].UpgradeValueBy(-10);
    }
}