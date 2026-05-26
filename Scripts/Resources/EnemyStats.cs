using Godot;

namespace NeonSwarm.Resources;

[GlobalClass]
public partial class EnemyStats : Resource
{
    [Export] public string EnemyName { get; set; } = "Enemy"; // The name of the enemy. Used for game over screen, score display and debugging.
    [Export] public int MaxHealth { get; set; } = 3; // The maximum health of the enemy. When health reaches 0, the enemy dies.
    [Export] public float MoveSpeed { get; set; } = 200.0f; // The movement speed of the enemy. Higher values make the enemy move faster.
    [Export] public int ContactDamage { get; set; } = 1; // The amount of damage the enemy deals to the player on contact. Hihher values mean more damage to the player.
    [Export] public int ScoreValue { get; set; } = 10; // The amount of points the player gets for defeating this enemy.

    [Export] public Color BodyColor { get; set; } = Colors.Red; // The color of the enemy's body and glow.
}
