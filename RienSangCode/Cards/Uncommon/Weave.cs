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

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class Weave : RienSangCard
{
    public Weave() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaGain", 10m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Owner.Creature.ApplyKarma(choiceContext, DynamicVars["KarmaGain"].BaseValue, Owner.Creature, this);

        var player = Owner;
        var combat = player.Creature.CombatState;
        
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
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}