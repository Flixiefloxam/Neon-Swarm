using Godot;
using NeonSwarm.Components;

namespace NeonSwarm.Weapons;

public partial class Projectile : Node2D
{
	[Export] public float Speed = 400f;
	[Export] public float Lifetime = 2f;

	private Vector2 _direction = Vector2.Right;
	private float _lifeRemaining = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Projectiles");

		_lifeRemaining = Lifetime;
		UpdateHitboxKnockbackDirection();
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

		UpdateHitboxKnockbackDirection();
	}

	private void UpdateHitboxKnockbackDirection()
	{
		HitboxComponent hitbox = GetNodeOrNull<HitboxComponent>("Hitbox");

		if (hitbox != null)
			hitbox.KnockbackDirectionOverride = _direction;
	}
}
