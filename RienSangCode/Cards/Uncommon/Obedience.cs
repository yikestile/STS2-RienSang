using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class Obedience : RienSangCard
{
    public Obedience() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaLoss", 20m),
        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TheOraclesProxyPower>(),
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int energyGain = DynamicVars.Energy.IntValue;
        
        await Owner.Creature.ModifyKarma(choiceContext, -DynamicVars["KarmaLoss"].BaseValue, Owner.Creature);
        
        await PowerCmd.Apply<TheOraclesProxyPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        var hermes = Owner.Creature.GetPower<TheOraclesProxyPower>();
        if (hermes != null)
        {
            hermes.EnergyGain = energyGain;
            hermes.HealAmount = 0;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KarmaLoss"].UpgradeValueBy(10);
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}