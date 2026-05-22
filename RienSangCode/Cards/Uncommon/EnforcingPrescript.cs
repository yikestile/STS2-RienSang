using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Patches;

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

        if (cardPlay.Target?.Monster is MonsterModel monster)
        {
            var enemyMove = monster.NextMove;
            if (enemyMove == null) return;

            List<AbstractIntent> newIntents = [];
            bool hasAttackIntent = false;

            foreach (var intent in enemyMove.Intents)
            {
                if (intent is AttackIntent attack)
                {
                    hasAttackIntent = true;
                    int originalHits = (attack is MultiAttackIntent multi) ? multi.Repeats : 1;
                    
                    decimal originalModifiedDamagePerHit = attack.GetSingleDamage(new Creature[] { Owner.Creature }, monster.Creature);
                    decimal totalOriginalModifiedDamage = originalModifiedDamagePerHit * originalHits;

                    int newHits = originalHits * 2;
                    decimal strengthAmount = monster.Creature.GetPower<StrengthPower>()?.Amount ?? 0m;

                    decimal desiredModifiedDamagePerNewHit = totalOriginalModifiedDamage / newHits;
                    decimal baseDamageForNewIntent = desiredModifiedDamagePerNewHit - strengthAmount;

                    int finalBaseDamageForNewIntent = Math.Max(0, (int)baseDamageForNewIntent);
                    if (desiredModifiedDamagePerNewHit > 0 && finalBaseDamageForNewIntent <= 0)
                    {
                        finalBaseDamageForNewIntent = 1;
                    }

                    newIntents.Add(new MultiAttackIntent(finalBaseDamageForNewIntent, newHits));
                }
                else
                {
                    newIntents.Add(intent);
                }
            }

            if (hasAttackIntent)
            {
                var moveType = typeof(MoveState);
                
                var actionField = moveType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType == typeof(Func<IReadOnlyList<Creature>, Task>));
                    
                var intentsField = moveType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => typeof(IEnumerable<AbstractIntent>).IsAssignableFrom(f.FieldType));

                Func<IReadOnlyList<Creature>, Task>? originalOnPerform = null;
                if (actionField != null)
                {
                    originalOnPerform = actionField.GetValue(enemyMove) as Func<IReadOnlyList<Creature>, Task>;
                }

                Func<IReadOnlyList<Creature>, Task> wrappedOnPerform = async (targets) =>
                {
                    EnforcingPrescriptTracker.ModifiedCreatures.Add(monster.Creature);
                    try
                    {
                        if (originalOnPerform != null)
                        {
                            await originalOnPerform(targets);
                        }
                    }
                    finally
                    {
                        EnforcingPrescriptTracker.ModifiedCreatures.Remove(monster.Creature);
                    }
                };

                actionField?.SetValue(enemyMove, wrappedOnPerform);

                if (intentsField != null)
                {
                    if (intentsField.FieldType == typeof(AbstractIntent[]))
                        intentsField.SetValue(enemyMove, newIntents.ToArray());
                    else if (intentsField.FieldType == typeof(List<AbstractIntent>))
                        intentsField.SetValue(enemyMove, newIntents);
                    else if (intentsField.FieldType == typeof(IReadOnlyList<AbstractIntent>))
                        intentsField.SetValue(enemyMove, newIntents.AsReadOnly());
                }

                monster.SetMoveImmediate(enemyMove);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}