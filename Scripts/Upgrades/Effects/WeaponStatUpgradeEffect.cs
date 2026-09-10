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
	[Export] public UpgradeOperation Operation { get; set; } = UpgradeOperation.AddFlat; // The operation applied to the modified stat
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

        ApplyToStats(weapon);
    }

    private void ApplyToStats(BaseWeapon weapon)
    {
        WeaponStats runtimeStats = weapon.RuntimeStats;
        WeaponStats baseStats = weapon.BaseStats ?? runtimeStats;

        switch (Stat)
        {
            case WeaponStatType.Damage:
				runtimeStats.Damage = Mathf.Max(0f, ApplyOperation(runtimeStats.Damage, baseStats.Damage));
				break;

			case WeaponStatType.FireRate:
				runtimeStats.FireRate = Mathf.Max(0.01f, ApplyOperation(runtimeStats.FireRate, baseStats.FireRate));
				break;

			case WeaponStatType.ProjectileSpeed:
				runtimeStats.ProjectileSpeed = Mathf.Max(0f, ApplyOperation(runtimeStats.ProjectileSpeed, baseStats.ProjectileSpeed));
				break;

			case WeaponStatType.ProjectileLifetime:
				runtimeStats.ProjectileLifetime = Mathf.Max(0.05f, ApplyOperation(runtimeStats.ProjectileLifetime, baseStats.ProjectileLifetime));
				break;

			case WeaponStatType.DamageCooldown:
				runtimeStats.DamageCooldown = Mathf.Max(0f, ApplyOperation(runtimeStats.DamageCooldown, baseStats.DamageCooldown));
				break;

			case WeaponStatType.KnockbackStrength:
				runtimeStats.KnockbackStrength = Mathf.Max(0f, ApplyOperation(runtimeStats.KnockbackStrength, baseStats.KnockbackStrength));
				break;

			case WeaponStatType.ProjectileSpawnOffset:
				runtimeStats.ProjectileSpawnOffset = Mathf.Max(0f, ApplyOperation(runtimeStats.ProjectileSpawnOffset, baseStats.ProjectileSpawnOffset));
				break;
            
            default:
                GD.PushWarning($"Unhandled weapon stat type: {Stat}");
				break;
        }
    }

    private float ApplyOperation(float currentValue, float baseValue)
    {
        return Operation switch
        {
            UpgradeOperation.AddFlat => currentValue + Value,
            UpgradeOperation.AddPercentOfBase => currentValue + (baseValue * Value),
            UpgradeOperation.MultiplyTotal => currentValue * Value,
            _ => currentValue
        };
    }
}
