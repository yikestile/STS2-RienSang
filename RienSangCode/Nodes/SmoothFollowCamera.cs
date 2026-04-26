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

        var visualsNode = _targetNode.GetParent<Node2D>();
        float animationPushDistance = visualsNode.Position.Length();
        bool animationWantsCamera = animationPushDistance > 500f;

        Vector2 destination;
        if (animationWantsCamera)
        {
            this.Enabled = true;
            destination = _targetNode.GlobalPosition;
        }
        else
        {
            destination = _restingPos;
            if (GlobalPosition.DistanceTo(_restingPos) < _returnThreshold)
            {
                GlobalPosition = _restingPos;
                this.Enabled = false;
                return; 
            }
        }

        float distance = GlobalPosition.DistanceTo(destination);

        float distanceBoost = Mathf.Remap(distance, 0, 1200, 1.0f, 3.5f);
        distanceBoost = Mathf.Clamp(distanceBoost, 1.0f, 5.0f);

        float activeWeight = Mathf.Max(0.0001f, _followWeight / distanceBoost);

        float lerpFactor = 1.0f - Mathf.Pow(activeWeight, (float)delta);
        GlobalPosition = GlobalPosition.Lerp(destination, lerpFactor);
    }
}