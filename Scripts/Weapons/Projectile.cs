using Godot;
using NeonSwarm.Components;

namespace NeonSwarm.Weapons;

public partial class Projectile : Area2D
{
	[Export] public float Speed = 400f;
	[Export] public float Damage = 1f;
	[Export] public float Lifetime = 2f;
	[Export] public DamageFaction TargetFaction { get; set; } = DamageFaction.Enemy; // The faction that this projectile will damage.

	private Vector2 _direction = Vector2.Right;
	private float _lifeRemaining = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Projectiles");

		_lifeRemaining = Lifetime;
		AreaEntered += OnAreaEntered;
	}

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _direction * Speed * (float)delta;

		_lifeRemaining -= (float)delta;
		if (_lifeRemaining <= 0f)
			QueueFree();
    }

	// Sets the direction of the projectile. The projectile will move in this direction at the specified speed.
	public void SetDirection(Vector2 direction)
	{
		if (direction == Vector2.Zero)
			return;

		_direction = direction.Normalized();
		Rotation = _direction.Angle();
	}

	// Damages compatible hurtboxes and destroys the projectile on hit.
	private void OnAreaEntered(Area2D area)
	{
		if (area is not HurtboxComponent hurtbox)
			return;

		if (hurtbox.Faction != TargetFaction)
			return;
		
		hurtbox.TakeDamage(Damage);
		QueueFree();
	}
}
