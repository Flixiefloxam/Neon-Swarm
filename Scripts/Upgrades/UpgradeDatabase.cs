using Godot;

namespace NeonSwarm.Upgrades;

[GlobalClass]
public partial class UpgradeDatabase : Resource
{
    // This holds the list of all possible upgrades
    [Export] public Godot.Collections.Array<UpgradeDefinition> Upgrades { get; set; } = new();
}
