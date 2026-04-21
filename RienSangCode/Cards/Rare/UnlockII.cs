using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

public class UnlockII : RienSangCard
{
    public UnlockII() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2m, ValueProp.Move),
        new RepeatVar(2)
    ];
    
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust, RienSangKeywords.Caduceus ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var times = DynamicVars.Repeat.IntValue;
        for (var i = 0; i < times; i++)
        {
            await CaduceusManager.Execute(this, cardPlay.Target, DynamicVars.Damage.BaseValue, choiceContext, i);
        }
        
        if (Owner.Creature.CombatState != null)
        {
            var nextCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<UnlockIII>(), Owner);
            
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(nextCard);
            }
            await CardPileCmd.Add(nextCard, PileType.Draw, CardPilePosition.Top);
        }
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m); // 2 -> 3
}
