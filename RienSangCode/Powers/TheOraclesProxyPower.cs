using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics; 
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Cards.Ancient;
using RienSang.RienSangCode.Cards.Basic;
using RienSang.RienSangCode.Cards.Common;
using RienSang.RienSangCode.Cards.Rare;
using RienSang.RienSangCode.Cards.Uncommon;

namespace RienSang.RienSangCode.Powers;

[Pool(typeof(RienSangCardPool))]
public class TheOraclesProxyPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public int HealAmount 
    {
        get => (int)DynamicVars["HealAmount"].BaseValue;
        set => DynamicVars["HealAmount"].BaseValue = value;
    }

    public int EnergyGain 
    {
        get => (int)DynamicVars["EnergyGain"].BaseValue;
        set => DynamicVars["EnergyGain"].BaseValue = value;
    }
    
    public bool IsActive { get; set; } = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("HealAmount", 0m),
        new("EnergyGain", 0m)
    ];

    public TheOraclesProxyPower() : base() { }
    public TheOraclesProxyPower(int turns, int heal, int energy) : base() 
    { 
        SetAmount(turns); 
        HealAmount = heal;
        EnergyGain = energy;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return;

        IsActive = true;

        if (HealAmount > 0)
        {
            Flash();
            await CreatureCmd.Heal(Owner, HealAmount);
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || EnergyGain <= 0) return;
        
        await PlayerCmd.GainEnergy(EnergyGain, player);
    }

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        if (player.Creature.CombatState != null)
        {
            ICombatState combatState = player.Creature.CombatState;
            if (combatState == null) return;

            using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
            {
                for (int cardsPlayed = 0; cardsPlayed < 13; cardsPlayed++)
                {
                    if (CombatManager.Instance.IsOverOrEnding) break;

                    CardPile hand = PileType.Hand.GetPile(player);
                    
                    if (hand.Cards.FirstOrDefault(c => c.CanPlay() && !IsExcluded(c)) is not CardModel card) break;

                    Creature? target = GetTarget(card, (CombatState)combatState, player);
                
                    await card.SpendResources();
                    await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
                }
            }
        }

        if (base.Amount > 1)
        {
            await PowerCmd.Decrement(this);
        }
        else
        {
            await PowerCmd.Remove(this);
        }
    }

    private bool IsExcluded(CardModel card)
    {
        if (card is RienSangCard rienSangCard && rienSangCard.GainsKarma) return true;
        return card is Tradeoff or AsThePrescriptOrdered or Reconstruct or ByUnpredictableWhim or ByGodsWill or TheWillOfHermes or Obedience or TheOraclesProxy;
    }

    private Creature? GetTarget(CardModel card, CombatState combatState, Player player)
    {
        Rng combatTargets = player.RunState.Rng.CombatTargets;
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(), 
            TargetType.AnyAlly => combatTargets.NextItem(combatState.Allies.Where(c => c != null && c.IsAlive && c.IsPlayer && c != player.Creature)), 
            TargetType.AnyPlayer => player.Creature, 
            _ => null, 
        };
    }
}