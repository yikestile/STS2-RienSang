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
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Common;

[Pool(typeof(RienSangCardPool))]
public class WillOfThePrescript : RienSangCard
{
    public WillOfThePrescript() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LCEvadePower>(9),
        new("DrawAmount", 3),
        new EnergyVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCEvadePower>(),
        HoverTipFactory.FromPower<Unlock>()
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Creature;
        
        await PowerCmd.Apply<LCEvadePower>(player, DynamicVars[nameof(LCEvadePower)].IntValue, player, this);
        
        EvadeRegistry.EvadeEffectStacks[player] += 1;
        EvadeRegistry.PendingDraw[player] += (int)DynamicVars["DrawAmount"].BaseValue;

        var unlockPower = player.GetPower<Unlock>();
        if (unlockPower != null && unlockPower.Amount >= 3)
        {
            EvadeRegistry.PendingEnergy[player] += DynamicVars.Energy.IntValue;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(LCEvadePower)].UpgradeValueBy(3);
    }
}