using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Cards.Ancient;
using RienSang.RienSangCode.Cards.Rare;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public class ProcurationHermes : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _isGranting = false;
    private int _stacksGainedThisTurn = 0;
    
    private bool _localConsumedThisTurn = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("HermesAmount", 0m),
        new DynamicVar("HermesThresholdCount", 0m)
    };

    public override int DisplayAmount => DynamicVars["HermesAmount"].IntValue;

    protected override bool IsVisibleInternal => true; 

    public ProcurationHermes() : base()
    {
    }

    public void UpdateUI() => InvokeDisplayAmountChanged();

    public void SetStacks(int amount, bool bypassLimit = false)
    {
        int unlockStage = Owner.GetPower<Unlock>()?.Amount ?? 0;
        int maxStacks = (unlockStage >= 2 || bypassLimit) ? 9 : 8;

        int finalAmount = Math.Clamp(amount, 0, maxStacks);
        
        if (DynamicVars["HermesAmount"].IntValue != finalAmount)
        {
            DynamicVars["HermesAmount"].BaseValue = finalAmount;
            UpdateUI();
            
            if (finalAmount == 9 && !_isGranting)
            {
                TaskHelper.RunSafely(ProcessThreshold());
            }
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player != Owner?.Player) return;

        _stacksGainedThisTurn = 0;
        _localConsumedThisTurn = false;
        await Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;

        int diff = (int)amount;

        if (Amount != 1)
        {
            SetAmount(1, silent: true);
        }

        if (diff == 0) return;

        int currentVirtual = DynamicVars["HermesAmount"].IntValue;
        int newVirtual = currentVirtual;

        bool isBypass = (applier == null && cardSource == null) || (cardSource is GodsBlessing);

        if (diff > 0)
        {
            if (!isBypass)
            {
                var tracker = Owner.GetPower<MarkofthePrescriptPower>();
                bool isConsumed = _localConsumedThisTurn || (tracker?.HermesConsumedThisTurn ?? false);

                if (isConsumed) return;

                int maxGainPerTurn = 3; 
                var unlockPower = Owner.GetPower<Unlock>();
                if (unlockPower != null && unlockPower.Amount > 0)
                {
                    maxGainPerTurn = unlockPower.Amount + 2;
                }
                
                maxGainPerTurn = Math.Max(3, maxGainPerTurn);

                int remainingAllowance = maxGainPerTurn - _stacksGainedThisTurn;
                if (remainingAllowance < 0) remainingAllowance = 0;

                int allowedGain = Math.Min(diff, remainingAllowance);

                if (allowedGain > 0)
                {
                    _stacksGainedThisTurn += allowedGain;
                    newVirtual += allowedGain;
                }
            }
            else
            {
                newVirtual += diff;
            }
        }
        else
        {
            newVirtual += diff;
        }

        int unlockStage = Owner.GetPower<Unlock>()?.Amount ?? 0;
        int maxStacks = (unlockStage >= 2 || isBypass) ? 9 : 8;

        if (newVirtual > maxStacks) newVirtual = maxStacks;
        if (newVirtual < 0) newVirtual = 0;

        if (newVirtual != currentVirtual)
        {
            DynamicVars["HermesAmount"].BaseValue = newVirtual;
            InvokeDisplayAmountChanged();

            if (newVirtual == 9 && diff > 0 && !_isGranting)
            {
                await ProcessThreshold();
            }
        }
    }

    public void LockGaining()
    {
        _localConsumedThisTurn = true;
        var tracker = Owner.GetPower<MarkofthePrescriptPower>();
        if (tracker != null)
        {
            tracker.HermesConsumedThisTurn = true;
        }
    }

    public async Task ConsumeStacksAndLock()
    {
        _localConsumedThisTurn = true;
        var tracker = Owner.GetPower<MarkofthePrescriptPower>();
        if (tracker != null)
        {
            tracker.HermesConsumedThisTurn = true;
        }

        DynamicVars["HermesAmount"].BaseValue = 0;
        InvokeDisplayAmountChanged();
        
        await Task.CompletedTask;
    }

    private async Task ProcessThreshold()
    {
        if (Owner.Player == null || _isGranting) return;

        if (DynamicVars["HermesAmount"].IntValue < 9) return;

        var handPile = PileType.Hand.GetPile(Owner.Player);
        bool hasFurioso = handPile.Cards.Any(c =>
            c is FuriosoReplica or FuriosoCrescendo or FuriosoLacrimosaCrescendo);

        if (!hasFurioso)
        {
            _isGranting = true;
            try
            {
                await GrantThresholdRewards();
            }
            finally
            {
                _isGranting = false;
            }
        }
    }

    private async Task GrantThresholdRewards()
    {
        if (Owner.Player == null) return;
        
        await TaskHelper.RunSafely(PowerCmd.Apply<GraceofthePrescriptPower>(Owner, 3, Owner, null));
        
        DynamicVars["HermesThresholdCount"].BaseValue++;
        int thresholdCount = DynamicVars["HermesThresholdCount"].IntValue;

        CardModel? cardTemplate;
        switch (thresholdCount)
        {
            case 1:
                cardTemplate = ModelDb.Card<FuriosoReplica>();
                break;
            case 2:
                cardTemplate = ModelDb.Card<FuriosoCrescendo>();
                await PowerCmd.Apply<IndulgenceInPrescript>(Owner, 1, Owner, null);
                break;
            default:
                cardTemplate = ModelDb.Card<FuriosoLacrimosaCrescendo>();
                await PowerCmd.Apply<IndulgenceInPrescript>(Owner, 1, Owner, null);
                break;
        }

        if (cardTemplate != null && Owner.CombatState != null)
        {
            CardModel cardToGive = Owner.CombatState.CreateCard(cardTemplate, Owner.Player);
            await CardPileCmd.AddGeneratedCardToCombat(cardToGive, PileType.Hand, addedByPlayer: true);
        }
    }
}
