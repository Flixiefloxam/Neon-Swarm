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
        UpdateVisuals();
    }

    // Used to apply both color and glow intensity at the same time.
    public void ApplyVisuals(Color bodyColor, float glowIntensity)
    {
        BodyColor = bodyColor;
        GlowIntensity = Mathf.Max(0f, glowIntensity); // Ensure glow intensity is not negative.
        UpdateVisuals();
    }

    // Used to apply just the color, keeping the existing glow intensity.
    public void ApplyColor(Color bodyColor)
    {
        BodyColor = bodyColor;
        UpdateVisuals();
    }

    // Used to apply just the glow intensity, keeping the existing color.
    public void ApplyGlowIntensity(float intensity)
    {
        GlowIntensity = Mathf.Max(0f, intensity); // Ensure glow intensity is not negative.
        UpdateVisuals();
    }

    // Updates the visuals based on the current BodyColor and GlowIntensity properties.
    private void UpdateVisuals()
    {
        if (_body == null)
        {
            GD.PushWarning($"{Name} has no Body Sprite2D.");
            return;
        }

        _body.SelfModulate = new Color(
            BodyColor.R * GlowIntensity,
            BodyColor.G * GlowIntensity,
            BodyColor.B * GlowIntensity,
            BodyColor.A
        );
    }
}