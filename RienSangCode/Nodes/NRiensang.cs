using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using LimbusCore.LimbusCoreCode;
using System.Collections.Generic;
using LimbusCore.LimbusCoreCode.Overlays;
using RienSang.RienSangCode.Nodes; 
using RienSang.RienSangCode.Powers; 
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Runs;

public partial class NRiensang : NCreatureVisuals
{
    private AnimationPlayer? _anim;
    private Node2D? _visuals;
    private PrescriptCombatOverlay? _combatOverlay; 
    private NShinEffect? _shinAura;
    private string? _currentAttack = null;
    
    private float _debounceTimer = 0f;
    private const float DebounceTimeLimit = 1.0f;
    
    private bool _isRoomMirrored = false;
    private ulong _playerNetId = 0;
    
    public override void _Ready()
    {
        base._Ready();
        _anim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _visuals = GetNodeOrNull<Node2D>("Visuals");
        
        _combatOverlay = new PrescriptCombatOverlay();
        AddChild(_combatOverlay);

        if (_visuals != null)
        {
            
            var shinScene = GD.Load<PackedScene>("res://LimbusCore/scenes/ShinEffect.tscn");
            if (shinScene != null)
            {
                _shinAura = shinScene.Instantiate<NShinEffect>();
                _shinAura.Name = "ShinAura";
                _visuals.AddChild(_shinAura);
            }
        }

        var parentNCreature = GetParentOrNull<NCreature>();
        if (parentNCreature != null && parentNCreature.Entity != null && parentNCreature.Entity.Player != null)
        {
            _playerNetId = parentNCreature.Entity.Player.NetId;
        } else {

            if (RunManager.Instance.NetService.Type == MegaCrit.Sts2.Core.Multiplayer.Game.NetGameType.Singleplayer)
            {
                _playerNetId = RunManager.Instance.NetService.NetId;
            }
        }


        if (_anim != null)
        {
            _anim.AnimationStarted += OnAnimationStartedCinematic;
            _anim.AnimationFinished += OnAnimationFinishedCinematic;
            this.CallDeferred(nameof(DeferredPreload));
            
        }
    }
    
    private void DeferredPreload()
    {
        LimbusCinematicManager.PreloadBackground();
    }

    
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_visuals == null || _anim == null) return;

        if (_anim.CurrentAnimation == "idle")
        {
            if (_visuals.Position != Vector2.Zero)
            {
                _visuals.Position = Vector2.Zero;
            }
        }

        if (_currentAttack != null && _anim.CurrentAnimation != _currentAttack)
        {
            HandleAttackFinished();
        }

        LimbusCinematicManager.UpdateShaderIntensity(_currentAttack != null, delta, _playerNetId);
        LimbusCinematicManager.UpdateCinematic(_visuals.GlobalPosition.X);

        if (LimbusCinematicManager.IsUiPendingShow)
        {
            _debounceTimer -= (float)delta;
            bool isNearNeutral = Mathf.Abs(_visuals.Position.X) < 20.0f;

            LimbusCinematicManager.UpdateBorders(_debounceTimer, DebounceTimeLimit, isNearNeutral, _playerNetId);

            if (!LimbusCinematicManager.IsQueueClear())
            {
                _debounceTimer = DebounceTimeLimit;
            }

            if (_debounceTimer <= 0 && isNearNeutral)
            {
                LimbusCinematicManager.ConfirmUiShow(_playerNetId);
            }
        }

        if (_shinAura != null)
        {
            var parentNCreature = GetParentOrNull<NCreature>();
            if (parentNCreature != null && parentNCreature.Entity != null)
            {
                Creature creature = parentNCreature.Entity;
                bool hasShin = creature.HasPower<ShinFate>();
                _shinAura.UpdateShinVisibility(hasShin);
            }
        }
    }

    private void OnAnimationStartedCinematic(StringName animName)
    {
        string name = animName.ToString();

        if (name == "idle" && !_isRoomMirrored)
        {
            LimbusCinematicManager.StartBackgroundParallax(_visuals?.GlobalPosition.X ?? 0, _playerNetId);
            _isRoomMirrored = true;
        }

        if (name.StartsWith("attack_"))
        {
            _currentAttack = name;
            _debounceTimer = 0f; 
            LimbusCinematicManager.StartUiHide(_playerNetId);
        
            LimbusCinematicManager.StartBackgroundParallax(_visuals?.GlobalPosition.X ?? 0, _playerNetId);
        }
    }

    private void HandleAttackFinished()
    {
        _currentAttack = null;
        _debounceTimer = DebounceTimeLimit; 

        if (_visuals != null) _visuals.Position = Vector2.Zero;

        LimbusCinematicManager.EndUiSequence(_playerNetId);
        LimbusCinematicManager.EndBackgroundParallax();
    }

    private void OnAnimationFinishedCinematic(StringName animName)
    {
        string name = animName.ToString();
        if (name == _currentAttack)
        {
            HandleAttackFinished();
        }
    }
    
    public void DoScreenShake() { }

    public void TriggerPrescriptOverlay(string text, bool isRandom)
    {
        if (_combatOverlay != null)
        {
            var centerPos = GetNodeOrNull<Marker2D>("CenterPos");
            if (centerPos != null)
            {
                _combatOverlay.Position = centerPos.Position - (_combatOverlay.Size / 2f) + _combatOverlay.Offset;
            }
            _combatOverlay.Play(text, isRandom);
        }
    }
}
