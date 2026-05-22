using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Localization;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class GodsFavor : RienSangCard
{
    public GodsFavor() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

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

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CaduceusCountVar()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Creature;
        
        await PowerCmd.Apply<ProcurationHermes>(choiceContext, player, 1m, player, this);
        
        var hermes = player.GetPower<ProcurationHermes>();
        if (hermes != null)
        {
            hermes.DynamicVars["HermesThresholdCount"].BaseValue++;
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}