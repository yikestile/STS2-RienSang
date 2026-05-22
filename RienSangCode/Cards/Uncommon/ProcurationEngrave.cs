using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;


namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class ProcurationEngrave : RienSangCard
{
    public ProcurationEngrave() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("UnlockBonus", 20)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RienSangKeywords.Caduceus];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        decimal baseDamage = DynamicVars.Damage.BaseValue;

        var karmaPower = Owner.Creature.GetPower<KarmicConsequence>();
        int karma = karmaPower?.DisplayAmount ?? 0;
        float rawChance = 75f - (karma / 2f);
        float biasChance = Math.Clamp(rawChance, 0f, 100f) / 100f;

        var unlockPower = Owner.Creature.GetPower<Unlock>();
        if (unlockPower != null && unlockPower.Amount >= 3)
        {
            baseDamage += Math.Floor(baseDamage * (DynamicVars["UnlockBonus"].BaseValue / 100m));
        }

        if (target != null && target.IsAlive)
        {
            await CaduceusManager.Execute(this, target, baseDamage, choiceContext, 0, LimbusDamageType.Slash, biasChance);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}