using Godot;
using NeonSwarm.Resources;
using NeonSwarm.Visuals;
using NeonSwarm.Components;
using NeonSwarm.Player;

namespace NeonSwarm.Enemies;

public partial class BaseEnemy : CharacterBody2D
{
	[Export] public EnemyStats Stats {get; set;}

	public float BodyRadius => Stats?.BodyRadius ?? 12f; // Fallback radius if Stats is not assigned.

	protected HealthComponent Health;
	protected Node2D Target; // The target node that the enemy will move towards.

	private GlowVisual _glowVisual;
	private HitboxComponent _contactHitbox; // The hitbox used for damaging the player on contact. This is set up in the scene and configured based on the enemy's stats.
	private Vector2 _knockbackVelocity = Vector2.Zero;
	private Vector2 _pushVelocityThisFrame = Vector2.Zero;

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

		Vector2 enemySeparationVeclocity = GetEnemySeparationVelocity();
		Vector2 playerSeparationVelocity = GetPlayerSeparationVelocity();

		// Combine all vectors to get the final velocity for this frame.
		Velocity =
			chaseVelocity +
			enemySeparationVeclocity +
			playerSeparationVelocity +
			_knockbackVelocity +
			_pushVelocityThisFrame;

		MoveAndSlide();

		_pushVelocityThisFrame = Vector2.Zero; // Reset push velocity after applying it for this frame to prevent it from compounding over multiple frames.

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

	public virtual void ApplyPush(Vector2 direction, float strength)
	{
		if (Stats == null || direction == Vector2.Zero)
			return;

		float resistance = Mathf.Max(Stats.KnockbackResistance, 0.1f); // Prevent division by zero and ensure some push is applied.
		Vector2 pushVelocity = direction.Normalized() * (strength / resistance);

		if (pushVelocity.LengthSquared() > _pushVelocityThisFrame.LengthSquared())
			_pushVelocityThisFrame = pushVelocity;
	}

	private Vector2 GetEnemySeparationVelocity()
	{
		Vector2 separation = Vector2.Zero;

		foreach (Node node in GetTree().GetNodesInGroup("Enemies"))
		{
			if (node == this)
				continue;
			
			if (node is not BaseEnemy otherEnemy)
				continue;
			
			Vector2 awayFromOther = GlobalPosition - otherEnemy.GlobalPosition;
			float distance = awayFromOther.Length();

			if (distance <=0.001f)
			{
				float angle = (GetInstanceId() % 360) * Mathf.Pi / 180f; // Unique angle based on instance ID to prevent enemies from overlapping in the same position.
				awayFromOther = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				distance = 0.001f; // Prevent division by zero and apply a small separation force.
			}

			float desiredDistance =
				BodyRadius +
				otherEnemy.BodyRadius +
				Stats.SeparationPadding;
			
			if (distance >= desiredDistance)
				continue; // No need to apply separation if already far enough apart.
			
			float closeness = 1f - distance / desiredDistance; // How close the enemies are to each other, from 0 (at or beyond desired distance) to 1 (completely overlapping).
			separation += awayFromOther.Normalized() * closeness; // Stronger separation when closer to the other enemy.
		}

		return separation * Stats.SeparationStrength;
	}

	private Vector2 GetPlayerSeparationVelocity()
	{
		if (Target == null || Stats == null)
			return Vector2.Zero;

		float playerRadius = 12f;

		if (Target is PlayerController player)
			playerRadius = player.BodyRadius;
		
		Vector2 awayFromPlayer = GlobalPosition - Target.GlobalPosition;
		float distance = awayFromPlayer.Length();

		if (distance <= 0.001f)
		{
			awayFromPlayer = Vector2.Right; // Arbitrary direction to push if exactly on top of the player.
			distance = 0.001f; // Prevent division by zero and apply a small separation force.
		}

		float desiredDistance = 
			BodyRadius +
			playerRadius +
			Stats.SeparationPadding;

		if (distance >= desiredDistance)
			return Vector2.Zero; // No need to apply separation if already far enough apart.
		
		float closeness = 1f - distance / desiredDistance; // How close the enemy is to the player, from 0 (at or beyond desired distance) to 1 (completely overlapping).

		return awayFromPlayer.Normalized() * closeness * Stats.SeparationStrength;
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
