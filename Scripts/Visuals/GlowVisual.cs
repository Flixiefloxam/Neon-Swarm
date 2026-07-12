using Godot;

namespace NeonSwarm.Visuals;

public partial class GlowVisual : Node2D
{
    [Export] public Color BodyColor { get; set; } = Colors.White;
    [Export] public float GlowIntensity { get; set; } = 1.6f;

    private Sprite2D _body;
    private Tween _flashTween; // Tracks the active hit-flash tween so repeated hits can restart the effect cleanly.

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

    // Makes _body flash the provided colour, for the provided duration.
    public void PlayFlash(Color flashColor, float duration = 0.08f)
    {
        if (_body == null)
            return;
        
        _flashTween?.Kill(); // Resets the tween if it was already resetting a previous flash.

        Color normalColor = GetDisplayColor();

        _body.SelfModulate = flashColor;

        _flashTween = CreateTween();
        _flashTween.TweenProperty(
            _body,
            "self_modulate",
            normalColor,
            Mathf.Max(duration, 0.01f)
        );
    }

    private Color GetDisplayColor()
    {
        return new Color(
            BodyColor.R * GlowIntensity,
            BodyColor.G * GlowIntensity,
            BodyColor.B * GlowIntensity,
            BodyColor.A
        );
    }

    // Updates the visuals based on the current BodyColor and GlowIntensity properties.
    private void UpdateVisuals()
    {
        if (_body == null)
        {
            GD.PushWarning($"{Name} has no Body Sprite2D.");
            return;
        }

        _body.SelfModulate = GetDisplayColor();
    }
}