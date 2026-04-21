using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class SomberProcuration : RienSangCard
{
    public SomberProcuration() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move), 
        new BlockVar(6, ValueProp.Move),
        new("UnlockBonus", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus, RienSangKeywords.Unlock];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Unlock>()
    ];

    private bool IsUnlockIII => Owner.Creature.GetPower<Unlock>()?.Amount >= 3;

    protected override bool ShouldGlowGoldInternal => IsUnlockIII;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal bonus = IsUnlockIII ? DynamicVars["UnlockBonus"].BaseValue : 0;
        
        decimal damageBase = DynamicVars.Damage.BaseValue + bonus;

        decimal blockTotal = DynamicVars.Block.BaseValue + bonus;

        if (cardPlay.Target != null && cardPlay.Target.IsAlive)
        {
            await CaduceusManager.Execute(this, cardPlay.Target, damageBase, choiceContext, 0);
        }

        await CreatureCmd.GainBlock(Owner.Creature, blockTotal, ValueProp.Unpowered, null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
