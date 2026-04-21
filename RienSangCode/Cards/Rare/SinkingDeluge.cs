using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Cards.Rare;

[Pool(typeof(RienSangCardPool))]
public class SinkingDeluge : RienSangCard
{
    public SinkingDeluge() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LCSinkingPower>(),
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null) return;

        if (IsUpgraded)
        {
            var power = await PowerCmd.Apply<LCSinkingPower>(play.Target, 3, Owner.Creature, this);
            power?.AddPotency(3);
        }

        var sinking = play.Target.GetPower<LCSinkingPower>();
        if (sinking != null)
        {
            int damage = (int)(sinking.Amount * sinking.Potency)/2;
            
            if (damage > 0)
            {
                await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
            }

            await PowerCmd.Remove(sinking);
        }
    }
}