using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using LimbusCore.LimbusCoreCode;
using System.Collections.Generic;

public partial class NRiensang : NCreatureVisuals
{
    private AnimationPlayer? _anim;
    private Node2D? _visuals;
    private string? _currentAttack = null;
    
    private float _debounceTimer = 0f;
    private const float DebounceTimeLimit = 1.0f;

    private static readonly HashSet<string> DashingMoves = new() 
    { 
        "attack_lance_3", 
        "attack_hammer_3",
        "attack_furioso_finisher"
    };
    
    public override void _Ready()
    {
        base._Ready();
        _anim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        _visuals = GetNodeOrNull<Node2D>("Visuals");

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
    
        if (_anim.CurrentAnimation == "idle_loop")
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

        LimbusCinematicManager.UpdateShaderIntensity(_currentAttack != null, delta);
        LimbusCinematicManager.UpdateCinematic(_visuals.GlobalPosition.X);

        if (LimbusCinematicManager.IsUiPendingShow)
        {
            _debounceTimer -= (float)delta;
            bool isNearNeutral = Mathf.Abs(_visuals.Position.X) < 20.0f;

            LimbusCinematicManager.UpdateBorders(_debounceTimer, DebounceTimeLimit, isNearNeutral);

            if (!LimbusCinematicManager.IsQueueClear())
            {
                _debounceTimer = DebounceTimeLimit;
            }

            if (_debounceTimer <= 0 && isNearNeutral)
            {
                LimbusCinematicManager.ConfirmUiShow();
            }
        }
    }

    private void OnAnimationStartedCinematic(StringName animName)
    {
        string name = animName.ToString();
        if (name.StartsWith("attack_"))
        {
            _currentAttack = name;
            _debounceTimer = 0f; 
            LimbusCinematicManager.StartUiHide();
            
            if (DashingMoves.Contains(name))
            {
                LimbusCinematicManager.StartBackgroundParallax(_visuals?.GlobalPosition.X ?? 0);
            }
        }
    }

    private void HandleAttackFinished()
    {
        _currentAttack = null;
        _debounceTimer = DebounceTimeLimit; 

        if (_visuals != null)
        {
            _visuals.Position = Vector2.Zero;
        }

        LimbusCinematicManager.EndUiSequence();
        LimbusCinematicManager.EndBackgroundParallax();
    }

    private void OnAnimationFinishedCinematic(StringName animName)
    {
        if (animName.ToString() == _currentAttack)
        {
            HandleAttackFinished();
        }
    }
    
    public void DoScreenShake() { }
}