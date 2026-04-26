using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using LimbusCore.LimbusCoreCode.Mechanics; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;
public class Unlock : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("HealAmount", 0m),
        new("SPHealAmount", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<DexterityPower>();
            yield return HoverTipFactory.FromPower<ShinFate>();
        }
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;

        int oldAmount = (int)(power.Amount - amount);

        if (power.Amount > 3m)
        {
            power.SetAmount(3);
        }

        var newAmount = (int)power.Amount;
        
        DynamicVars["HealAmount"].BaseValue = newAmount * 2;
        DynamicVars["SPHealAmount"].BaseValue = newAmount * 5;

        await CheckUnlockMilestones(choiceContext, oldAmount, newAmount);
        Flash();
        InvokeDisplayAmountChanged();
    }

    private async Task CheckUnlockMilestones(PlayerChoiceContext choiceContext, int oldAmount, int newAmount)
    {
        if (oldAmount < 3 && newAmount >= 3)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 1, Owner, null);
            await PowerCmd.Apply<ShinFate>(choiceContext, Owner, 1, Owner, null);
        }
    }
    
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var healAmount = base.Amount * 2m;
        var spHeal = base.Amount * 5f;
        
        if (Owner != null && !Owner.IsDead)
        {
            Flash();
            if (healAmount > 0)
            {
                await CreatureCmd.Heal(Owner, healAmount);
            }
            
            if (Owner.Player != null)
            {
                SanityManager.ModifySanity(Owner.Player, spHeal);
            }
        }
    }
}