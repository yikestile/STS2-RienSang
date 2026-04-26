using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class GraceOfGod() : RienSangCard(2, CardType.Power, CardRarity.Rare, TargetType.None)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.GraceOfGodPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<Powers.GraceOfGodPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
