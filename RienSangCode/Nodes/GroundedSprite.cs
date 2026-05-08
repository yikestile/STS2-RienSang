using Godot;
using System.Collections.Generic;

[Tool]
public partial class GroundedSprite : Sprite2D
{
    private Dictionary<ulong, Vector2> _initialLocalPositions = new Dictionary<ulong, Vector2>();

    public override void _Ready()
    {
        Centered = false; 
        _initialLocalPositions.Clear();
        CacheChildren();
    }

    private void CacheChildren()
    {
        foreach (Node child in GetChildren())
        {
            if (child is Node2D child2D && !_initialLocalPositions.ContainsKey(child.GetInstanceId()))
            {
                _initialLocalPositions[child.GetInstanceId()] = child2D.Position;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Texture != null) UpdateGrounding();
    }

    private void UpdateGrounding()
    {
        Vector2 groundingOffset = new Vector2(-Texture.GetWidth() / 2f, -Texture.GetHeight());
        Offset = groundingOffset;

        foreach (Node child in GetChildren())
        {
            if (child.Name == "ShinAura") continue;

            if (child is Node2D child2D)
            {
                ulong id = child.GetInstanceId();
                if (!_initialLocalPositions.ContainsKey(id))
                    _initialLocalPositions[id] = child2D.Position;

                child2D.Position = groundingOffset + _initialLocalPositions[id];
            }
        }
    }
}