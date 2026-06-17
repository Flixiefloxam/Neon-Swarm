using Godot;
using NeonSwarm.Components;

namespace NeonSwarm.Resources;

[GlobalClass]
public partial class WeaponStats : Resource
{
    // ==================== General ====================
    [ExportGroup("General")]
	[Export] public string WeaponId { get; set; } = "weapon_id"; // naming convention is snake_case
    [Export] public string WeaponName { get; set; } = "Weapon"; // The name displayed on ingame Ui

	// ==================== Firing ====================
	[ExportGroup("Firing")]
	[Export] public PackedScene ProjectileScene { get; set; }
	[Export] public float FireRate { get; set; } = 1f; // Shots per second.
	[Export] public float ProjectileSpawnOffset { get; set; } = 16f; // Distance from weapon position where projectiles spawn.

	// ==================== Projectile ====================
	[ExportGroup("Projectile")]
	[Export] public float ProjectileSpeed { get; set; } = 400f;
	[Export] public float ProjectileLifetime { get; set; } = 2f; // How long the projectile lasts in seconds before disappearing if it doesn't hit anything.
	[Export] public int HitsUntilDestroyed { get; set; } = 1; // 0 means infinite hits.

	// ==================== Damage ====================
	[ExportGroup("Damage")]
	[Export] public float Damage { get; set; } = 1f;
	[Export] public HitboxDamageMode DamageMode { get; set; } = HitboxDamageMode.OnEnter;
	[Export] public float DamageCooldown { get; set; } = 0.75f; // Cooldown time in seconds between damage instances for the same target when using modes that allow multiple hits (OnCooldown)
	[Export] public DamageFaction TargetFactions { get; set; } = DamageFaction.Enemy;

	// ==================== Knockback ====================
	[ExportGroup("Knockback")]
	[Export] public float KnockbackStrength { get; set; } = 350f;
}
