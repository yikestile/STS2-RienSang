using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Cards.Curse;

[Pool(typeof(RienSangCardPool))]
public class PrescriptIncomplianceRisk : RienSangCard
{
    public override bool HasTurnEndInHandEffect => true;

    public PrescriptIncomplianceRisk() : base(1, CardType.Curse, CardRarity.Curse, TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaGain", 10m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal currentKarma = Owner.Creature.GetKarmaAmount();
        
        if (currentKarma > 0)
        {
            int damage = (int)(currentKarma / 5m);
            if (damage > 0)
            {
                await CreatureCmd.Damage(choiceContext, Owner.Creature, damage, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this);
            }
        }
    }

    public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await Owner.Creature.ApplyKarma(choiceContext, DynamicVars["KarmaGain"].BaseValue, Owner.Creature);
        
        await CardCmd.Exhaust(choiceContext, this);

        var copy = CreateClone();
        await CardPileCmd.Add(copy, PileType.Draw, CardPilePosition.Top);
    }
}