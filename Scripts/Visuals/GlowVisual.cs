using Godot;

namespace NeonSwarm.Visuals;

public partial class GlowVisual : Node2D
{
    [Export] public Color BodyColor { get; set; } = Colors.White;
    [Export] public float GlowIntensity { get; set; } = 1.6f;

    private Sprite2D _body;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _body = GetNodeOrNull<Sprite2D>("Body");
        ApplyColor(BodyColor);
    }

    public void ApplyColor(Color bodyColor)
    {
        BodyColor = bodyColor;

        if (_body == null)
        {
            GD.PushWarning($"{Name} has no Body Sprite2D.");
            return;
        }

        _body.SelfModulate = new Color(
            bodyColor.R * GlowIntensity,
            bodyColor.G * GlowIntensity,
            bodyColor.B * GlowIntensity,
            bodyColor.A
        );
    }
}