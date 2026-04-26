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
        new EnergyVar(0) // Added for {Energy:diff()}
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TheOraclesProxyPower>(),
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int energyGain = DynamicVars.Energy.IntValue;
        if (energyGain > 0)
        {
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, energyGain, Owner.Creature, this);
        }
        
        await Owner.Creature.ModifyKarma(choiceContext, -DynamicVars["KarmaLoss"].BaseValue, Owner.Creature);
        await PowerCmd.Apply<TheOraclesProxyPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KarmaLoss"].UpgradeValueBy(10);
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}