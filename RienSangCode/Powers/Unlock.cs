using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
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
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;

namespace RienSang.RienSangCode.Powers;

public class Unlock : RienSangPower
{
    private static readonly SpireField<Creature, bool> _hermes9ReachedThisCombat = new SpireField<Creature, bool>(() => false);

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

        if (oldAmount < 3 && newAmount >= 3)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 1, Owner, null);
        }

        await UpdateShinFate(choiceContext);
        Flash();
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        await UpdateShinFate(choiceContext);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            await UpdateShinFate(new ThrowingPlayerChoiceContext());
        }
    }

    public static async Task CheckShinRequirement(Creature creature, PlayerChoiceContext? context = null)
    {
        var unlock = creature.GetPower<Unlock>();
        if (unlock != null)
        {
            await unlock.UpdateShinFate(context ?? new ThrowingPlayerChoiceContext());
        }
    }

    public async Task UpdateShinFate(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || Owner.IsDead || Owner.Player == null) return;

        var hermes = Owner.GetPower<ProcurationHermes>();
        if (hermes != null && hermes.DisplayAmount == 9)
        {
            _hermes9ReachedThisCombat[Owner] = true;
        }

        bool unlock3 = (int)Amount >= 3;
        bool hermesReached = _hermes9ReachedThisCombat[Owner];
        float sp = SanityManager.GetSanity(Owner.Player);

        bool canGainShin = unlock3 && hermesReached && sp >= 0;

        var currentShin = Owner.GetPower<ShinFate>();

        if (canGainShin && currentShin == null)
        {
            await PowerCmd.Apply<ShinFate>(choiceContext, Owner, 1, Owner, null);
            Flash();
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
        _hermes9ReachedThisCombat[Owner] = false;
    }
}