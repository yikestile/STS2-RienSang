using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class WillOfTheCity : RienSangCard
{
    public WillOfTheCity() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }
    
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(13, ValueProp.Move), 
        new EnergyVar(2),
        new("DrawAmount", 1)
    ];

    private bool IsUnlockIII => Owner.Creature.GetPower<Unlock>()?.Amount >= 3;

    protected override bool ShouldGlowGoldInternal => IsUnlockIII;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay); 
        
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, (decimal)DynamicVars["DrawAmount"].BaseValue, Owner.Creature, this);

        if (IsUnlockIII)
        {
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, (decimal)DynamicVars.Energy.IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}