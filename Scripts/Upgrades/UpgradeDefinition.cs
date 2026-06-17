using Godot;

namespace NeonSwarm.Upgrades;

[GlobalClass]
public partial class UpgradeDefinition : Resource
{
    [ExportGroup("Identity")]
	[Export] public string UpgradeId { get; set; } = "upgrade_id";
	[Export] public string DisplayName { get; set; } = "Upgrade";

	[Export(PropertyHint.MultilineText)]
	public string Description { get; set; } = "Upgrade description.";

	[ExportGroup("Rarity")]
	[Export] public UpgradeRarity Rarity { get; set; } = UpgradeRarity.Common;

	[ExportGroup("Stacking")]
	[Export] public int MaxStacks { get; set; } = 1; // 0 or less means unlimited.

	[ExportGroup("Effects")]
	[Export] public Godot.Collections.Array<BaseUpgradeEffect> Effects { get; set; } = new();

    // Is this upgrade valid to be offered to the player
    public bool CanBeSelected(UpgradeManager upgradeManager)
    {
        if (upgradeManager == null)
			return false;

		if (string.IsNullOrWhiteSpace(UpgradeId))
			return false;

		if (MaxStacks > 0 && upgradeManager.GetUpgradeStackCount(UpgradeId) >= MaxStacks)
			return false;

		if (Effects == null || Effects.Count == 0)
			return false;

        foreach (BaseUpgradeEffect effect in Effects)
        {
            if (effect == null)
				return false;

			if (!effect.CanApply(upgradeManager))
				return false;
        }

        return true;
    }
    
    public void Apply(UpgradeManager upgradeManager)
    {
        if (Effects == null)
            return;
        
        foreach (BaseUpgradeEffect effect in Effects)
        {
            effect?.Apply(upgradeManager);
        }
    }
}
