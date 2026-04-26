using System.Collections.Generic;
using BaseLib.Abstracts;
using RienSang.RienSangCode.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using RienSang.RienSangCode.Cards.Basic;
using RienSang.RienSangCode.Cards.EGO;
using RienSang.RienSangCode.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using LimbusCore.LimbusCoreCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Threading.Tasks;
using System.Linq;
using MegaCrit.Sts2.Core.Helpers;
using System;
using RienSang.RienSangCode.Cards.Ancient;
using RienSang.RienSangCode.Cards.Common;
using RienSang.RienSangCode.Cards.Curse;
using RienSang.RienSangCode.Cards.Rare;
using RienSang.RienSangCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.Entities.Players;
using RienSang.RienSangCode.Mechanics;

namespace RienSang.RienSangCode.Character;

public class RienSang : CustomCharacterModel
{
    public const string CharacterId = "RienSang";

    public static readonly Color Color = new("31c2ee");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 66;

public override IEnumerable<CardModel> StartingDeck =>
[
  // Basic
  ModelDb.Card<ByUnpredictableWhim>(),
  ModelDb.Card<FollowingThePrescript>(),
  // Common
  ModelDb.Card<Faith>(),
  ModelDb.Card<Tradeoff>(),
  ModelDb.Card<BlindFaith>(),
  ModelDb.Card<Multislash>(),
  ModelDb.Card<SenseQuarry>(),
  ModelDb.Card<BindingChain>(),
  ModelDb.Card<WillOfTheCity>(),
  ModelDb.Card<TheIndexsBlade>(),
  ModelDb.Card<PoisedBreathing>(),
  ModelDb.Card<DeliverPrescripts>(),
  ModelDb.Card<SomberProcuration>(),
  ModelDb.Card<WillOfThePrescript>(),
  ModelDb.Card<UndertakePrescripts>(),
  ModelDb.Card<AsThePrescriptOrdered>(),
  ModelDb.Card<ToWhereThePrescriptPoints>(),
  ModelDb.Card<AimTowardAPointLetItEchoWithin>(),
  ModelDb.Card<SwingToFellHaveItMeetTheGround>(),
  ModelDb.Card<CarveAtALowSlantPeelWhatRemains>(),
  ModelDb.Card<SlamDownWithWeightToppleTheBody>(),
  ModelDb.Card<LayVerticalTheEndInsertUpToTheWick>(),
  ModelDb.Card<LayTheBladeOnItsSideSliceLikeASeveredBreath>(),
  // Uncommon
  ModelDb.Card<Execute>(),
  ModelDb.Card<Eliminate>(),
  ModelDb.Card<Obedience>(),
  ModelDb.Card<RimeShank>(),
  ModelDb.Card<Enwrap330Times>(),
  ModelDb.Card<ToDecideMyFate>(),
  ModelDb.Card<DefensiveStance>(),
  ModelDb.Card<RecitePrescript>(),
  ModelDb.Card<TheWillOfHermes>(),
  ModelDb.Card<EquivalentExchange>(),
  ModelDb.Card<ProcurationEngrave>(),
  ModelDb.Card<SanguinePointillism>(),
  ModelDb.Card<TheIndexNursefather>(),
  ModelDb.Card<AsThePrescriptDemands>(),
  ModelDb.Card<RaiseAndLaughTheBlade>(),
  ModelDb.Card<ImpaleInVoicelessSorrow>(),
  ModelDb.Card<InLongSwathsOfFrozenBlood>(),
  ModelDb.Card<RevelWithSoundlessApplause>(),
  ModelDb.Card<WithTemperedSecretSeverTheForm>(),
  ModelDb.Card<DestroyTheSoundCrushFlatTheThought>(),
  ModelDb.Card<StabTheHeartOfSilencePenetrateTheMemory>(),
  ModelDb.Card<CryTheWaterfall>(),
  ModelDb.Card<AbsoluteFaith>(),
  ModelDb.Card<Atonement>(),
  ModelDb.Card<EnforcingPrescript>(),
  ModelDb.Card<Weave>(),
  ModelDb.Card<WeaknessExploit>(),
  ModelDb.Card<ThickVapor>(),
  ModelDb.Card<PoisedWarding>(),
  ModelDb.Card<SteadyTheBreath>(),
  ModelDb.Card<ThisWillDo>(),
  // Rare
  ModelDb.Card<UnlockI>(),
  ModelDb.Card<UnlockII>(),
  ModelDb.Card<UnlockIII>(),
  ModelDb.Card<GraceOfGod>(),
  ModelDb.Card<Reconstruct>(),
  ModelDb.Card<SinkingDeluge>(),
  ModelDb.Card<StarOfTheCity>(),
  ModelDb.Card<TheOraclesProxy>(),
  ModelDb.Card<PrecognitionReplica>(),
  ModelDb.Card<OracleDeviceCaduceus>(),
  ModelDb.Card<ProcurationAnnihilate>(),
  ModelDb.Card<GodsBlessing>(),
  ModelDb.Card<DeepBreath>(),
  ModelDb.Card<SorsSalutis>(),
  ModelDb.Card<SorsImmanis>(),
  ModelDb.Card<OFortuna>(),
  ModelDb.Card<DivineProtection>(),
  ModelDb.Card<CompulsoryOffering>(),
  ModelDb.Card<Volition>(),
  ModelDb.Card<Conviction>(),
  ModelDb.Card<FaithBeyondQuestion>(),
  ModelDb.Card<GodsFavor>(),
  // Ancient
  ModelDb.Card<GloomInRuins>(),
  ModelDb.Card<ByGodsWill>(),
  // EGO
  ModelDb.Card<Sunshower>(),
  ModelDb.Card<BygoneDays>(),
  ModelDb.Card<FellBullet>(),
  ModelDb.Card<CrowsEyeView>(),
  ModelDb.Card<WishingCairn>(),
  ModelDb.Card<FourthMatchFlame>(),
  ModelDb.Card<DimensionShredder>(),
  ModelDb.Card<GreatTrichiliocosm>(),
  // Curse
  ModelDb.Card<Fpoon>(),
  ModelDb.Card<Spork>(),
  ModelDb.Card<PrescriptIncomplianceRisk>()
];

    
    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<PrescriptDevice>()];


    public override CardPoolModel CardPool => ModelDb.CardPool<RienSangCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<RienSangRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<RienSangPotionPool>();

    public override CustomEnergyCounter? CustomEnergyCounter => 
        new CustomEnergyCounter(EnergyCounterPaths, new Color(0.69f, 0.67f, 0.55f), new Color(1f, 1f, 1f));
    
    public override string CustomVisualPath => "res://RienSang/scenes/riensang/riensang.tscn";
    public override string CustomCharacterSelectBg => "res://RienSang/scenes/riensang/char_select_bg_riensang.tscn";
    public override string CustomIconPath => "res://RienSang/scenes/riensang/riensang_icon.tscn";
    public override string CustomIconTexturePath => "char_icon_riensang.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_riensang.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_riensang_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_riensang.png".CharacterUiPath();
    
    public override string CustomRestSiteAnimPath => "res://RienSang/scenes/riensang/riensang_rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://RienSang/scenes/riensang/riensang_merchant.tscn";
    
    public override string CustomCharacterSelectTransitionPath =>
        "res://RienSang/images/riensang/transitions/riensang_transition_mat.tres";
    
    public override string CustomTrailPath => "res://RienSang/scenes/riensang/card_trail_riensang.tscn";
    
    public override CreatureAnimator? GenerateAnimator(MegaSprite controller)
    {
        return null; 
    }

    private static readonly SpireField<Creature, Vector2?> _originalPositions = new SpireField<Creature, Vector2?>(() => null);
    public static readonly SpireField<Creature, Creature?> LastDashTarget = new SpireField<Creature, Creature?>(() => null);

    public void PrepareVisualsForAction(Creature creature, Creature target, bool isBehindAttack = false)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node?.Visuals == null || targetNode == null) return;

        var visuals = node.Visuals.GetNodeOrNull<Sprite2D>("Visuals");
        if (visuals != null)
        {
            const float baseScale = 0.676f;
            
            bool shouldFaceLeft = node.GlobalPosition.X > targetNode.GlobalPosition.X;
            
            visuals.Scale = new Vector2(shouldFaceLeft ? -baseScale : baseScale, baseScale);
            visuals.Position = Vector2.Zero;
        }
    }

    public void DoScreenShake(ShakeStrength strength = ShakeStrength.Medium, ShakeDuration duration = ShakeDuration.Short)
    {
        NGame.Instance?.ScreenShake(strength, duration);
    }

    public (float total, float[] impacts) PlayAnimation(Creature creature, string trigger)
    {
        if (creature == null || string.IsNullOrEmpty(trigger)) return (0f, []);

        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node?.Visuals == null) return (0f, []);

        var animPlayer = node.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (animPlayer != null)
        {
            string godotTrigger = trigger.ToLowerInvariant() switch {
                "hit" => "hurt",
                "idle" => "idle_loop",
                "dead" => "die",
                "cast" => "cast",
                "block" => "block",
                "evade" => "evade",
                "dash" => "dash_forward",
                _ => trigger
            };

            if (animPlayer.HasAnimation(godotTrigger))
            {
                var originalAnim = animPlayer.GetAnimation(godotTrigger);
                var anim = (Animation)originalAnim.Duplicate();
    
                var visuals = node.Visuals.GetNodeOrNull<Sprite2D>("Visuals");

                if (visuals != null)
                {
                    bool isFlipped = visuals.Scale.X < 0;
                    float multiplier = isFlipped ? -1f : 1f;

                    int trackCount = anim.GetTrackCount();
                    for (int i = 0; i < trackCount; i++)
                    {
                        if (anim.TrackGetPath(i) == "Visuals:position")
                        {
                            int keyCount = anim.TrackGetKeyCount(i);
                            for (int k = 0; k < keyCount; k++)
                            {
                                Vector2 originalPos = (Vector2)anim.TrackGetKeyValue(i, k);
                                float newX = originalPos.X * multiplier;
                                anim.TrackSetKeyValue(i, k, new Vector2(newX, originalPos.Y));
                            }
                        }
                    }
                }

                var library = animPlayer.GetAnimationLibrary("");
                string tempName = godotTrigger + "_temp";
    
                if (library.HasAnimation(tempName)) library.RemoveAnimation(tempName);
                library.AddAnimation(tempName, anim);

                float totalLength = anim.Length;
                float[] impactDelays = GetImpactDelays(godotTrigger, totalLength);

                animPlayer.Play(tempName);
    
                if (godotTrigger != "idle_loop" && godotTrigger != "die")
                {
                    animPlayer.Queue("idle_loop");
                }
    
                return (totalLength, impactDelays);
            }
        }
        return (0f, []);
    }

    private float[] GetImpactDelays(string animName, float totalLength)
    {
        return animName switch
        {
            "attack_hammer_1" => [0.5f],
            "attack_hammer_2" => [0.35f],
            "attack_hammer_3" => [0.5f],
            "attack_hatchet_1" => [0.4f],
            "attack_hatchet_2" => [0.3f],
            "attack_hatchet_3" => [0.3f],
            "attack_bastardsword_1" => [0.1f],
            "attack_bastardsword_2" => [0.1f],
            "attack_bastardsword_3" => [0.1f],
            "attack_greatsword_1" => [0.1f],
            "attack_greatsword_2" => [0.1f],
            "attack_greatsword_3" => [0.1f],
            "attack_lance_1" => [0.1f],
            "attack_lance_2" => [0.1f],
            "attack_lance_3" => [0.4f],
            "attack_rapier_1" => [0.1f],
            "attack_rapier_2" => [0.1f],
            "attack_rapier_3" => [0.1f],
            "attack_scythe_1" => [0.1f],
            "attack_scythe_2" => [0.1f],
            "attack_scythe_3" => [0.1f],
            "attack_stiletto_1" => [0.1f],
            "attack_stiletto_2" => [0.1f],
            "attack_stiletto_3" => [0.1f],
            "attack_whip_1" => [0.1f],
            "attack_whip_2" => [0.1f],
            "attack_whip_3" => [0.1f],
            "FuriosoFinish" => [0.1f],
            _ => [totalLength * 0.5f] 
        };
    }

    public async Task DashTo(Creature creature, Creature target, float duration, bool dashBehind = false)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || targetNode == null) return;

        if (_originalPositions[creature] == null)
        {
            _originalPositions[creature] = node.GlobalPosition;
        }

        LastDashTarget[creature] = target;
        PlayAnimation(creature, "dash");

        var tween = node.CreateTween();
        
        Vector2 offsetDir = (creature.Side == CombatSide.Player) ? Vector2.Left : Vector2.Right;
        if (dashBehind) offsetDir = -offsetDir;

        Vector2 targetPos = targetNode.GlobalPosition + offsetDir * 200f;
        tween.TweenProperty(node, "global_position", targetPos, duration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        
        await Task.Delay((int)(duration * 1000));
    }

    public async Task ReturnToIdlePosition(Creature creature, float duration)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node == null || !_originalPositions[creature].HasValue) return;

        var tween = node.CreateTween();
    
        tween.TweenProperty(node, "global_position", _originalPositions[creature]!.Value, duration)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
    
        await Task.Delay((int)(duration * 1000));
    
        _originalPositions[creature] = null;
        LastDashTarget[creature] = null;

        var visuals = node.Visuals.GetNodeOrNull<Sprite2D>("Visuals");
        if (visuals != null)
        {
            const float baseScale = 0.676f;
        
            float targetXScale = (creature.Side == CombatSide.Player) ? baseScale : -baseScale;

            var flipTween = node.CreateTween();
            flipTween.TweenProperty(visuals, "scale", new Vector2(targetXScale, baseScale), 0.1f)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            
            visuals.Position = Vector2.Zero;
        }
    }
    
    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt", "vfx/vfx_heavy_blunt", "vfx/vfx_attack_slash", "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }
    
    private string EnergyCounterPaths(int i)
    {
        return "res://RienSang/images/ui/combat/energy_counters/riensang/limbus_orb_layer.png";
    }

    public static int CountUniqueCaduceusCards(Player player)
    {
        HashSet<ModelId> uniqueCaduceusCards = new HashSet<ModelId>();

        AddCaduceusCardsFromPile(PileType.Hand.GetPile(player), uniqueCaduceusCards);
        AddCaduceusCardsFromPile(PileType.Draw.GetPile(player), uniqueCaduceusCards);
        AddCaduceusCardsFromPile(PileType.Discard.GetPile(player), uniqueCaduceusCards);
        AddCaduceusCardsFromPile(PileType.Exhaust.GetPile(player), uniqueCaduceusCards);
        
        return uniqueCaduceusCards.Count;
    }

    private static void AddCaduceusCardsFromPile(CardPile pile, HashSet<ModelId> uniqueIds)
    {
        if (pile == null) return;
        foreach (var card in pile.Cards)
        {
            if (card.CanonicalKeywords.Contains(RienSangKeywords.Caduceus))
            {
                uniqueIds.Add(card.Id);
            }
        }
    }
    
    [HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
    public static class NCreatureSetTriggerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NCreature __instance, string trigger)
        {
            if (__instance.Entity?.Player?.Character is RienSang character)
            {
                character.PlayAnimation(__instance.Entity, trigger);
                return false; 
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
    public static class StartDeathAnimPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCreature __instance, ref float __result)
        {
            if (__instance.Entity?.Player?.Character is RienSang character)
            {
                character.PlayAnimation(__instance.Entity, "dead");
                
                var animPlayer = __instance.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                float duration = animPlayer?.GetAnimation("die")?.Length ?? 1.5f;
            
                __result = duration;

                __instance.ZIndex = 0;
                __instance.Visuals.ZIndex = 0;
            }
        }
    }
    
    [HarmonyPatch(typeof(NCombatRoom), "CreateAllyNodes")]
    public static class CaptureInitialPositionPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            var room = NCombatRoom.Instance;
            if (room == null) return;

            foreach (var node in room.CreatureNodes)
            {
                if (node.Entity?.Player?.Character is RienSang)
                {
                    _originalPositions[node.Entity] = node.GlobalPosition;
                    LastDashTarget[node.Entity] = null;
                }
            }
        }
    }

    [HarmonyPatch(typeof(NFakeMerchant), "AfterRoomIsLoaded")]
    public static class FakeMerchantLayeringPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NFakeMerchant __instance)
        {
            var container = AccessTools.Field(typeof(NFakeMerchant), "_characterContainer")
                .GetValue(__instance) as Control;
        
            if (container != null)
            {
                container.ZIndex = -1; 
            
                var inventory = AccessTools.Field(typeof(NFakeMerchant), "_inventory")
                    .GetValue(__instance) as Control;
                if (inventory != null)
                {
                    inventory.ZIndex = 10;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class CardPlayCastPatch
    {
        [HarmonyPostfix]
        public static void Postfix(CardModel __instance)
        {
            if (__instance.Type == CardType.Skill && __instance.Owner?.Character is RienSang character)
            {
                character.PlayAnimation(__instance.Owner.Creature, "cast");
            }
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
    public static class DamageAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Creature target, DamageResult result, ValueProp props, Creature? dealer)
        {
            if (target.Player?.Character is RienSang character)
            {
                if (result.WasFullyBlocked && result.BlockedDamage > 0)
                {
                    if (dealer != null && dealer.Side == CombatSide.Enemy && !props.HasFlag(ValueProp.SkipHurtAnim) && !props.HasFlag(ValueProp.Unpowered))
                    {
                        character.PlayAnimation(target, "block");
                    }
                }
                
                else if (result.UnblockedDamage > 0 && !target.IsDead)
                {
                    if (dealer != null && dealer.Side == CombatSide.Enemy && !props.HasFlag(ValueProp.SkipHurtAnim) && !props.HasFlag(ValueProp.Unpowered))
                    {
                        character.PlayAnimation(target, "hit");
                    }
                }

                if (LCEvadePower.HasEvadedThisTurn[target] && !props.HasFlag(ValueProp.Unpowered))
                {
                    if (dealer != null && dealer.Side == CombatSide.Enemy && !props.HasFlag(ValueProp.SkipHurtAnim) && !props.HasFlag(ValueProp.Unpowered))
                    {
                        character.PlayAnimation(target, "evade");
                    }
                }
            }
        }
    }
}
