using Godot;
using NeonSwarm.Resources;
using NeonSwarm.Visuals;
using NeonSwarm.Components;

namespace NeonSwarm.Enemies;

public partial class BaseEnemy : CharacterBody2D
{
	[Export] public EnemyStats Stats {get; set;}

	protected HealthComponent Health;
	protected Node2D Target; // The target node that the enemy will move towards.

	private GlowVisual _glowVisual;
	private HitboxComponent _contactHitbox; // The hitbox used for damaging the player on contact. This is set up in the scene and configured based on the enemy's stats.
	private Vector2 _knockbackVelocity = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Enemies");
		MotionMode = MotionModeEnum.Floating;

		if (Stats == null)
		{
			GD.PushWarning($"{Name} has no EnemyStats assigned. Using default values.");
			Stats = new EnemyStats();
		}
		Target = GetTree().GetFirstNodeInGroup("Player") as Node2D;

		Health = GetNodeOrNull<HealthComponent>("HealthComponent");
		if (Health == null)
		{
			GD.PushWarning($"{Name} has no HealthComponent.");
		}
		else
		{
			Health.SetMaxHealth(Stats.MaxHealth);
			Health.Died += Die;
		}

		_glowVisual = GetNodeOrNull<GlowVisual>("Visuals");
		if (_glowVisual != null)
		{
			_glowVisual.ApplyColor(Stats.BodyColor);
		}
		else
		{
			GD.PushWarning($"{Name} has no GlowVisual child.");
		}

		_contactHitbox = GetNodeOrNull<HitboxComponent>("ContactHitbox");
		if (_contactHitbox != null)
		{
			_contactHitbox.Damage = Stats.ContactDamage;
			_contactHitbox.TargetFactions = DamageFaction.Player;
			_contactHitbox.DamageMode = HitboxDamageMode.OnCooldown;
			_contactHitbox.AttackCooldown = Stats.ContactAttackCooldown;
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
		if (Target == null || Stats == null)
			return;
		
		MoveTowardsTarget(delta);
	}

	protected virtual void MoveTowardsTarget(double delta)
	{
		Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
		Vector2 chaseVelocity = direction * Stats.MoveSpeed;

		Velocity = chaseVelocity + _knockbackVelocity; // Apply knockback velocity on top of regular movement velocity.
		MoveAndSlide();

		// Gradually reduce knockback velocity over time using damping.
		_knockbackVelocity = _knockbackVelocity.MoveToward(
			Vector2.Zero,
			Stats.KnockbackDamping * (float)delta
		);
	}

	public virtual void ApplyKnockback(Vector2 direction, float strength)
	{
		if (Stats == null || direction == Vector2.Zero)
			return;
		
		float resistance = Mathf.Max(Stats.KnockbackResistance, 0.1f); // Prevent division by zero and ensure some knockback is applied.
		_knockbackVelocity += direction.Normalized() * (strength / resistance);
	}

	protected virtual void Die()
	{
		QueueFree();
	}

	public override void _ExitTree()
	{
		if (Health != null)
			Health.Died -= Die;
	}

}
