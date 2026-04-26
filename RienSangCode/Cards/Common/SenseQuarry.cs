using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class SenseQuarry : RienSangCard
{
    public SenseQuarry() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(7, ValueProp.Move), 
        new("BonusBlock", 3),
        new PowerVar<LCPoisePower>("PoisePotency", 4m)
    ];

    private bool IsUnlockIII => Owner.Creature.GetPower<Unlock>()?.Amount >= 3;

    protected override bool ShouldGlowGoldInternal => IsUnlockIII;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Unlock>(),
        HoverTipFactory.FromPower<LCPoisePower>()
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        
        if (IsUnlockIII)
        {
            await CreatureCmd.GainBlock(Owner.Creature, (decimal)DynamicVars["BonusBlock"].BaseValue, ValueProp.Unpowered, null);
            await LCPoisePower.Apply(choiceContext, Owner.Creature, 1, DynamicVars["PoisePotency"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars["BonusBlock"].UpgradeValueBy(2);
        DynamicVars["PoisePotency"].UpgradeValueBy(2);
    }
}