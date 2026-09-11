using Godot;

namespace NeonSwarm.Weapons;

public partial class BasicGun : BaseWeapon
{
	public Node2D CurrentTarget { get; private set; } // The enemy that's currently being targetted. Used so the player's eyes can look at the currently targetted enemy.

    // Attempts to shoot at the nearest enemy.
    protected override bool TryFire()
	{
		CurrentTarget = FindNearestEnemy();

		if (CurrentTarget == null)
			return false;
		
		return ShootAt(CurrentTarget);
	}

	private bool ShootAt(Node2D target)
	{
		Vector2 toTarget = target.GlobalPosition - GlobalPosition;

		if (toTarget.LengthSquared() <= 0.001f)
			return false;
		
		Vector2 direction = toTarget.Normalized();

		Vector2 spawnPosition =
			GlobalPosition +
			direction * RuntimeStats.ProjectileSpawnOffset;
		
		ProjectileSpawnData projectileData = CreateProjectileSpawnData(direction);

		return SpawnProjectile(spawnPosition, projectileData) != null;
	}

	// Finds nearest on screen enemy. Returns null if there are no enemies.
	private Node2D FindNearestEnemy()
	{
		Godot.Collections.Array<Node> enemies = GetTree().GetNodesInGroup("Enemies");

		Node2D nearestEnemy = null;
		float nearestDistanceSquared = float.MaxValue;

		foreach (Node node in enemies)
		{
			if (node is not Node2D enemy)
				continue;
			
			if (enemy.IsQueuedForDeletion())
				continue;
			
			if (!IsOnScreen(enemy))
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

	// Checks to see if the given Node2D is on the screen right now. Used to make sure an enemy is visible before targeting it.
	private bool IsOnScreen(Node2D enemy)
	{
		Vector2 viewportPosition = enemy.GetGlobalTransformWithCanvas().Origin;

		return GetViewportRect().HasPoint(viewportPosition);
	}
}
