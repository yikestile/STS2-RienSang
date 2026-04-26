using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Powers;

namespace RienSang.RienSangCode.Extensions;

public static class KarmaExtensions
{
    public static async Task ApplyKarma(this Creature creature, PlayerChoiceContext choiceContext, decimal amount, Creature? applier = null, CardModel? cardSource = null)
    {
        if (creature.HasPower<KarmicConsequenceFortuna>())
        {
            await PowerCmd.Apply<KarmicConsequenceFortuna>(choiceContext, creature, amount, applier, cardSource);
        }
        else
        {
            await PowerCmd.Apply<KarmicConsequence>(choiceContext, creature, amount, applier, cardSource);
        }
    }

    public static async Task ModifyKarma(this Creature creature, PlayerChoiceContext choiceContext, decimal amount, Creature? applier = null, CardModel? cardSource = null)
    {
        var fortuna = creature.GetPower<KarmicConsequenceFortuna>();
        if (fortuna != null)
        {
            await PowerCmd.ModifyAmount(choiceContext, fortuna, amount, applier, cardSource);
            return;
        }

        var regular = creature.GetPower<KarmicConsequence>();
        if (regular != null)
        {
            await PowerCmd.ModifyAmount(choiceContext, regular, amount, applier, cardSource);
        }
    }
    
    public static decimal GetKarmaAmount(this Creature creature)
    {
        var fortuna = creature.GetPower<KarmicConsequenceFortuna>();
        if (fortuna != null) return (decimal)fortuna.DisplayAmount;

        var regular = creature.GetPower<KarmicConsequence>();
        return regular?.Amount ?? 0m;
    }
}