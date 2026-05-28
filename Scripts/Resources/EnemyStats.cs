using Godot;

namespace NeonSwarm.Resources;

[GlobalClass]
public partial class EnemyStats : Resource
{
    [Export] public string EnemyName { get; set; } = "Enemy"; // The name of the enemy.
    [Export] public float MaxHealth { get; set; } = 3; // The maximum health of the enemy. When health reaches 0, the enemy dies.
    [Export] public float MoveSpeed { get; set; } = 200.0f; // The movement speed of the enemy.
    [Export] public float ContactDamage { get; set; } = 1; // The amount of damage the enemy deals to the player on contact.
    [Export] public int ScoreValue { get; set; } = 10; // The amount of points the player gets for defeating this enemy.
    [Export] public float ContactAttackCooldown { get; set; } = 0.75f; // The cooldown time in seconds between contact damage instances.
    [Export] public float KnockbackResistance { get; set; } = 1f; // How resistant the enemy is to being knockbacked by the player/attacks. Higher values make the enemy harder to knockback.
    [Export] public float KnockbackDamping { get; set; } = 900f; // How fast the enemy recovers from knockback. Higher values make the enemy recover faster.
    [Export] public Color BodyColor { get; set; } = Colors.Red; // The color of the enemy's body and glow. //TODO: add glow strength to stats and use it in GlowVisual.
    [Export] public float BodyRadius { get; set; } = 16f; // The radius of the enemy's body, used for calculating collisions and push effects.
    [Export] public float SeparationPadding { get; set; } = 2f;
    [Export] public float SeparationStrength { get; set; } = 500f; // The strength of the separation force applied to the enemy to prevent clustering with other enemies.
}
