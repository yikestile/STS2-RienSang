using Godot;
using System;

public partial class SmoothFollowCamera : Camera2D
{
    private Node2D _targetNode;
    private Vector2 _startOffset;
    private bool _calibrated = false;
    private bool _isInitialSnapDone = false;
    
    [Export] private float _followWeight = 1.0f; 

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        
        PositionSmoothingEnabled = false;
        
        _targetNode = GetParent()?.GetNodeOrNull<Node2D>("CameraTarget");   
        
        if (_targetNode != null)
        {
            _startOffset = GlobalPosition - _targetNode.GlobalPosition;
            _calibrated = true;
            
            GlobalPosition = _targetNode.GlobalPosition + _startOffset;
        }

        GetTree().CreateTimer(0.5f).Timeout += () => {
            _isInitialSnapDone = true;
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint() || !_calibrated || _targetNode == null || !_isInitialSnapDone) return;

        Vector2 desiredPos = _targetNode.GlobalPosition + _startOffset;
    
        float lerpFactor = 1.0f - Mathf.Pow(_followWeight, (float)delta);
    
        float newX = Mathf.Lerp(GlobalPosition.X, desiredPos.X, lerpFactor);
        GlobalPosition = new Vector2(newX, desiredPos.Y);
    }
}
