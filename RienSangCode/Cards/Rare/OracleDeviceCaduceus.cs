using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class OracleDeviceCaduceus : RienSangCard
{
    public OracleDeviceCaduceus() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<OracleDevicePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<OracleDevicePower>("DamageBonus", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<OracleDevicePower>(choiceContext, Owner.Creature, DynamicVars["DamageBonus"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamageBonus"].UpgradeValueBy(1);
    }
}