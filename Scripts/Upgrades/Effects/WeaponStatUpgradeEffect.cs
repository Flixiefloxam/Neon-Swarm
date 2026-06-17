using Godot;
using NeonSwarm.Resources;
using NeonSwarm.Weapons;

namespace NeonSwarm.Upgrades;

[GlobalClass]
public partial class WeaponStatUpgradeEffect : BaseUpgradeEffect
{
    [ExportGroup("Target")]
	[Export] public string TargetWeaponId { get; set; } = "basic_gun";

	[ExportGroup("Stat Change")]
	[Export] public WeaponStatType Stat { get; set; } = WeaponStatType.Damage; // The stat the upgrade modifies
	[Export] public UpgradeOperation Operation { get; set; } = UpgradeOperation.Add; // The operation applied to the modified stat
	[Export] public float Value { get; set; } = 1f;

    // Returns whether this effect can currently be applied.
    public override bool CanApply(UpgradeManager upgradeManager)
    {
        if (upgradeManager == null)
            return false;
        
        BaseWeapon weapon = upgradeManager.FindWeapon(TargetWeaponId);

        return weapon?.RuntimeStats != null;
    }

    // Applies this effect to the current run.
    public override void Apply(UpgradeManager upgradeManager)
    {
        BaseWeapon weapon = upgradeManager.FindWeapon(TargetWeaponId);

        if (weapon?.RuntimeStats == null)
        {
            GD.PushWarning($"Could not apply upgrade. Weapon '{TargetWeaponId}' was not found.");
			return;
        }

        ApplyToStats(weapon.RuntimeStats);
    }

    private void ApplyToStats(WeaponStats stats)
    {
        switch (Stat)
        {
            case WeaponStatType.Damage:
				stats.Damage = Mathf.Max(0f, ApplyOperation(stats.Damage));
				break;

			case WeaponStatType.FireRate:
				stats.FireRate = Mathf.Max(0.01f, ApplyOperation(stats.FireRate));
				break;

			case WeaponStatType.ProjectileSpeed:
				stats.ProjectileSpeed = Mathf.Max(0f, ApplyOperation(stats.ProjectileSpeed));
				break;

			case WeaponStatType.ProjectileLifetime:
				stats.ProjectileLifetime = Mathf.Max(0.05f, ApplyOperation(stats.ProjectileLifetime));
				break;

			case WeaponStatType.DamageCooldown:
				stats.DamageCooldown = Mathf.Max(0f, ApplyOperation(stats.DamageCooldown));
				break;

			case WeaponStatType.KnockbackStrength:
				stats.KnockbackStrength = Mathf.Max(0f, ApplyOperation(stats.KnockbackStrength));
				break;

			case WeaponStatType.ProjectileSpawnOffset:
				stats.ProjectileSpawnOffset = Mathf.Max(0f, ApplyOperation(stats.ProjectileSpawnOffset));
				break;
            
            default:
                GD.PushWarning($"Unhandled weapon stat type: {Stat}");
				break;
        }
    }

    private float ApplyOperation(float currentValue)
    {
        return Operation switch
        {
            UpgradeOperation.Add => currentValue + Value,
            UpgradeOperation.Multiply => currentValue * Value,
            _ => currentValue
        };
    }
}
