using Godot;

namespace NeonSwarm.Resources;

[GlobalClass]
public partial class EnemyStats : Resource
{
    // ==================== General Stats ====================
    [ExportGroup("General")]
    [Export] public string EnemyName { get; set; } = "Enemy"; // The name of the enemy.
    [Export] public int ScoreValue { get; set; } = 10; // The amount of points the player gets for defeating this enemy.

    // ==================== Combat Stats ====================
    [ExportGroup("Combat")]
    [Export] public float MaxHealth { get; set; } = 3; // The maximum health of the enemy. When health reaches 0, the enemy dies.
    [Export] public float ContactDamage { get; set; } = 1; // The amount of damage the enemy deals to the player on contact.
    [Export] public float ContactAttackCooldown { get; set; } = 0.75f; // The cooldown time in seconds between contact damage instances.

    // ==================== Movement Stats ====================
    [ExportGroup("Movement")]
    [Export] public float MoveSpeed { get; set; } = 200.0f; // The movement speed of the enemy.
    [Export] public float SteeringAcceleration { get; set;} = 2400f; // The acceleration of the enemy.

    // ==================== Crowd Stats ====================
    [ExportGroup("Crowd")]
    [Export] public float CrowdRadius { get; set; } = 12f; // How big this enemy is considered by the Enemy crowd manager when preventing too much overlap.
    [Export] public float CrowdMass { get; set; } = 1; // How heavy this enemy is in crowd movement. Higher calues make it harder to push.

    // ==================== Knockback ====================
    [ExportGroup("Knockback")]
    [Export] public float KnockbackResistance { get; set; } = 1f; // How resistant the enemy is to being knocked back by the player/attacks. Higher values make the enemy harder to knock back.
    [Export] public float KnockbackDamping { get; set; } = 900f; // How fast the enemy recovers from knockback. Higher values make the enemy recover faster.

    // ==================== Visual ====================
    [ExportGroup("Visual")]
    [Export] public Color BodyColor { get; set; } = Colors.Red; // The color of the enemy's body and glow.
    [Export] public float GlowIntensity { get; set; } = 1.3f; // The intensity of the enemy's glow visual effect.
}
