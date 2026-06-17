using Godot;

namespace NeonSwarm.Upgrades;

[GlobalClass]
public abstract partial class BaseUpgradeEffect : Resource
{
	// Returns whether this effect can currently be applied.
	public abstract bool CanApply(UpgradeManager upgradeManager);

	// Applies this effect to the current run.
	public abstract void Apply(UpgradeManager upgradeManager);
}