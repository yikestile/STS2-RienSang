using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class StarOfTheCity : RienSangCard
{
    public StarOfTheCity() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
    }
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCSinkingPower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            int stacksApplied = IsUpgraded ? 2 : 1;

            return new List<DynamicVar>
            {
                new DynamicVar("SinkingPotency", stacksApplied * StarOfTheCityPower.SinkingPotencyPerStack),
                new DynamicVar("SinkingCount", stacksApplied * StarOfTheCityPower.SinkingCountPerStack)
            };
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Creature;
        
        int stacksToApply = IsUpgraded ? 2 : 1;
        await PowerCmd.Apply<StarOfTheCityPower>(choiceContext, player, stacksToApply, player, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars["SinkingPotency"].UpgradeValueBy(5);
        DynamicVars["SinkingCount"].UpgradeValueBy(2);
    }
}
