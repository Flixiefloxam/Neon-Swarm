using Godot;
using NeonSwarm.Components;
using NeonSwarm.Resources;
using NeonSwarm.Visuals;
using NeonSwarm.Pickups;

namespace NeonSwarm.Enemies;

public partial class BaseEnemy : CharacterBody2D, IKnockbackReceiver
{
	[Export] public EnemyStats Stats { get; set; }

	public float CrowdRadius => Stats?.CrowdRadius ?? 7f;
	public float CrowdMass => Mathf.Max(Stats?.CrowdMass ?? 1f, 0.01f);

	protected HealthComponent Health;
	protected Node2D Target;

	private GlowVisual _glowVisual;
	private HitboxComponent _contactHitbox;

	private Vector2 _movementVelocity = Vector2.Zero;
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

		FindTarget();
		SetupHealth();
		SetupVisuals();
		SetupContactHitbox();
	}

	private void FindTarget()
	{
		Target = GetTree().GetFirstNodeInGroup("Player") as Node2D;

		if (Target == null)
			GD.PushWarning($"{Name} could not find Player target.");
	}

	private void SetupHealth()
	{
		Health = GetNodeOrNull<HealthComponent>("HealthComponent");

		if (Health == null)
		{
			GD.PushWarning($"{Name} has no HealthComponent.");
			return;
		}

		Health.SetMaxHealth(Stats.MaxHealth);
		Health.Died += Die;
	}

	private void SetupVisuals()
	{
		_glowVisual = GetNodeOrNull<GlowVisual>("Visuals");

		if (_glowVisual == null)
		{
			GD.PushWarning($"{Name} has no GlowVisual child.");
			return;
		}

		_glowVisual.ApplyVisuals(Stats.BodyColor, Stats.GlowIntensity);
	}

	private void SetupContactHitbox()
	{
		_contactHitbox = GetNodeOrNull<HitboxComponent>("ContactHitbox");

		if (_contactHitbox == null)
			return;
		
		_contactHitbox.Damage = Stats.ContactDamage;
		_contactHitbox.TargetFactions = DamageFaction.Player;
		_contactHitbox.DamageMode = HitboxDamageMode.OnCooldown;
		_contactHitbox.DamageCooldown = Stats.ContactAttackCooldown;
	}

	// Sets what the enemy wants the current movement velocity to be (_movementVelocity) when factoring in acceleration.
	public void PrepareCrowdMovement(double delta)
	{
		if (Stats == null)
			return;
		
		if (Target == null)
			FindTarget();
		
		Vector2 desiredVelocity = GetDesiredVelocity();

		_movementVelocity = _movementVelocity.MoveToward(
			desiredVelocity,
			Stats.SteeringAcceleration * (float)delta
		);
	}

	// Returns the combined vector of the movement velocity and the knockback velocity.
	public Vector2 GetCrowdVelocity()
	{
		return _movementVelocity + _knockbackVelocity;
	}

	// Returns the vector of the desired direction and speed of the enemy.
	protected virtual Vector2 GetDesiredVelocity()
	{
		if (Target == null || Stats == null)
			return Vector2.Zero;

		Vector2 toTarget = Target.GlobalPosition - GlobalPosition;

		if (toTarget.LengthSquared() <= 0.001f)
			return Vector2.Zero;
		
		return toTarget.Normalized() * Stats.MoveSpeed;
	}

	// Sets the knockback velocity when knockback is applied.
	public virtual void ApplyKnockback(Vector2 direction, float strength)
	{
		if (Stats == null || direction == Vector2.Zero)
			return;
		
		float resistance = Mathf.Max(Stats.KnockbackResistance, 0.1f);
		_knockbackVelocity += direction.Normalized() * (strength / resistance);
	}

	// Used to slowly reduce knockback velocity every frame after it's been applied
	public void FinishCrowdMovement(double delta)
	{
		if (Stats == null)
			return;
		
		_knockbackVelocity = _knockbackVelocity.MoveToward(
			Vector2.Zero,
			Stats.KnockbackDamping * (float)delta
		);
	}

	// Drops an experience gem with the enemies xp value
	private void DropExperience()
	{
		if (Stats  == null)
			return;
		
		if (Stats.ExperienceValue <= 0f)
			return;

		if (Stats.ExperiencePickupScene == null)
			return;
		
		if (Stats.ExperiencePickupScene.Instantiate() is not XpGem xpGem)
		{
			GD.PushWarning($"{Name}'s ExperiencePickupScene is not an XpGem.");
			return;
		}

		Node2D pickupContainer = GetPickupContainer();

		if (pickupContainer == null)
		{
			GD.PushWarning($"{Name} could not find a PickupContainer.");
			xpGem.QueueFree();
			return;
		}

		xpGem.ExperienceAmount = Stats.ExperienceValue;

		// Set local position before AddChild to avoid spawning at the wrong world position for a frame.
		xpGem.Position = pickupContainer.ToLocal(GlobalPosition);

		pickupContainer.AddChild(xpGem);
	}

	private Node2D GetPickupContainer()
	{
		Node currentScene = GetTree().CurrentScene;

		Node2D pickupContainer = currentScene?.GetNodeOrNull<Node2D>("PickupContainer");

		if (pickupContainer != null)
			return pickupContainer;

		return GetParent() as Node2D ?? currentScene as Node2D;
	}

	// Runs when the enemy dies
	protected virtual void Die()
	{
		DropExperience();
		QueueFree();
	}

	// Runs when the enemy is about to be removed from the scene or the scene changes
	public override void _ExitTree()
	{
		if (Health != null)
			Health.Died -= Die;
	}

}
