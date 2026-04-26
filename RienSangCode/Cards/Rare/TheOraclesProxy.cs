using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class TheOraclesProxy : RienSangCard
{
    protected override bool HasEnergyCostX => true;

    public TheOraclesProxy() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Turns", 0m),
        new DynamicVar("Heal", 3m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TheOraclesProxyPower>(),
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int x = ResolveEnergyXValue();
        if (x > 0)
        {
            int healPerTurn = (int)DynamicVars["Heal"].BaseValue + x;
            await PowerCmd.Apply<Powers.TheOraclesProxyPower>(choiceContext, Owner.Creature, x, Owner.Creature, this);
            
            var hermes = Owner.Creature.GetPower<Powers.TheOraclesProxyPower>();
            if (hermes != null)
            {
                hermes.HealAmount = healPerTurn;
            }
            
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(2);
    }
}