using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Patches;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class EnforcingPrescript : RienSangCard
{
    public EnforcingPrescript() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaGain", 10m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Owner.Creature.ApplyKarma(choiceContext, DynamicVars["KarmaGain"].BaseValue, Owner.Creature, this);

        if (cardPlay.Target != null && cardPlay.Target.Monster != null)
        {
            var monster = cardPlay.Target.Monster;
            var currentMove = monster.NextMove;
        
            var attackIntent = currentMove.Intents.OfType<AttackIntent>().FirstOrDefault();
            if (attackIntent != null)
            {
                int originalHits;
                decimal originalBaseDamage;

 
                originalBaseDamage = attackIntent.DamageCalc?.Invoke() ?? 0m;

                if (attackIntent is SingleAttackIntent)
                {
                    originalHits = 1;
                }
                else if (attackIntent is MultiAttackIntent multi)
                {
    
                    originalHits = multi.Repeats;
                }
                else
                {
                    return;
                }

                int newHits = originalHits * 2;
                decimal newBaseDamage = originalBaseDamage / 2m;

                int newRepeats = newHits;

                MoveState splitMove = new MoveState(
                    currentMove.StateId, 
                    currentMove.PerformMove,
                    new MultiAttackIntent((int)newBaseDamage, newRepeats)
                );
            
                monster.SetMoveImmediate(splitMove, forceTransition: true);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}