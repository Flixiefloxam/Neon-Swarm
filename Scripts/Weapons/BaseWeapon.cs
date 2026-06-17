using Godot;
using System;
using NeonSwarm.Components;
using NeonSwarm.Resources;

namespace NeonSwarm.Weapons;

public abstract partial class BaseWeapon : Node2D
{
	[Export] public WeaponStats BaseStats { get; set; }

	[ExportGroup("Runtime")]
	[Export] public int MaxShotsPerFrame { get; set; } = 5;

	public WeaponStats RuntimeStats { get; protected set; }
	public string WeaponId => RuntimeStats?.WeaponId ?? BaseStats?.WeaponId ?? "";
	public string WeaponName => RuntimeStats?.WeaponName ?? BaseStats?.WeaponName ?? Name;

	protected Node2D ProjectileContainer { get; private set; }

	private float _timeSinceLastShot = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Weapons");
		
		LoadStats();
		ProjectileContainer = GetOrCreateProjectileContainer();

		if (ProjectileContainer == null)
			GD.PushError($"{Name} could not create or find a projectile container.");
		if (string.IsNullOrWhiteSpace(WeaponId))
			GD.PushWarning($"{Name} has no WeaponId.");
	}

    public override void _PhysicsProcess(double delta)
	{
		if (!CanFire())
			return;
		
		float secondsPerShot = 1f / RuntimeStats.FireRate;

		// Charge up to roughly one ready shot without building a large backlog.
		if (_timeSinceLastShot < secondsPerShot)
			_timeSinceLastShot += (float)delta;
		
		int shotsThisFrame = 0;

		while (_timeSinceLastShot >= secondsPerShot)
		{
			if (shotsThisFrame >= MaxShotsPerFrame)
			{
				_timeSinceLastShot = 0f;
				break;
			}

			if (!TryFire())
				break;
			
			_timeSinceLastShot -= secondsPerShot;
			shotsThisFrame++;
		}
	}

	public bool HasWeaponId(string weaponId)
	{
		return !string.IsNullOrWhiteSpace(weaponId) &&
			string.Equals(WeaponId, weaponId, StringComparison.OrdinalIgnoreCase);
	}

	protected virtual void LoadStats()
	{
		if (BaseStats == null)
		{
			GD.PushWarning($"{Name} has no WeaponStats assigned. Using default runtime stats.");
			RuntimeStats = new WeaponStats();
			return;
		}

		RuntimeStats = BaseStats.Duplicate() as WeaponStats;

		if (RuntimeStats == null)
		{
			GD.PushError($"{Name} failed to duplicate WeaponStats. Using default runtime stats.");
			RuntimeStats = new WeaponStats();
		}
	}

	protected virtual bool CanFire()
	{
		return RuntimeStats != null &&
			RuntimeStats.FireRate > 0f &&
			RuntimeStats.ProjectileScene != null &&
			ProjectileContainer != null;
	}

	protected abstract bool TryFire();

	protected ProjectileSpawnData CreateProjectileSpawnData(Vector2 direction)
	{
		return new ProjectileSpawnData
		{
			Direction = direction,

			Speed = RuntimeStats.ProjectileSpeed,
			Lifetime = RuntimeStats.ProjectileLifetime,

			Damage = RuntimeStats.Damage,
			DamageMode = RuntimeStats.DamageMode,
			DamageCooldown = RuntimeStats.DamageCooldown,
			HitsUntilDestroyed = RuntimeStats.HitsUntilDestroyed,
			TargetFactions = RuntimeStats.TargetFactions,

			KnockbackStrength = RuntimeStats.KnockbackStrength
		};
	}

	protected Projectile SpawnProjectile(Vector2 globalSpawnPosition, ProjectileSpawnData data)
	{
		if (RuntimeStats.ProjectileScene == null)
		{
			GD.PushWarning($"{Name} has no ProjectileScene assigned.");
			return null;
		}

		if (ProjectileContainer == null)
		{
			GD.PushWarning($"{Name} has no ProjectileContainer.");
			return null;
		}

		Projectile projectile = RuntimeStats.ProjectileScene.Instantiate<Projectile>();

		projectile.Position = ProjectileContainer.ToLocal(globalSpawnPosition);
		projectile.Initialize(data);

		ProjectileContainer.AddChild(projectile);

		return projectile;
	}

	private Node2D GetOrCreateProjectileContainer()
	{
		Node parent = GetTree().CurrentScene ?? GetParent();
		if (parent == null)
		{
			GD.PushWarning($"{Name} could not find a valid parent for ProjectileContainer.");
			return null;
		}

		Node2D container = parent.GetNodeOrNull<Node2D>("ProjectileContainer");
		if (container != null)
			return container;

		container = new Node2D
		{
			Name = "ProjectileContainer"
		};

		parent.AddChild(container);
		GD.PushWarning($"{Name} couldn't find a ProjectileContainer. Created a temporary ProjectileContainer node.");

		return container;
	}
}
