using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Extensions;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Cards.Common;
using RienSang.RienSangCode.Cards.Rare;

namespace RienSang.RienSangCode.Cards.Uncommon;

[Pool(typeof(RienSangCardPool))]
public class TheWillOfHermes : RienSangCard
{
    public TheWillOfHermes() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new("KarmaLoss", 20m),
        new EnergyVar(4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TheOraclesProxyPower>(),
        HoverTipFactory.FromPower<KarmicConsequence>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int energyGain = DynamicVars.Energy.IntValue;
        if (energyGain > 0)
        {
            await PlayerCmd.GainEnergy(energyGain, Owner);
        }
        
        await Owner.Creature.ModifyKarma(choiceContext, -DynamicVars["KarmaLoss"].BaseValue, Owner.Creature, this);

        if (Owner.Creature.CombatState != null)
        {
            ICombatState combatState = Owner.Creature.CombatState;
            if (combatState == null) return;

            using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
            {
                for (int cardsPlayed = 0; cardsPlayed < 13; cardsPlayed++)
                {
                    if (CombatManager.Instance.IsOverOrEnding) break;

                    CardPile hand = PileType.Hand.GetPile(Owner);
                
                    if (hand.Cards.FirstOrDefault(c => c.CanPlay() && c != this && !IsExcluded(c)) is not CardModel card) break;

                    Creature? target = GetTarget(card, combatState);
                
                    await card.SpendResources();
                    await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: true);
                }
            }
        }
    }

    private bool IsExcluded(CardModel card)
    {
        if (card is RienSangCard rienSangCard && rienSangCard.GainsKarma) return true;
        return card is Tradeoff or AsThePrescriptOrdered or Reconstruct;
    }

    private Creature? GetTarget(CardModel card, ICombatState combatState)
    {
        Rng combatTargets = Owner.RunState.Rng.CombatTargets;
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(), 
            TargetType.AnyAlly => combatTargets.NextItem(combatState.Allies.Where(c => c != null && c.IsAlive && c.IsPlayer && c != Owner.Creature)), 
            TargetType.AnyPlayer => Owner.Creature, 
            _ => null, 
        };
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KarmaLoss"].UpgradeValueBy(10);
        EnergyCost.UpgradeBy(-1);
    }
}