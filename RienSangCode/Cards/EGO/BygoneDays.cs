using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Cards.EGO;

[Pool(typeof(RienSangCardPool))]
public class BygoneDays : RienSangCard
{
    public BygoneDays() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override bool IsEgoCard => true;
    public override int SpCost => 15;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCSinkingPower>(1),
        new("SinkingPotency", 10)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCSinkingPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int potency = (int)DynamicVars["SinkingPotency"].BaseValue;
        int count = (int)DynamicVars[nameof(LCSinkingPower)].BaseValue;

        if (Owner.Creature.CombatState != null)
        {
            foreach (var enemy in Owner.Creature.CombatState.Enemies)
            {
                if (enemy.IsAlive)
                {
                    await LCSinkingPower.Apply(choiceContext, enemy, count, potency, Owner.Creature, this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LCSinkingPower)].UpgradeValueBy(2);
    }
}