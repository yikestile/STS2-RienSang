using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class CryTheWaterfall : RienSangCard
{
    public CryTheWaterfall() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar("Block", 5, ValueProp.Move),
        new PowerVar<LCSinkingPower>(3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCSinkingPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await CreatureCmd.GainBlock(Owner.Creature, (BlockVar)DynamicVars.Block, cardPlay);

        if (Owner.Creature.CombatState != null)
        {
            foreach (var enemy in Owner.Creature.CombatState.Enemies)
            {
                if (enemy.IsAlive)
                {
                    await LCSinkingPower.Apply(choiceContext, enemy, DynamicVars[nameof(LCSinkingPower)].IntValue, 1, Owner.Creature, this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars[nameof(LCSinkingPower)].UpgradeValueBy(1);
    }
}