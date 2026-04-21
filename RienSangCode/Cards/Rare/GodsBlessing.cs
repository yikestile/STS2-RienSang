using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

public class GodsBlessing : RienSangCard
{
    public GodsBlessing() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Retain
    };
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<ProcurationHermes>(),
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    public bool MeetConditions
    {
        get
        {
            if (Owner == null || Owner.Creature == null) return false;

            int uniqueCaduceusCount = CountUniqueCaduceusCards(Owner);
            if (uniqueCaduceusCount < 15) return false;

            var karmicPower = Owner.Creature.GetPower<KarmicConsequence>();
            if (karmicPower != null && karmicPower.Amount >= 15) return false;

            return true;
        }
    }

    protected override bool IsPlayable => MeetConditions;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hermesPower = Owner.Creature.GetPower<ProcurationHermes>();
        if (hermesPower != null)
        {
            hermesPower.SetStacks(9, bypassLimit: true);
        }
        else
        {
            await PowerCmd.Apply<ProcurationHermes>(Owner.Creature, 1m, Owner.Creature, this);
            var newHermes = Owner.Creature.GetPower<ProcurationHermes>();
            if (newHermes != null)
            {
                newHermes.SetStacks(9, bypassLimit: true);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private int CountUniqueCaduceusCards(Player player)
    {
        HashSet<ModelId> uniqueCaduceusCards = new HashSet<ModelId>();

        AddCaduceusCardsFromPile(PileType.Hand.GetPile(player), uniqueCaduceusCards);
        AddCaduceusCardsFromPile(PileType.Draw.GetPile(player), uniqueCaduceusCards);
        AddCaduceusCardsFromPile(PileType.Discard.GetPile(player), uniqueCaduceusCards);
        AddCaduceusCardsFromPile(PileType.Exhaust.GetPile(player), uniqueCaduceusCards);
        
        return uniqueCaduceusCards.Count;
    }

    private void AddCaduceusCardsFromPile(CardPile pile, HashSet<ModelId> uniqueIds)
    {
        if (pile == null) return;
        foreach (var card in pile.Cards)
        {
            if (card.CanonicalKeywords.Contains(RienSangKeywords.Caduceus))
            {
                uniqueIds.Add(card.Id);
            }
        }
    }
}
