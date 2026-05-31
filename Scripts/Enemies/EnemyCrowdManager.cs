using Godot;
using System.Collections.Generic;
using NeonSwarm.Player;

namespace NeonSwarm.Enemies;

public partial class EnemyCrowdManager : Node
{
    [Export] public int SolverIterations { get; set; } = 6;
    [Export] public float OverlapCorrectionStrength { get; set; } = 1f;
    [Export] public float FallbackPlayerRadius { get; set; } = 12f;

    private Node2D _player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        FindPlayer();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null)
            FindPlayer();
        if (_player == null)
            return;
        
        float deltaFloat = (float)delta; // Casts delta into a float and stores it, as delta is needed as both a float and double in several parts of this method.

        if (deltaFloat <= 0f)
            return;
        
        List<BaseEnemy> enemies = GetEnemies();

        if (enemies.Count == 0)
            return;
        
        Dictionary<BaseEnemy, Vector2> predictedPositions = new(enemies.Count);

        foreach (BaseEnemy enemy in enemies) // Get and store where each enemy is trying to go (desired velocity + knockback) this frame.
        {
            enemy.PrepareCrowdMovement(delta);

            Vector2 predictedPosition = 
                enemy.GlobalPosition +
                enemy.GetCrowdVelocity() * deltaFloat;
            
            predictedPositions[enemy] = predictedPosition;
        }
        
        for (int i = 0; i < SolverIterations; i++) // Resolve enemies trying to overlap with the player and each other (Up to the solver iteration limit).
        {
            ResolvePlayerEnemyOverlaps(enemies, predictedPositions);
            ResolveEnemyEnemyOverlaps(enemies, predictedPositions);
        }

        foreach (BaseEnemy enemy in enemies)
        {
            Vector2 oldPosition = enemy.GlobalPosition;
            Vector2 newPosition = predictedPositions[enemy];

            enemy.GlobalPosition = newPosition;
            enemy.Velocity = (newPosition - oldPosition) / deltaFloat;

            enemy.FinishCrowdMovement(delta);
        }
    }

    // Finds the player and sets _player.
    private void FindPlayer()
    {
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

        if (_player == null)
            GD.PushWarning($"{Name} could not find Player.");
    }

    // Find all the enemies in the current scene and returns them as a list
    private List<BaseEnemy> GetEnemies()
    {
        List<BaseEnemy> enemies = new();

        foreach (Node node in GetTree().GetNodesInGroup("Enemies"))
        {
            if (node is not BaseEnemy enemy)
                continue;
            
            if (!enemy.IsInsideTree())
                continue;
            
            enemies.Add(enemy);
        }

        return enemies;
    }

    private void ResolvePlayerEnemyOverlaps(List<BaseEnemy> enemies, Dictionary<BaseEnemy, Vector2> positions)
    {
        float playerRadius = GetPlayerRadius();
        Vector2 playerPosition = _player.GlobalPosition;

        foreach (BaseEnemy enemy in enemies)
        {
            Vector2 enemyPosition = positions[enemy];

            Vector2 awayFromPlayer = enemyPosition - playerPosition;
            float distance = awayFromPlayer.Length();

            float minimumDistance = playerRadius + enemy.CrowdRadius;

            if (distance >= minimumDistance)
                continue; // If this enemy is already far enough away from the player, skip them.
            
            Vector2 normal;

            if (distance <= 0.001f) // If the enemy is in exactly the same place as the player, use a stable fallback direction so they can be pushed apart.
            {
                normal = GetStableNormal(enemy);
                distance = 0.001f;
            }
            else
            {
                normal = awayFromPlayer / distance;
            }

            float penetration = minimumDistance - distance;
            positions[enemy] += normal * penetration * OverlapCorrectionStrength;
        }
    }

    private void ResolveEnemyEnemyOverlaps(List<BaseEnemy> enemies, Dictionary<BaseEnemy, Vector2> positions)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            for (int j = i + 1; j < enemies.Count; j++)
            {
                BaseEnemy enemyA = enemies[i];
                BaseEnemy enemyB = enemies[j];

                Vector2 positionA = positions[enemyA];
                Vector2 positionB = positions[enemyB];

                Vector2 awayFromB = positionA - positionB;
                float distance = awayFromB.Length();

                float minimumDistance =
                    enemyA.CrowdRadius +
                    enemyB.CrowdRadius;

                if (distance >= minimumDistance)
                    continue; // If enemyB is already far enough away from enemyA, skip them.
                
                Vector2 normal;

                if (distance <= 0.001f) // If the enemyA is in exactly the same place as enemyB, use a stable fallback direction so they can be pushed apart.
                {
                    normal = GetStableNormal(enemyA, enemyB);
                    distance = 0.001f;
                }
                else
                {
                    normal = awayFromB / distance;
                }

                float penetration = minimumDistance - distance;

                // Calculates inverse mass for enemies so heavier enemies get moved less
                float inverseMassA = 1f / enemyA.CrowdMass;
                float inverseMassB = 1f / enemyB.CrowdMass;
                float inverseMassTotal = inverseMassA + inverseMassB;

                if (inverseMassTotal <= 0f)
                    continue;

                Vector2 correction =
                    normal *
                    (penetration / inverseMassTotal) *
                    OverlapCorrectionStrength;
                
                positions[enemyA] += correction * inverseMassA;
                positions[enemyB] -= correction * inverseMassB;
            }
        }
    }

    private float GetPlayerRadius()
    {
        if (_player is PlayerController player)
            return player.BodyRadius;
        
        return FallbackPlayerRadius;
    }


    // Used when an enemy and the player are in the exact same place to get a direction at which to push the enemy away in.
    private static Vector2 GetStableNormal(BaseEnemy enemy)
    {
        ulong hash = enemy.GetInstanceId();
        float angle = (hash % 360UL) * Mathf.Pi / 180f;

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    // Used when two enemies are in the exact same place to get a direction at which to push them apart.
    private static Vector2 GetStableNormal(BaseEnemy enemyA, BaseEnemy enemyB)
    {
        ulong hash =
            enemyA.GetInstanceId() ^
            (enemyB.GetInstanceId() * 31UL);
        
        float angle = (hash % 360UL) * Mathf.Pi / 180f;

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}
