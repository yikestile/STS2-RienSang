using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class Weave : RienSangCard
{
    private static readonly SpireField<Creature, int> _usesThisCombat = new SpireField<Creature, int>(() => 0);

    public Weave() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
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

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player.Creature;
        var combat = creature.CombatState;
        
        var existingCaduceusIds = player.Piles.SelectMany(p => p.Cards)
            .Where(c => c.CanonicalKeywords.Contains(RienSangKeywords.Caduceus))
            .Select(c => c.Id)
            .ToHashSet();

        var allCaduceusTemplates = ModelDb.AllCards
            .Where(c => c.CanonicalKeywords.Contains(RienSangKeywords.Caduceus))
            .ToList();

        var uniqueTemplates = allCaduceusTemplates
            .Where(t => !existingCaduceusIds.Contains(t.Id))
            .ToList();

        if (uniqueTemplates.Any())
        {
            var chosenTemplate = uniqueTemplates.StableShuffle(player.RunState.Rng.CombatCardGeneration).First();
            var newCard = combat.CreateCard(chosenTemplate, player);
            await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, player);
        }

        var copy = CreateClone();
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, player);

        int uses = _usesThisCombat[creature];
        decimal karmaBase = DynamicVars["KarmaGain"].BaseValue;
        decimal karmaToGain = karmaBase + (uses * 5);

        await creature.ApplyKarma(choiceContext, karmaToGain, creature, this);

        _usesThisCombat[creature] = uses + 1;
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
        DynamicVars["KarmaGain"].UpgradeValueBy(-1);
    }
}