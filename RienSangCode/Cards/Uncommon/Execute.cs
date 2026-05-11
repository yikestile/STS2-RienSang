using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class Execute() : RienSangCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    private bool WasLastCardPlayedNotAttack
    {
        get
        {
            CardPlayStartedEntry? cardPlayEntry = CombatManager.Instance.History.CardPlaysStarted.LastOrDefault(e => 
                e.CardPlay.Card.Owner == base.Owner && 
                e.HappenedThisTurn(base.CombatState) && 
                e.CardPlay.Card != this);
            
            if (cardPlayEntry == null) return true;
            return cardPlayEntry.CardPlay.Card.Type != CardType.Attack;
        }
    }

    protected override bool ShouldGlowGoldInternal => WasLastCardPlayedNotAttack;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4m, ValueProp.Move),
        new RepeatVar(2),
        new("BonusDamage", 2m),
        new("StrGain", 1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus];

    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.Static(StaticHoverTip.Fatal)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        var dmg = DynamicVars.Damage.BaseValue;

        if (WasLastCardPlayedNotAttack) { dmg += DynamicVars["BonusDamage"].BaseValue; }

        var killedEnemy = false;

        try 
        {
            for (var i = 0; i < DynamicVars.Repeat.IntValue; i++)
            {
                if (target is not { IsAlive: true }) break;
                
                var results = await CaduceusManager.Execute(this, target, dmg, choiceContext, i);

                if (results != null && results.Results.SelectMany(list => list).Any((DamageResult r) => r.WasTargetKilled))
                {
                    killedEnemy = true;
                    break;
                }
            }
        }
        finally 
        {
            if (Owner.Character is Character.RienSang character)
            {
                await character.ReturnToIdlePosition(Owner.Creature, 0.3f);
            }
        }

        if (killedEnemy)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, (int)DynamicVars["StrGain"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["BonusDamage"].UpgradeValueBy(1m);
    }
}
