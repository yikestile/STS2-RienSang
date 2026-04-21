using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
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

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
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

            await CheckThresholds(oldAmount, newAmount);
        }
        finally 
        {
            _isProcessing = false;
        }
    }

    private async Task CheckThresholds(int oldAmount, int newAmount)
    {

        if (oldAmount < 3 && newAmount >= 3) await PowerCmd.SetAmount<Unlock>(Owner, 1, Owner, null);
        if (oldAmount < 6 && newAmount >= 6) await PowerCmd.SetAmount<Unlock>(Owner, 2, Owner, null);
        if (oldAmount < 9 && newAmount >= 9) 
        {
            await PowerCmd.SetAmount<Unlock>(Owner, 3, Owner, null);
            await PowerCmd.Apply<StrengthPower>(Owner, 1, Owner, null);
        }
    }
}