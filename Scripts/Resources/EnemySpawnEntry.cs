using Godot;

namespace NeonSwarm.Resources;

[GlobalClass]
public partial class EnemySpawnEntry : Resource
{
	[Export] public PackedScene EnemyScene { get; set; } // The enemy being spawned.

	[Export(PropertyHint.Range, "0,100,0.1")]
	public float Weight { get; set; } = 1f; // How likely the enemy is to spawn. Bigger number means more likely to spawn.

	[Export] public float MinimumElapsedTime { get; set; } = 0f; // How much time has to have passed until the enemy starts spawning.
}