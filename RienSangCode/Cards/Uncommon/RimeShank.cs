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

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class RimeShank : RienSangCard
{
    public RimeShank() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCSinkingPower>(3) // Multiplier for X
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCSinkingPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        if (x > 0)
        {
            int sinkingCount = DynamicVars[nameof(LCSinkingPower)].IntValue * x;
            if (Owner.Creature.CombatState != null)
            {
                foreach (var enemy in Owner.Creature.CombatState.Enemies)
                {
                    if (enemy.IsAlive)
                    {
                        await PowerCmd.Apply<LCSinkingPower>(enemy, sinkingCount, Owner.Creature, this);
                    }
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LCSinkingPower)].UpgradeValueBy(1); // 3 -> 4
    }
}
