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

public class UnlockIII : RienSangCard
{
    public UnlockIII() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }
    
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ RienSangKeywords.Caduceus ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2, ValueProp.Move),
        new RepeatVar(3),
        new ("DrawAmount", 1),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int times = DynamicVars.Repeat.IntValue;
        for (int i = 0; i < times; i++)
        {
            await CaduceusManager.Execute(this, cardPlay.Target, DynamicVars.Damage.BaseValue, choiceContext, i);
        }

        await PlayerCmd.GainEnergy(1, base.Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawAmount"].BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1); 
        DynamicVars["DrawAmount"].UpgradeValueBy(1);
    }
}
