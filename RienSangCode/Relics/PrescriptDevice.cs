using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using RienSang.RienSangCode.Character;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Relics;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Relics;

[Pool(typeof(RienSangRelicPool))]
public class PrescriptDevice : RienSangRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<MarkofthePrescriptPower>(1m), 
        new PowerVar<ThePrescriptsTarget>(1m),
        new PowerVar<WoundcasingMask>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<MarkofthePrescriptPower>(),
        HoverTipFactory.FromPower<ThePrescriptsTarget>(),
        HoverTipFactory.FromPower<WoundcasingMask>(),
    ];
    
    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature == null) return;
        
        Flash();

        await PowerCmd.Apply<MarkofthePrescriptPower>(Owner.Creature, 1, Owner.Creature, null);
        await PowerCmd.Apply<WoundcasingMask>(Owner.Creature, 1, Owner.Creature, null);
    }
    
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner?.Creature == null) return;
        
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return; 

        var potentialTargets = combatState.HittableEnemies;
        
        if (potentialTargets is { Count: > 0 })
        {
            var target = potentialTargets.OrderBy(_ => Owner.RunState.Rng.Niche.NextFloat()).First();
            
            await PowerCmd.Apply<ThePrescriptsTarget>((Creature)target, 1, Owner.Creature, null);
        }
    }
}