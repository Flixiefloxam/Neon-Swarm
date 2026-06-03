using Godot;

namespace NeonSwarm.Weapons;

public partial class BasicGun : BaseWeapon
{
    // Attempts to shoot at the nearest enemy
    protected override bool TryFire()
	{
		Node2D target = FindNearestEnemy();

		if (target == null)
			return false;
		
		return ShootAt(target);
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

	// Finds nearest enemy in the scene. Returns null if there are no enemies.
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
			
			float distanceSquared = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);

			if (distanceSquared < nearestDistanceSquared)
			{
				nearestDistanceSquared = distanceSquared;
				nearestEnemy = enemy;
			}
		}

		return nearestEnemy;
	}
}
