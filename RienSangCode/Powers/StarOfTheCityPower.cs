using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Extensions;

namespace RienSang.RienSangCode.Powers;

public sealed class StarOfTheCityPower : RienSangPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.None;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new("SinkingPotency", 5),
        new("SinkingCount", 2)
    };
    
    public StarOfTheCityPower() : base()
    {
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }

        Flash();
        int sinkingPotency = 5;
        int sinkingCount = 2;

        var enemies = combatState.HittableEnemies;
        foreach (var enemy in enemies)
        {
            await LCSinkingPower.Apply(new ThrowingPlayerChoiceContext(), enemy, sinkingCount, sinkingPotency, Owner, null);
        }
    }
}