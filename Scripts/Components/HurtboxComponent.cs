using Godot;

namespace NeonSwarm.Components;

public partial class HurtboxComponent : Area2D
{
	[Export] public NodePath HealthComponentPath { get; set; } = "../HealthComponent"; // The path to the HealthComponent node that this Hurtbox will interact with.
	[Export] public DamageFaction Faction { get; set; } = DamageFaction.Neutral; // The faction of this Hurtbox. This can be used to determine what types of projectiles or attacks should affect this Hurtbox.

	private HealthComponent _health; // Reference to the HealthComponent that will be affected by this Hurtbox.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_health = GetNodeOrNull<HealthComponent>(HealthComponentPath);

		if (_health == null)
		{
			GD.PushWarning($"{Name} could not find HealthComponent.");
		}
	}

	public void TakeDamage(float damage)
	{
		_health?.TakeDamage(damage);
	}
}
