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
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Cards.Basic;

[Pool(typeof(RienSangCardPool))]
public class ByUnpredictableWhim : RienSangCard
{
    public ByUnpredictableWhim() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    public override bool GainsKarma => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ("KarmaMultiplier", 5m),
        new PowerVar<GraceofthePrescriptPower>(6m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<GraceofthePrescriptPower>(),
        HoverTipFactory.FromPower<KarmicConsequence>(),
        HoverTipFactory.FromPower<Unlock>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var unlockPower = Owner.Creature.GetPower<Unlock>();
        int currentUnlock = (int)(unlockPower?.Amount ?? 0);

        if (currentUnlock < 2)
        {
            var gracePower = Owner.Creature.GetPower<GraceofthePrescriptPower>();
            decimal currentGrace = gracePower?.Amount ?? 0m;

            if (currentGrace < 6m)
            {
                decimal graceToGain = 6m - currentGrace;
                await PowerCmd.Apply<GraceofthePrescriptPower>(choiceContext, Owner.Creature, graceToGain, Owner.Creature, this);
                
                decimal karmaMultiplier = DynamicVars["KarmaMultiplier"].BaseValue;
                decimal karmaToGain = graceToGain * karmaMultiplier;
                
                if (karmaToGain > 0)
                {
                    await Owner.Creature.ApplyKarma(choiceContext, karmaToGain, Owner.Creature, this);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        AddKeyword(CardKeyword.Innate);
    }
}