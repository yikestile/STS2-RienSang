using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class InLongSwathsOfFrozenBlood : RienSangCard
{
    public InLongSwathsOfFrozenBlood() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar("Block", 5, ValueProp.Move),
        new PowerVar<LCPoisePower>(4),
        new DynamicVar("PoisePotency", 4m),
        new PowerVar<LCSinkingPower>(2),
        new DynamicVar("SinkingPotency", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, (BlockVar)DynamicVars.Block, cardPlay);
        
        int poiseCount = DynamicVars[nameof(LCPoisePower)].IntValue;
        int poisePotency = DynamicVars["PoisePotency"].IntValue;
        await LCPoisePower.Apply(choiceContext, Owner.Creature, poiseCount, poisePotency, Owner.Creature, this);

        var target = cardPlay.Target;
        if (target != null && target.IsAlive)
        {
            int sinkingCount = DynamicVars[nameof(LCSinkingPower)].IntValue;
            int sinkingPotency = DynamicVars["SinkingPotency"].IntValue;
            await LCSinkingPower.Apply(choiceContext, target, sinkingCount, sinkingPotency, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars[nameof(LCPoisePower)].UpgradeValueBy(2);
        DynamicVars["PoisePotency"].UpgradeValueBy(2);
        DynamicVars[nameof(LCSinkingPower)].UpgradeValueBy(1);
        DynamicVars["SinkingPotency"].UpgradeValueBy(1);
    }
}