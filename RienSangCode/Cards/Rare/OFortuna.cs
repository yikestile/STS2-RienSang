using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;
using MegaCrit.Sts2.Core.Combat;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class OFortuna : RienSangCard
{
    public OFortuna() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaLoss", 20m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<KarmicConsequence>(),
        HoverTipFactory.FromPower<KarmicConsequenceFortuna>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Creature;
        decimal currentKarma = player.GetKarmaAmount();

        var regular = player.GetPower<KarmicConsequence>();
        if (regular != null)
        {
            await PowerCmd.Remove(regular);
        }

        decimal reduction = IsUpgraded ? DynamicVars["KarmaLoss"].BaseValue : 0m;
        decimal nextKarma = Math.Max(0, currentKarma - reduction);

        await PowerCmd.Apply<KarmicConsequenceFortuna>(choiceContext, player, 1, player, this);
        var fortuna = player.GetPower<KarmicConsequenceFortuna>();
        if (fortuna != null)
        {
             fortuna.DynamicVars["KarmaAmount"].BaseValue = nextKarma;
             fortuna.SetAmount(1, silent: true); 
        }

        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }
}