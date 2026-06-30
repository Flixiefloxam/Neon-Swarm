using Godot;
using NeonSwarm.Components;

namespace NeonSwarm.Pickups;

public partial class XpGem : Node2D
{
	[Export] public float ExperienceAmount { get; set; } = 1f; // How much experience is awarded when the gem is picked up.

	[Export] public float CollectionRadius { get; set; } = 16f; // Distance from the player at which the gem is collected.

	[ExportGroup("Visuals")]
	[Export] public Color BodyColor { get; set; } = new(0.8f, 0f, 1f, 1f);
	[Export] public float GlowIntensity { get; set; } = 1.8f;
	[Export] public float RotationSpeed { get; set; } = 2.5f;
	[Export] public float PulseSpeed { get; set; } = 4f;
	[Export] public float PulseAmount { get; set; } = 0.08f;

	private Node2D _player;
	private ExperienceComponent _experienceComponent;
	private Polygon2D _body;

	private Vector2 _baseScale = Vector2.One;
	private float _time = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Pickups");

		_body = GetNodeOrNull<Polygon2D>("Body");
		_baseScale = Scale;
		
		ApplyVisuals();
		FindPlayer();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float deltaFloat = (float)delta;

		_time += deltaFloat;
		Rotation += RotationSpeed * deltaFloat;

		float pulse = 1f + Mathf.Sin(_time * PulseSpeed) * PulseAmount;
		Scale = _baseScale * pulse;
	}

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null || _experienceComponent == null)
			FindPlayer();

		if (_player == null || _experienceComponent == null)
			return;
		
		float collectionRadiusSquared = CollectionRadius * CollectionRadius;
		float distanceSquared = GlobalPosition.DistanceSquaredTo(_player.GlobalPosition);

		if (distanceSquared <= collectionRadiusSquared)
			Collect();
    }

	private void FindPlayer()
	{
		_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

		if (_player == null)
			return;

		_experienceComponent = _player.GetNodeOrNull<ExperienceComponent>("ExperienceComponent");
	}

	private void Collect()
	{
		_experienceComponent.AddExperience(ExperienceAmount);
		QueueFree();
	}

	private void ApplyVisuals()
	{
		if (_body == null)
		{
			GD.PushWarning($"{Name} has no Body Polygon2D.");
			return;
		}

		float glowIntensity = Mathf.Max(0f, GlowIntensity);

		_body.Color = new Color(
			BodyColor.R * glowIntensity,
			BodyColor.G * glowIntensity,
			BodyColor.B * glowIntensity,
			BodyColor.A
		);
	}
}
