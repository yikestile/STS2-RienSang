using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Ancient;

[Pool(typeof(RienSangCardPool))]
public class FuriosoReplica : RienSangCard
{
    public FuriosoReplica() : base(3, CardType.Attack, CardRarity.Ancient, TargetType.RandomEnemy)
    {
    }
    public override bool IsLCSpecialCard => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1, ValueProp.Move),
        new RepeatVar(9),
        new PowerVar<LCPoisePower>(3), 
        new("PoisePotency", 3),      
        new PowerVar<Mang>(1),
        new("SinkingPotency", 1) 
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCPoisePower>(),
        HoverTipFactory.FromPower<Mang>(),
        HoverTipFactory.FromPower<LCSinkingPower>(),
        HoverTipFactory.FromPower<ProcurationHermes>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Creature;
        
        if (SanityManager.GetSanity(Owner) >= 0)
        {
            SanityManager.SpendSanity(Owner, 10f);
            await PowerCmd.Apply<Mang>(choiceContext, player, (decimal)DynamicVars[nameof(Mang)].IntValue, player, null);
        }

        var hermes = player.GetPower<ProcurationHermes>();
        
        if (hermes != null)
        {
            hermes.LockGaining();
        }

        var combatState = base.CombatState;
        
        var woundPower = player.GetPower<WoundcasingMask>();
        if (woundPower != null) woundPower.FuriosoPlayedThisCombat = true;

        var initialPoisePotency = player.HasPower<IndulgenceInPrescript>() ? 4 : DynamicVars["PoisePotency"].IntValue;
        var poiseCount = DynamicVars[nameof(LCPoisePower)].IntValue;

        await LCPoisePower.Apply(choiceContext, player, poiseCount, initialPoisePotency, player, this);

        int hitCount = base.DynamicVars.Repeat.IntValue;
        decimal baseDamage = base.DynamicVars.Damage.BaseValue;

        for (var i = 0; i < hitCount; i++)
        {
            if (combatState != null)
            {
                var potentialTargets = combatState.HittableEnemies;
                if (potentialTargets.Count <= 0) break;

                var target = potentialTargets.TakeRandom(1, combatState.RunState.Rng.CombatTargets).First();

                bool isFinisher = (i == hitCount - 1);
                await CaduceusManager.Execute(this, target, baseDamage, choiceContext, i, isFuriosoFinisher: isFinisher);
            }
        }

        if (hermes != null)
        {
            await hermes.ConsumeStacksAndLock();
        }
    }
}