using Godot;

namespace NeonSwarm.Weapons;

public partial class BasicGun : Node2D
{
	[Export] public PackedScene ProjectileScene { get; set; }
	[Export] public NodePath ProjectileContainerPath { get; set; }
	[Export] public float FireRate { get; set; } = 1f; // Number of shots per second. Higher values mean faster firing.

	private const int MaxShotsPerFrame = 5; // Maximum number of shots that can be fired in a single frame to prevent performance issues during frame rate drops.

	private float _timeSinceLastShot = 0f;
	private Node2D _projectileContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_projectileContainer = GetOrCreateProjectileContainer();

		if (_projectileContainer == null)
			GD.PushError($"{Name} could not create or find a projectile container.");
	}

    public override void _PhysicsProcess(double delta)
    {
		if (FireRate <= 0f || ProjectileScene == null || _projectileContainer == null)
			return;
		
		int shotsThisFrame = 0;
		float secondsPerShot = 1f / FireRate;

		if (_timeSinceLastShot < secondsPerShot) // This prevents a massive backlog of shots if there are no enemies to shoot at.
        	_timeSinceLastShot += (float)delta;
		
		while (_timeSinceLastShot >= secondsPerShot)
		{
			if (shotsThisFrame >= MaxShotsPerFrame)
			{
				_timeSinceLastShot = 0f; // Reset the timer if too many shots happen in one frame, preventing the backlog of shots spilling over to the next frame.
				break;
			}

			if (!TryShoot()) // Try to shoot at the nearest enemy. If there are no enemies, stop trying to shoot until the next frame.
			{
				break;
			}

			_timeSinceLastShot -= secondsPerShot; // Subtract the time for one shot to allow for consistent firing even if there are frame rate drops or a high fire rate.
			shotsThisFrame++;
		}
    }

	// Attempts to shoot at the nearest enemy.
	private bool TryShoot()
	{
		Node2D target = FindNearestEnemy();

		if (target == null)
			return false;

		return ShootAt(target);
	}

	// Shoots a projectile towards the specified target. The projectile will move in the direction of the target at the specified speed and deal damage on impact.
	private bool ShootAt(Node2D target)
	{
		if (ProjectileScene == null)
		{
			GD.PushWarning($"{Name} has no ProjectileScene assigned.");
			return false;
		}

		if (_projectileContainer == null)
		{
			GD.PushWarning($"{Name} has no projectile container.");
			return false;
		}

		Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();

		Projectile projectile = ProjectileScene.Instantiate<Projectile>();
		_projectileContainer.AddChild(projectile);
		projectile.GlobalPosition = GlobalPosition;
		projectile.SetDirection(direction);
		return true;
	}

	// Finds the nearest enemy in the scene and returns it. Returns null if there are no enemies.
	private Node2D FindNearestEnemy()
	{
		Godot.Collections.Array<Node> enemies = GetTree().GetNodesInGroup("Enemies");
		Node2D nearestEnemy = null;
		float nearestDistanceSquared = float.MaxValue;

		foreach (Node node in enemies)
		{
			if (node is not Node2D enemy)
				continue;
			
			float distanceSquared = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);

			if (distanceSquared < nearestDistanceSquared)
			{
				nearestDistanceSquared = distanceSquared;
				nearestEnemy = enemy;
			}
		}

		return nearestEnemy;
	}

	// Retrieves the projectile container node based on the specified ProjectileContainerPath, falling back to an existing node or creating one if needed.
	private Node2D GetOrCreateProjectileContainer()
	{
		Node2D container = GetNodeOrNull<Node2D>(ProjectileContainerPath);

		if (container != null)
			return container;

		Node parent = GetTree().CurrentScene ?? GetParent();

		if (parent == null)
		{
			GD.PushWarning($"{Name} could not find a valid parent for ProjectileContainer.");
			return null;
		}

		container = parent.GetNodeOrNull<Node2D>("ProjectileContainer");

		if (container != null)
		{
			GD.PushWarning($"{Name} wasn't given a ProjectileContainerPath, but found a node named 'ProjectileContainer'. Using that node as the projectile container. Please assign ProjectileContainerPath to avoid this warning.");
			return container;
		}

		container = new Node2D
		{
			Name = "ProjectileContainer"
		};

		parent.AddChild(container);

		GD.PushWarning($"{Name} has no ProjectileContainerPath assigned and couldn't find a ProjectileContainer. Created a temporary ProjectileContainer node. Please assign ProjectileContainerPath to avoid this warning.");

		return container;
	}
}
