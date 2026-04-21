using System.Threading.Tasks;
using System.Linq;
using LimbusCore.LimbusCoreCode.Mechanics;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using RienSang.RienSangCode.Cards;
using RienSang.RienSangCode.Powers;
using RienSang.RienSangCode.Character;
using Godot;

namespace RienSang.RienSangCode.Mechanics;

public static class CaduceusManager
{
    public static int HatchetUsesTurn = 0;
    public static int StilettoUsesTurn = 0;
    public static int BastardSwordUsesTurn = 0;
    public static int RapierUsesTurn = 0;
    public static int HammerUsesTurn = 0;
    public static int GreatSwordUsesTurn = 0;
    public static int LanceUsesTurn = 0;
    public static int WhipUsesTurn = 0;
    public static int ScytheUsesTurn = 0;
    
    public static int GlobalAnimIndex = 0;

    public static void ResetTurnCounters()
    {
        HatchetUsesTurn = 0;
        StilettoUsesTurn = 0;
        BastardSwordUsesTurn = 0;
        RapierUsesTurn = 0;
        HammerUsesTurn = 0;
        GreatSwordUsesTurn = 0;
        LanceUsesTurn = 0;
        WhipUsesTurn = 0;
        ScytheUsesTurn = 0;
    }

    public static async Task<AttackCommand?> Execute(CardModel card, Creature? target, decimal baseDamage, PlayerChoiceContext context, int hitIndex, LimbusDamageType preferredType = LimbusDamageType.None, float biasChance = 0f, bool isFuriosoFinisher = false, int forcedWeaponRoll = 0)
    {
        var player = card.Owner.Creature;
        
        if (hitIndex == 0)
        {
            await PowerCmd.Apply<ProcurationHermes>(player, 1, player, null);
        }

        if (player.CombatState != null)
        {
            var rng = player.CombatState.RunState.Rng.Niche;
            int weaponRoll;

            if (forcedWeaponRoll > 0)
            {
                weaponRoll = forcedWeaponRoll;
            }
            else if (isFuriosoFinisher)
            {
                weaponRoll = 9; 
            }
            else if (preferredType != LimbusDamageType.None && rng.NextFloat() < biasChance)
            {
                weaponRoll = GetRandomWeaponOfType(preferredType, rng);
            }
            else
            {
                weaponRoll = rng.NextInt(1, 10);
            }
        
            var damageMultiplier = 1.0m;
            var vfx = "vfx/vfx_attack_slash";
            var damageType = LimbusDamageType.None; 
            string animTrigger = "";
        
            switch (weaponRoll)
            {
                case 1: // Hatchet (Blunt)
                    vfx = "vfx/vfx_attack_blunt";
                    damageType = LimbusDamageType.Blunt;
                    animTrigger = $"attack_hatchet_{(GlobalAnimIndex % 3) + 1}";
                    GlobalAnimIndex++;
                    HatchetUsesTurn++;
                    var poiseInstance = await PowerCmd.Apply<LCPoisePower>(player, 1m, player, null);
                    poiseInstance?.AddPotency(2);
                    break;

                case 2: // Stiletto (Pierce)
                    vfx = "vfx/vfx_attack_slash"; 
                    damageType = LimbusDamageType.Pierce;
                    animTrigger = $"attack_stiletto_{(GlobalAnimIndex % 3) + 1}";
                    GlobalAnimIndex++;
                    StilettoUsesTurn++;
                    if (target != null)
                    {
                        var sinkingInstance = await PowerCmd.Apply<LCSinkingPower>(target, 1m, player, null);
                        sinkingInstance?.AddPotency(2);
                    }
                    break;

                case 3: // Bastard Sword (Slash)
                    damageType = LimbusDamageType.Slash;
                    damageMultiplier = 1.05m;
                    animTrigger = $"attack_bastardsword_{(GlobalAnimIndex % 3) + 1}";
                    if (BastardSwordUsesTurn < 2) {
                        await PowerCmd.Apply<LCStrengthNextTurn>(player, 1, player, null);
                    }
                    GlobalAnimIndex++;
                    BastardSwordUsesTurn++;
                    break;

                case 4: // Rapier (Pierce)
                    damageType = LimbusDamageType.Pierce;
                    damageMultiplier = 1.05m;
                    vfx = "vfx/vfx_attack_slash"; 
                    animTrigger = $"attack_rapier_{(GlobalAnimIndex % 3) + 1}";
                    if (RapierUsesTurn < 2) {
                        await PowerCmd.Apply<LCWeakNextTurn>(target!, 1, player, null);
                    }
                    GlobalAnimIndex++;
                    RapierUsesTurn++;
                    break;

                case 5: // Hammer (Blunt)
                    damageType = LimbusDamageType.Blunt;
                    damageMultiplier = 1.05m;
                    vfx = "vfx/vfx_attack_blunt";
                    animTrigger = $"attack_hammer_{(GlobalAnimIndex % 3) + 1}";
                    GlobalAnimIndex++;
                    HammerUsesTurn++;
                    if (target != null && target.Block > 0)
                    {
                        await CreatureCmd.LoseBlock(target, target.Block);
                    }
                    break;

                case 6: // Great Sword (Slash)
                    damageType = LimbusDamageType.Slash;
                    damageMultiplier = 1.15m;
                    animTrigger = $"attack_greatsword_{(GlobalAnimIndex % 3) + 1}";
                    if (GreatSwordUsesTurn < 2) {
                        await PowerCmd.Apply<LCSlashFragility>(target!, 1, player, null);
                    }
                    GlobalAnimIndex++;
                    GreatSwordUsesTurn++;
                    break;

                case 7: // Lance (Pierce)
                    damageType = LimbusDamageType.Pierce;
                    damageMultiplier = 1.15m;
                    vfx = "vfx/vfx_attack_slash"; 
                    animTrigger = $"attack_lance_{(GlobalAnimIndex % 3) + 1}";
                    if (LanceUsesTurn < 2) {
                        await PowerCmd.Apply<LCPierceFragility>(target!, 1, player, null);
                    }
                    GlobalAnimIndex++;
                    LanceUsesTurn++;
                    break;

                case 8: // Whip (Blunt)
                    damageType = LimbusDamageType.Blunt;
                    damageMultiplier = 1.15m;
                    vfx = "vfx/vfx_attack_blunt";
                    animTrigger = $"attack_whip_{(GlobalAnimIndex % 3) + 1}";
                    if (WhipUsesTurn < 2) {
                        await PowerCmd.Apply<LCBluntFragility>(target!, 1, player, null);
                    }
                    GlobalAnimIndex++;
                    WhipUsesTurn++;
                    break;

                case 9: // Scythe (Slash)
                    damageType = LimbusDamageType.Slash;
                    damageMultiplier = 1.30m;
                    if (isFuriosoFinisher)
                    {
                        animTrigger = "FuriosoFinish";
                    }
                    else
                    {
                        animTrigger = $"attack_scythe_{(GlobalAnimIndex % 3) + 1}";
                        GlobalAnimIndex++;
                    }
                    ScytheUsesTurn++;
                    var scythePoise = await PowerCmd.Apply<LCPoisePower>(player, 1, player, null);
                    scythePoise?.AddPotency(0);
                    scythePoise?.ForceCrit();
                    vfx = "vfx/vfx_attack_slash";
                    break;
            }
        
            if (card is RienSangCard rienSangCard)
            {
                rienSangCard.CurrentDamageType = damageType;
            }
            
            decimal finalDamage = baseDamage * damageMultiplier;
            var oraclePower = player.GetPower<OracleDevicePower>();
            if (oraclePower != null)
            {
                finalDamage += oraclePower.Amount;
            }
            
            if (card.Owner.Character is Character.RienSang character && target != null)
            {
                if (hitIndex == 0 || Character.RienSang.LastDashTarget[player] != target) 
                {
                    await character.DashTo(player, target, 0.4f);
                }
    
                if (!string.IsNullOrEmpty(animTrigger))
                {
                    character.PrepareVisualsForAction(player, target);
                    var (totalLength, impactDelays) = character.PlayAnimation(player, animTrigger);
                    
                    if (impactDelays.Length > 1)
                    {
                        decimal subHitDamage = finalDamage / impactDelays.Length;
                        float lastWait = 0f;

                        for (int i = 0; i < impactDelays.Length; i++)
                        {
                            float currentWait = impactDelays[i] - lastWait;
                            if (currentWait > 0) await Task.Delay((int)(currentWait * 1000));
                            
                            var subCmd = DamageCmd.Attack(subHitDamage).FromCard(card).WithHitFx(vfx);
                            await subCmd.Targeting(target).Execute(context);
                            
                            lastWait = impactDelays[i];
                        }
                        
                        float remainingWait = totalLength - lastWait;
                        if (remainingWait > 0) await Task.Delay((int)(remainingWait * 1000));
                    }
                    else if (impactDelays.Length == 1)
                    {
                        await Task.Delay((int)(impactDelays[0] * 1000));
                        var cmd = DamageCmd.Attack(finalDamage).FromCard(card).WithHitFx(vfx);
                        await cmd.Targeting(target).Execute(context);
                        
                        float remainingWait = totalLength - impactDelays[0];
                        if (remainingWait > 0) await Task.Delay((int)(remainingWait * 1000));
                    }
                    else
                    {
                        await Task.Delay((int)(totalLength * 1000));
                    }
                }

                int totalHits = card.DynamicVars.ContainsKey("Repeat") ? card.DynamicVars.Repeat.IntValue : 1;
                if (hitIndex == totalHits - 1)
                {
                    await character.ReturnToIdlePosition(player, 0.3f);
                }
                
                return null;
            }
        }
        return null;
    }

    private static int GetRandomWeaponOfType(LimbusDamageType type, Rng nicheRng)
    {
        return type switch
        {
            LimbusDamageType.Blunt => nicheRng.NextItem([1, 5, 8]), 
            LimbusDamageType.Pierce => nicheRng.NextItem([2, 4, 7]), 
            LimbusDamageType.Slash => nicheRng.NextItem([3, 6, 9]),  
            _ => nicheRng.NextInt(1, 10)
        };
    }
}