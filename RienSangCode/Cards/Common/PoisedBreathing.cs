using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class PoisedBreathing : RienSangCard
{
    public PoisedBreathing() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCEvadePower>(5),
        new("PoiseAmount", 5)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCEvadePower>(),
        HoverTipFactory.FromPower<LCPoisePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<LCEvadePower>(choiceContext, Owner.Creature, (decimal)DynamicVars[nameof(LCEvadePower)].IntValue, Owner.Creature, this);
        
        int poiseVal = (int)DynamicVars["PoiseAmount"].BaseValue;
        
        EvadeRegistry.EvadeEffectStacks[Owner.Creature] += 1;
        EvadeRegistry.PendingPoisePotency[Owner.Creature] += poiseVal;
        
        // Simplified: The Power class now handles potency accumulation internally via AfterApplied.
        // We need to ensure the count is set correctly if it's a new power instance.
        // For EvadeRegistry, we just set the pending values, the patch will apply it.
        EvadeRegistry.PendingPoiseCount[Owner.Creature] = DynamicVars[nameof(LCPoisePower)].IntValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LCEvadePower)].UpgradeValueBy(2);
        DynamicVars["PoiseAmount"].UpgradeValueBy(2);
    }
}
