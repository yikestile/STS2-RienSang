using Godot;
using System;

public partial class SmoothFollowCamera : Camera2D
{
    private Node2D _targetNode;
    private bool _calibrated = false;
    private Vector2 _restingPos = new Vector2(960, 540);
    
    [Export] private float _followWeight = 0.1f; 
    [Export] private float _returnThreshold = 1.0f;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        
        _targetNode = GetParent()?.GetNodeOrNull<Node2D>("Visuals/CameraTarget");   
        _calibrated = (_targetNode != null);

        if (_calibrated)
        {
            GlobalPosition = _targetNode.GlobalPosition;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint() || !_calibrated || _targetNode == null) return;

        bool animationWantsCamera = IsAnimationActive();

        Vector2 destination;

        if (animationWantsCamera)
        {
            Enabled = true;
            destination = _targetNode.GlobalPosition;
        }
        else
        {
            destination = _restingPos;

            if (GlobalPosition.DistanceTo(_restingPos) < _returnThreshold)
            {
                Enabled = false;
                return; 
            }
        }

        float lerpFactor = 1.0f - Mathf.Pow(_followWeight, (float)delta);
        GlobalPosition = GlobalPosition.Lerp(destination, lerpFactor);
    }

    private bool IsAnimationActive()
    {
        var anim = GetParent()?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (anim != null && anim.IsPlaying())
        {
            string current = anim.CurrentAnimation;
            return current == "attack_lance_3";
        }
        return false;
    }
}