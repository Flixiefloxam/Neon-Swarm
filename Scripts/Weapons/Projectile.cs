using Godot;
using NeonSwarm.Components;
using NeonSwarm.Vfx;

namespace NeonSwarm.Weapons;

public partial class Projectile : Node2D
{
	private float _speed = 400f;
	private float _lifetime = 2f;
	private Vector2 _direction = Vector2.Right;
	private float _lifeRemaining = 0f;
	private PackedScene _impactVfxScene;
	private HitboxComponent _hitbox;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Projectiles");

		_lifeRemaining = _lifetime;

		_hitbox = GetNodeOrNull<HitboxComponent>("Hitbox");

		if (_hitbox == null)
		{
			GD.PushWarning($"{Name} has no Hitbox child.");
		}
		else
		{
			_hitbox.HitLanded += OnHitLanded;
		}

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
		_impactVfxScene = data.ImpactVfxScene;

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
		_hitbox = GetNodeOrNull<HitboxComponent>("Hitbox");

		if (_hitbox == null)
		{
			GD.PushWarning($"{Name} has no Hitbox child.");
			return;
		}

		_hitbox.Damage = data.Damage;
		_hitbox.DamageMode = data.DamageMode;
		_hitbox.DamageCooldown = data.DamageCooldown;
		_hitbox.HitsUntilDestroyed = data.HitsUntilDestroyed;
		_hitbox.TargetFactions = data.TargetFactions;
		_hitbox.KnockbackStrength = data.KnockbackStrength;
		_hitbox.KnockbackDirectionOverride = _direction;
	}

	private void OnHitLanded(HurtboxComponent hurtbox)
	{
		SpawnImpactVfx();
	}

	private void SpawnImpactVfx()
	{
		if (_impactVfxScene == null)
			return;
		
		if (_impactVfxScene.Instantiate() is not OneShotParticlesVfx impactVfx)
		{
			GD.PushWarning(
				$"{Name}'s ImpactVfxScene is not a OneShotParticlesVfx."
			);
			return;
		}

		Node currentScene = GetTree().CurrentScene;
		Node2D vfxContainer =
			currentScene?.GetNodeOrNull<Node2D>("VfxContainer");

		if (vfxContainer == null)
		{
			GD.PushWarning($"{Name} could not find a VfxContainer.");
			impactVfx.QueueFree();
			return;
		}

		CanvasItem body =
			GetNodeOrNull<CanvasItem>("Visuals/Body");

		Color impactColor =
			body?.SelfModulate ?? Colors.White;

		impactVfx.Initialize(impactColor);

		impactVfx.Position = vfxContainer.ToLocal(GlobalPosition);

		impactVfx.Rotation = GlobalRotation - vfxContainer.GlobalRotation; // Rotating particle effect so it sprays back form projectile impact.

		vfxContainer.AddChild(impactVfx);
	}

	private void UpdateHitboxKnockbackDirection()
	{
		HitboxComponent hitbox = GetNodeOrNull<HitboxComponent>("Hitbox");

		if (hitbox != null)
			hitbox.KnockbackDirectionOverride = _direction;
	}

	public override void _ExitTree()
	{
		if (_hitbox != null)
			_hitbox.HitLanded -= OnHitLanded;
	}
}
