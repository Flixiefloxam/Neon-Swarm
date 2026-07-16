using Godot;
using NeonSwarm.Components;
using NeonSwarm.Resources;
using NeonSwarm.Visuals;
using NeonSwarm.Pickups;
using NeonSwarm.Vfx;

namespace NeonSwarm.Enemies;

public partial class BaseEnemy : CharacterBody2D, IKnockbackReceiver
{
	[Export] public EnemyStats Stats { get; set; }

	public float CrowdRadius => Stats?.CrowdRadius ?? 7f;
	public float CrowdMass => Mathf.Max(Stats?.CrowdMass ?? 1f, 0.01f);

	protected HealthComponent Health;
	protected Node2D Target;

	private GlowVisual _visuals;
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
		SetupSize();
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
		_visuals = GetNodeOrNull<GlowVisual>("Visuals");

		if (_visuals == null)
		{
			GD.PushWarning($"{Name} has no GlowVisual child.");
			return;
		}

		_visuals.ApplyVisuals(Stats.BodyColor, Stats.GlowIntensity);
	}

	private void SetupSize()
	{
		ApplyVisualScale();
		ApplySquareShapeSize("Hurtbox/CollisionShape2D", Stats.HurtboxSize);
		ApplyCircleShapeRadius("ContactHitbox/CollisionShape2D", Stats.ContactHitboxRadius);
	}

	private void ApplyVisualScale()
	{
		if (_visuals == null)
			return;
		
		float visualScale = Mathf.Max(Stats.VisualScale, 0.01f);
		_visuals.Scale = Vector2.One * visualScale;
	}

	private void ApplySquareShapeSize(string collisionShapePath, float size)
	{
		CollisionShape2D collisionShape = GetNodeOrNull<CollisionShape2D>(collisionShapePath);

		if (collisionShape == null)
		{
			GD.PushWarning($"{Name} could not find CollisionShape2D at '{collisionShapePath}'.");
			return;
		}

		if (collisionShape.Shape is not RectangleShape2D rectangleShape)
		{
			GD.PushWarning($"{Name}'s CollisionShape2D at '{collisionShapePath}' is not a RectangleShape2D.");
			return;
		}

		float safeSize = Mathf.Max(size, 1f);

		// Duplicate before editing so this enemy instance does not mutate a shared scene resource.
		RectangleShape2D uniqueShape = rectangleShape.Duplicate() as RectangleShape2D;

		if (uniqueShape == null)
			return;
		
		uniqueShape.Size = Vector2.One * safeSize;
		collisionShape.Shape = uniqueShape;
	}

	private void ApplyCircleShapeRadius(string collisionShapePath, float radius)
	{
		CollisionShape2D collisionShape = GetNodeOrNull<CollisionShape2D>(collisionShapePath);

		if (collisionShape == null)
		{
			GD.PushWarning($"{Name} could not find CollisionShape2D at '{collisionShapePath}'.");
			return;
		}

		if (collisionShape.Shape is not CircleShape2D circleShape)
		{
			GD.PushWarning($"{Name}'s CollisionShape2D at '{collisionShapePath}' is not a CircleShape2D.");
			return;
		}

		float safeRadius = Mathf.Max(radius, 1f);

		// Duplicate before editing so this enemy instance does not mutate a shared scene resource.
		CircleShape2D uniqueShape = circleShape.Duplicate() as CircleShape2D;

		if (uniqueShape == null)
			return;
		
		uniqueShape.Radius = safeRadius;
		collisionShape.Shape = uniqueShape;
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

	// Gradually reduces knockback velocity after it has been applied.
	public void FinishCrowdMovement(double delta)
	{
		if (Stats == null)
			return;
		
		_knockbackVelocity = _knockbackVelocity.MoveToward(
			Vector2.Zero,
			Stats.KnockbackDamping * (float)delta
		);
	}

	private void SpawnDeathVfx()
	{
		if (Stats?.DeathVfxScene == null)
			return;
		
		if (Stats.DeathVfxScene.Instantiate() is not OneShotParticlesVfx deathVfx)
		{
			GD.PushWarning(
				$"{Name}'s DeathVfxScene is not a OneShotParticlesVfx."
			);
			return;
		}

		Node2D vfxContainer = GetVfxContainer();

		if (vfxContainer == null)
		{
			GD.PushWarning($"{Name} could not find a VfxContainer.");
			deathVfx.QueueFree();
			return;
		}

		deathVfx.Initialize(Stats.BodyColor); // run before addchild because addchild triggers _Ready.
		deathVfx.Scale = Vector2.One * Mathf.Max(Stats.VisualScale, 0.01f); // Scales death vfx to enemy size.

		deathVfx.Position = vfxContainer.ToLocal(GlobalPosition); // Set pos before spawning so it's not in the wrong place for a frame.
		vfxContainer.AddChild(deathVfx);
	}

	private Node2D GetVfxContainer()
	{
		Node currentScene = GetTree().CurrentScene;

		return currentScene?.GetNodeOrNull<Node2D>("VfxContainer");
	}

	// Drops an experience gem using this enemy's XP value.
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
		SpawnDeathVfx();
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
