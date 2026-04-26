using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;
public class GraceofthePrescriptPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<StrengthPower>();
            yield return HoverTipFactory.FromPower<Unlock>();
        }
    }
    
    public GraceofthePrescriptPower() : base() { } 

    public GraceofthePrescriptPower(int amount) : this()
    {
        SetAmount(amount);
    }

    private bool _isProcessing = false;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || _isProcessing) return;

        _isProcessing = true;
        try 
        {
            if (power.Amount > 9m)
            {
                power.SetAmount(9); 
            }

            int oldAmount = (int)(power.Amount - amount);
            int newAmount = (int)power.Amount;

            await CheckThresholds(choiceContext, oldAmount, newAmount);
        }
        finally 
        {
            _isProcessing = false;
        }
    }

    private async Task CheckThresholds(PlayerChoiceContext choiceContext, int oldAmount, int newAmount)
    {
        async Task SetPowerAmount<T>(int targetAmount) where T : PowerModel
        {
            var existing = Owner.GetPower<T>();
            int currentAmount = existing?.Amount ?? 0;
            int diff = targetAmount - currentAmount;
            if (diff != 0)
            {
                await PowerCmd.Apply<T>(choiceContext, Owner, (decimal)diff, Owner, null);
            }
        }

        if (oldAmount < 3 && newAmount >= 3) await SetPowerAmount<Unlock>(1);
        if (oldAmount < 6 && newAmount >= 6) await SetPowerAmount<Unlock>(2);
        if (oldAmount < 9 && newAmount >= 9) 
        {
            await SetPowerAmount<Unlock>(3);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1m, Owner, null);
        }
    }
}