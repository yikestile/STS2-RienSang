using System.Collections.Generic;
using System.Threading.Tasks;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Patches;

namespace RienSang.RienSangCode.Powers;


public class DefensiveStancePower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner?.Player) return;
        if (cardPlay.Card.Type == CardType.Attack)
        {
            if (cardPlay.Card.DynamicVars.TryGetValue("Damage", out var damageVar))
            {
                var damage = damageVar.IntValue;
                await CreatureCmd.GainBlock(Owner, damage, ValueProp.SkipHurtAnim, cardPlay);
            }
        }
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!PreviewPatch.IsPreviewCalculation)
        {
            if (cardSource != null && cardSource.Type == CardType.Attack && dealer == Owner)
            {
                return 0m;
            }
        }
        return 1m;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, ICombatState state)
    {
        if (side == CombatSide.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}