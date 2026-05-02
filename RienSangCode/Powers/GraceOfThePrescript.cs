using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public class GraceofthePrescriptPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _threshold3Reached = false;
    private bool _threshold6Reached = false;
    private bool _threshold9Reached = false;

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

            int newAmount = (int)power.Amount;
            await CheckThresholds(choiceContext, newAmount);
        }
        finally 
        {
            _isProcessing = false;
        }
    }

    private async Task CheckThresholds(PlayerChoiceContext choiceContext, int newAmount)
    {
        async Task ApplyUnlock(int targetAmount)
        {
            var existing = Owner.GetPower<Unlock>();
            int currentAmount = existing?.Amount ?? 0;
            int diff = targetAmount - currentAmount;
            if (diff > 0)
            {
                await PowerCmd.Apply<Unlock>(choiceContext, Owner, (decimal)diff, Owner, null);
            }
        }

        if (newAmount >= 3 && !_threshold3Reached)
        {
            _threshold3Reached = true;
            await ApplyUnlock(1);
        }
        if (newAmount >= 6 && !_threshold6Reached)
        {
            _threshold6Reached = true;
            await ApplyUnlock(2);
        }
        if (newAmount >= 9 && !_threshold9Reached)
        {
            _threshold9Reached = true;
            await ApplyUnlock(3);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1m, Owner, null);
        }
    }
}