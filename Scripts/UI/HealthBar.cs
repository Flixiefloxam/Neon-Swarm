using Godot;
using NeonSwarm.Components;

namespace NeonSwarm.UI;

public partial class HealthBar : Node2D
{
    [Export] public NodePath HealthComponentPath { get; set; } = "../HealthComponent";

    [Export] public Vector2 BarSize { get; set; } = new(36f, 5f);
    [Export] public float VerticalOffset { get; set; } = 18f;

    [Export] public bool HideWhenFullHealth { get; set; } = false;

    [Export] public Color BackgroundColor { get; set; } = new(0.04f, 0.04f, 0.06f, 0.85f); // The background color of the healthbar.
    [Export] public Color FillColor { get; set; } = Colors.Green; // The color of the filling bar indicating the current health level.
    [Export] public Color BorderColor { get; set; } = new(1f, 1f, 1f, 0.75f); // The color of the border of the healthbar

    private HealthComponent _health;

    private float _currentHealth = 1f;
    private float _maxHealth = 1f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ZIndex = 10;

        _health = GetNodeOrNull<HealthComponent>(HealthComponentPath);

        if (_health == null)
        {
            GD.PushWarning($"{Name} could not find HealthComponent");
            return;
        }

        _health.HealthChanged += OnHealthChanged;

        OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
    }

    // Draws the healthbar
    public override void _Draw()
    {
        if (_maxHealth <= 0f)
            return;
        
        float healthPercent = Mathf.Clamp(_currentHealth / _maxHealth, 0f, 1f);

        Vector2 topLeft = new(
            -BarSize.X / 2f,
            VerticalOffset
        );

        Rect2 backgroundRect = new(topLeft, BarSize);

        DrawRect(backgroundRect, BackgroundColor, true);

        if (healthPercent > 0f)
        {
            Vector2 fillSize = new(
                BarSize.X * healthPercent,
                BarSize.Y
            );

            Rect2 fillRect = new(topLeft, fillSize);
            DrawRect(fillRect, FillColor, true);
        }

        DrawRect(backgroundRect, BorderColor, false, 1f);
    }

    // Updates and redraws the health bar when current or max health changes.
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        _currentHealth = currentHealth;
        _maxHealth = Mathf.Max(maxHealth, 1f);

        Visible = !HideWhenFullHealth || currentHealth < _maxHealth;

        QueueRedraw();
    }

    public override void _ExitTree()
    {
        if (_health != null)
            _health.HealthChanged -= OnHealthChanged;
    }

}
