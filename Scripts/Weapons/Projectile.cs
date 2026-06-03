using Godot;
using NeonSwarm.Components;

namespace NeonSwarm.Weapons;

public partial class Projectile : Node2D
{
	private float _speed = 400f;
	private float _lifetime = 2f;
	private Vector2 _direction = Vector2.Right;
	private float _lifeRemaining = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Projectiles");

		_lifeRemaining = _lifetime;
		UpdateHitboxKnockbackDirection();
	}

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _direction * _speed * (float)delta;

		_lifeRemaining -= (float)delta;
		if (_lifeRemaining <= 0f)
			QueueFree();
    }

	// Applies weapon/projectile data when the projectile is spawned.
	public void Initialize(ProjectileSpawnData data)
	{
		_speed = data.Speed;
		_lifetime = data.Lifetime;
		_lifeRemaining = data.Lifetime;

		SetDirection(data.Direction);
		ConfigureHitbox(data);
	}

	// Sets the direction of the projectile. The projectile will move in this direction.
	public void SetDirection(Vector2 direction)
	{
		if (direction == Vector2.Zero)
			return;

		_direction = direction.Normalized();
		Rotation = _direction.Angle();

		UpdateHitboxKnockbackDirection();
	}

	private void ConfigureHitbox(ProjectileSpawnData data)
	{
		HitboxComponent hitbox = GetNodeOrNull<HitboxComponent>("Hitbox");

		if (hitbox == null)
		{
			GD.PushWarning($"{Name} has no Hitbox child.");
			return;
		}

		hitbox.Damage = data.Damage;
		hitbox.DamageMode = data.DamageMode;
		hitbox.DamageCooldown = data.DamageCooldown;
		hitbox.HitsUntilDestroyed = data.HitsUntilDestroyed;
		hitbox.TargetFactions = data.TargetFactions;
		hitbox.KnockbackStrength = data.KnockbackStrength;
		hitbox.KnockbackDirectionOverride = _direction;
	}

	private void UpdateHitboxKnockbackDirection()
	{
		HitboxComponent hitbox = GetNodeOrNull<HitboxComponent>("Hitbox");

		if (hitbox != null)
			hitbox.KnockbackDirectionOverride = _direction;
	}
}
