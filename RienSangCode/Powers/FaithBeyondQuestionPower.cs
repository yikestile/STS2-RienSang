using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using LimbusCore.LimbusCoreCode.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;

namespace RienSang.RienSangCode.Powers;

public class FaithBeyondQuestionPower : RienSangPower, IHasSecondAmount
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public int amount2; 

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new("Potency", 0m) 
    };

    public int Potency
    {
        get => amount2;
        set
        {
            if (amount2 == value) return;
            amount2 = value;
            InvokeDisplayAmountChanged();
        }
    }

    public FaithBeyondQuestionPower() : base()
    {
    }

    public static async Task Apply(PlayerChoiceContext context, Creature target, int amount, Creature? applier, CardModel? source)
    {
        await PowerCmd.Apply<FaithBeyondQuestionPower>(context, target, (decimal)amount, applier, source);
    }

    public string GetSecondAmount() => Potency.ToString();

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Tags.Contains(RienSangTags.BlindFaith))
        {
            Flash();

            Potency += Amount; 
            UpdateDynamicVars();
        }
        await Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && cardSource != null && cardSource.Tags.Contains(RienSangTags.BlindFaith))
        {
            return (decimal)amount2;
        }
        return 0m;
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        UpdateDynamicVars();
        await Task.CompletedTask;
    }

    private void UpdateDynamicVars()
    {
        if (DynamicVars == null) return;
        DynamicVars["Potency"].BaseValue = amount2;
    }
}
