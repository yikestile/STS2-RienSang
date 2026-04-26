using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using RienSang.RienSangCode.Character;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class PrecognitionReplica : RienSangCard
{
    protected override bool HasEnergyCostX => true;

    public PrecognitionReplica() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCEvadePower>(7), 
        new("PoisePotency", 7),
        new PowerVar<LCPoisePower>(3), 
        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCEvadePower>(),
        HoverTipFactory.FromPower<LCPoisePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int x = ResolveEnergyXValue();
        if (x > 0)
        {
            int evadeAmount = DynamicVars[nameof(LCEvadePower)].IntValue * x;
            await PowerCmd.Apply<LCEvadePower>(choiceContext, Owner.Creature, (decimal)evadeAmount, Owner.Creature, this);
            
            EvadeRegistry.EvadeEffectStacks[Owner.Creature] += 1;
            EvadeRegistry.PendingPoisePotency[Owner.Creature] += (int)DynamicVars["PoisePotency"].BaseValue;
            
            EvadeRegistry.PendingPoiseCount[Owner.Creature] = DynamicVars[nameof(LCPoisePower)].IntValue;

            EvadeRegistry.PendingEnergy[Owner.Creature] += DynamicVars.Energy.IntValue;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LCEvadePower)].UpgradeValueBy(3);
        DynamicVars["PoisePotency"].UpgradeValueBy(3);
        DynamicVars[nameof(LCPoisePower)].UpgradeValueBy(1);
    }
}