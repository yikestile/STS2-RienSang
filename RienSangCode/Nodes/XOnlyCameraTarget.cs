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

        Vector2 parentPos = GetParent<Node2D>().GlobalPosition;
        Vector2 rootPos = GetParent<Node2D>().GetParent<Node2D>().GlobalPosition;
    
        float currentDrift = parentPos.DistanceTo(rootPos);

        bool isCinematicActive = currentDrift > 300f; 

        if (!isCinematicActive)
        {
            GlobalPosition = _restingGlobalPos;
        }
        else 
        {
            GlobalPosition = new Vector2(GetParent<Node2D>().GlobalPosition.X, _restingGlobalPos.Y);
        }
    }
}