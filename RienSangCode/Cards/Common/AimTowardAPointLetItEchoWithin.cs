using System;
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
public class AimTowardAPointLetItEchoWithin : RienSangCard
{
    public AimTowardAPointLetItEchoWithin() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move),
        new PowerVar<LCSinkingPower>(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCSinkingPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        var player = Owner.Creature;
        decimal baseDamage = DynamicVars.Damage.BaseValue;

        var karmaPower = player.GetPower<KarmicConsequence>();
        int karma = karmaPower?.DisplayAmount ?? 0;
        float rawChance = 75f - (karma / 2f);
        float biasChance = Math.Clamp(rawChance, 0f, 100f) / 100f;

        if (target != null && target.IsAlive)
        {
            await CaduceusManager.Execute(this, target, baseDamage, choiceContext, 0, LimbusDamageType.Pierce, biasChance);
            
            int val = DynamicVars[nameof(LCSinkingPower)].IntValue;
            var sinking = await PowerCmd.Apply<LCSinkingPower>(target, val, player, this);
            if (sinking != null)
            {
                sinking.AddPotency(val);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars[nameof(LCSinkingPower)].UpgradeValueBy(1);
    }
}
