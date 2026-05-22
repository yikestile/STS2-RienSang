using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using RienSang.RienSangCode.Extensions;
using MegaCrit.Sts2.Core.Combat;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class Reconstruct : RienSangCard
{
    private static readonly SpireField<Creature, int> _usesThisCombat = new SpireField<Creature, int>(() => 0);

    public Reconstruct() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override bool GainsKarma => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaGain", 5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = PileType.Hand.GetPile(Owner);
        if (hand != null && hand.Cards.Count >= 1) 
        {
            CardModel? selected = null;

            if (hand.Cards.Count == 1)
            {
                selected = hand.Cards.FirstOrDefault();
            }
            else
            {
                selected = (await CardSelectCmd.FromHand(
                    choiceContext, 
                    Owner, 
                    new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1), 
                    (CardModel c) => c != this,
                    this
                )).FirstOrDefault();
            }

            if (selected != null)
            {
                IEnumerable<CardModel> defaultOptions = CardFactory.GetDefaultTransformationOptions(selected, true);

                var filteredOptions = defaultOptions.Where(c => 
                    !(c is RienSangCard rienSangCard && rienSangCard.IsEgoCard) &&
                    c.Rarity != CardRarity.Ancient
                ).ToList();

                if (filteredOptions.Any())
                {
                    var randomCard = CardFactory.CreateRandomCardForTransform(selected, filteredOptions, true, Owner.RunState.Rng.CombatCardGeneration);
                    if (randomCard != null)
                    {
                        await CardCmd.Transform(selected, randomCard);
                    }
                }
            }
        }

        var copy = CreateClone();
        await CardPileCmd.Add(copy, PileType.Hand);

        int uses = _usesThisCombat[Owner.Creature];
        decimal karma = DynamicVars["KarmaGain"].BaseValue + (uses * 5);
        await Owner.Creature.ApplyKarma(choiceContext, karma, Owner.Creature);

        _usesThisCombat[Owner.Creature] = uses + 1;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Creature.Side)
        {
            _usesThisCombat[Owner.Creature] = 0;
        }
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KarmaGain"].UpgradeValueBy(-2);
    }
}