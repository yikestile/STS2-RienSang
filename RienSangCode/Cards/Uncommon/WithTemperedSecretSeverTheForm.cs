using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class WithTemperedSecretSeverTheForm : RienSangCard
{
    public WithTemperedSecretSeverTheForm() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new RepeatVar(2),
        new PowerVar<VulnerablePower>("Vulnerable", 1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus];

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
            for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
            {
                await CaduceusManager.Execute(this, target, baseDamage, choiceContext, i, LimbusDamageType.Slash, biasChance);
            }
            
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, DynamicVars["Vulnerable"].IntValue, player, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["Vulnerable"].UpgradeValueBy(1);
    }
}