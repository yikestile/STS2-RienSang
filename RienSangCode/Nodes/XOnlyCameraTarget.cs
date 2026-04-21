using Godot;
using System;

public partial class XOnlyCameraTarget : Marker2D
{
    private Node2D _visuals;
    private AnimationPlayer? _anim;
    private Vector2 _restingGlobalPos;
    private bool _isInitialized = false;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        
        _visuals = GetParent<Node2D>();
        _anim = _visuals?.GetParent()?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer") ?? 
                _visuals?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        
        _restingGlobalPos = GlobalPosition;
        _isInitialized = true;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || !_isInitialized) return;

        bool isCinematicActive = false;
        if (_anim != null && _anim.IsPlaying())
        {
            string current = _anim.CurrentAnimation;
            if (current == "attack_lance_3")
                isCinematicActive = true;
        }

        if (!isCinematicActive)
        {
            GlobalPosition = _restingGlobalPos;
        }

    }
}