using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Mechanics;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Localization;

namespace RienSang.RienSangCode.Cards.Rare;

public class GodsBlessing : RienSangCard
{
    public GodsBlessing() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CaduceusCountVar()
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<ProcurationHermes>(),
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    public bool MeetConditions
    {
        get
        {
            if (Owner == null || Owner.Creature == null) return false;

            int uniqueCaduceusCount = Character.RienSang.CountUniqueCaduceusCards(Owner);
            if (uniqueCaduceusCount < 12) return false;

            decimal karma = Owner.Creature.GetKarmaAmount();
            if (karma >= 20) return false;

            return true;
        }
    }

    protected override bool IsPlayable => MeetConditions;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hermesPower = Owner.Creature.GetPower<ProcurationHermes>();
        if (hermesPower != null)
        {
            hermesPower.SetStacks(9, bypassLimit: true);
        }
        else
        {
            await PowerCmd.Apply<ProcurationHermes>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            var newHermes = Owner.Creature.GetPower<ProcurationHermes>();
            if (newHermes != null)
            {
                newHermes.SetStacks(9, bypassLimit: true);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        AddKeyword(CardKeyword.Retain);
    }
}