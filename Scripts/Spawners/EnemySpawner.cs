using Godot;
using Godot.Collections;
using NeonSwarm.Resources;

namespace NeonSwarm.Spawners;

public partial class EnemySpawner : Node
{
	[Export] public Array<EnemySpawnEntry> SpawnEntries { get; set; } = new();

	[ExportGroup("Spawning")]
	[Export] public float StartingSpawnRate { get; set; } = 0.5f; // Starting enemies spawned per second.
	[Export] public float SpawnRateIncreasePerMinute { get; set; } = 0.35f;
	[Export] public float MaxSpawnRate { get; set; } = 5f; // Maximum spawn rate per second for performance reasons.
	[Export] public float SpawnDistanceFromPlayer { get; set; } = 700f; // How far from the player the enemies spawn.
	[Export] public bool SpawnImmediatelyOnStart { get; set; } = true; // Does an enemy spawn as soon as this spawner is ready, so the player doesn't have to wait.

	[ExportGroup("Alive Cap")]
	[Export] public int StartingMaxAliveEnemies { get; set; } = 50; // Starting cap for simultaneously alive enemies.
	[Export] public float MaxAliveIncreasePerMinute { get; set; } = 35f;
	[Export] public int AbsoluteMaxAliveEnemies { get; set; } = 250; // Highest the enemy cap can go for performance reasons.

	[ExportGroup("Despawning")]
	[Export] public float DespawnDistanceFromPlayer { get; set; } = 1600f; // Max distance from the player enemies can be before being despawned.
	[Export] public float DespawnCheckInterval { get; set; } = 1f; // Time between each check to see if any enemies need to be despawned in seconds.

	private Node2D _player;
	private Node2D _enemyContainer;
	private readonly RandomNumberGenerator _rng = new();

	private float _spawnAccumulator = 0f;
	private float _despawnAccumulator = 0f;
	private float _elapsedRunTime = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rng.Randomize();
		_despawnAccumulator = Mathf.Max(DespawnCheckInterval, 0.1f);
		_spawnAccumulator = SpawnImmediatelyOnStart ? 1f : 0f;

		FindPlayer();
		_enemyContainer = GetOrCreateEnemyContainer();

		if (_enemyContainer == null)
			GD.PushError($"{Name} could not create or find an enemy container.");

		if (_player == null)
			GD.PushWarning($"{Name} could not find a Player node.");

		if (SpawnEntries == null || SpawnEntries.Count == 0)
			GD.PushWarning($"{Name} has no enemy spawn entries assigned.");
		
		if (DespawnDistanceFromPlayer <= SpawnDistanceFromPlayer)
		{
			GD.PushWarning(
				$"{Name} DespawnDistanceFromPlayer should be greater than SpawnDistanceFromPlayer. " +
				"Enemies may despawn too soon."
			);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_player == null)
			FindPlayer();
		
		if (_player == null || _enemyContainer == null)
			return;
		
		float deltaFloat = (float)delta;

		_elapsedRunTime += deltaFloat;

		_despawnAccumulator -= deltaFloat;
		if (_despawnAccumulator <= 0f)
		{
			_despawnAccumulator = Mathf.Max(DespawnCheckInterval, 0.1f);
			DespawnFarEnemies();
		}

		if (SpawnEntries == null || SpawnEntries.Count == 0)
			return;
		
		float currentSpawnRate = GetCurrentSpawnRate();

		if (currentSpawnRate <= 0f)
			return;
		
		int currentMaxAliveEnemies = GetCurrentMaxAliveEnemies();
		int aliveEnemyCount = GetAliveEnemyCount();

		if (aliveEnemyCount >= currentMaxAliveEnemies)
			return;

		_spawnAccumulator += deltaFloat * currentSpawnRate;

		while (_spawnAccumulator >= 1f && aliveEnemyCount < currentMaxAliveEnemies)
		{
			if (!SpawnEnemy())
			{
				// No enemy type is currently available, so do not retain a spawn backlog.
				_spawnAccumulator = 0f;
				break;
			}
			
			aliveEnemyCount++;
			_spawnAccumulator -= 1f; // Subtract one spawn from the accumulator to allow for consistent spawning even if there are frame rate drops or a high spawn rate.
		}

		if (aliveEnemyCount >= currentMaxAliveEnemies)
		{
			// Avoid storing up a huge spawn backlog while capped.
			_spawnAccumulator = 0f;
		}
	}

	private float GetCurrentSpawnRate()
	{
		float minutesAlive = _elapsedRunTime / 60f;

		float scaledSpawnRate = StartingSpawnRate + SpawnRateIncreasePerMinute * minutesAlive;

		float spawnRateCap = Mathf.Max(MaxSpawnRate, 0f);

		return Mathf.Clamp(scaledSpawnRate, 0f, spawnRateCap);
	}

	private int GetCurrentMaxAliveEnemies()
	{
		float minutesAlive = _elapsedRunTime / 60f;

		int startingCap = Mathf.Max(StartingMaxAliveEnemies, 0);

		int scaledMaxAlive =
			startingCap +
			(int)Mathf.Floor(MaxAliveIncreasePerMinute * minutesAlive);
		
		// If the starting enemy cap is bigger than the absolute enemy cap for some reason, then just use that.
		int absoluteCap = Mathf.Max(AbsoluteMaxAliveEnemies, startingCap);

		return Mathf.Clamp(scaledMaxAlive, 0, absoluteCap);
	}

	private int GetAliveEnemyCount()
	{
		int count = 0;

		foreach (Node node in GetTree().GetNodesInGroup("Enemies"))
		{
			if (node.IsQueuedForDeletion())
				continue;
			count++;
		}

		return count;
	}

	// Spawns a single enemy at a random position around the player. The enemy is added to the specified container node and positioned at the calculated spawn position.
	private bool SpawnEnemy()
	{
		PackedScene enemyScene = ChooseEnemyScene();

		if (enemyScene == null)
			return false;
		
		Node2D enemyInstance = enemyScene.Instantiate<Node2D>();

		Vector2 spawnPosition = GetSpawnPosition();
		enemyInstance.Position = _enemyContainer.ToLocal(spawnPosition);

		_enemyContainer.AddChild(enemyInstance);
		return true;
	}

	// Picks an enemy to spawn using the spawn entries list
	private PackedScene ChooseEnemyScene()
	{
		float totalWeight = 0f;

		foreach (EnemySpawnEntry entry in SpawnEntries)
		{
			if (!IsSpawnEntryAvailable(entry))
				continue;
			
			totalWeight += entry.Weight;
		}

		if (totalWeight <= 0f)
			return null;
		
		float roll = _rng.RandfRange(0f, totalWeight);

		foreach (EnemySpawnEntry entry in SpawnEntries)
		{
			if (!IsSpawnEntryAvailable(entry))
				continue;
			
			roll -= entry.Weight;

			if (roll <= 0f)
				return entry.EnemyScene;
		}

		return null;
	}

	// Is the given enemy spawn entry valid for spawning at this time in the run.
	private bool IsSpawnEntryAvailable(EnemySpawnEntry entry)
	{
		if (entry == null)
			return false;

		if (entry.EnemyScene == null)
			return false;

		if (entry.Weight <= 0f)
			return false;

		return _elapsedRunTime >= Mathf.Max(entry.MinimumElapsedTime, 0f);
	}

	// Calculates a random spawn position around the player at a specified distance.
	// The spawn position is determined by generating a random angle and placing the enemy at that angle from the player.
	private Vector2 GetSpawnPosition()
	{
		float angle = _rng.RandfRange(0f, Mathf.Tau);

		Vector2 direction = new Vector2(
			Mathf.Cos(angle),
			Mathf.Sin(angle)
		);
		return _player.GlobalPosition + direction * SpawnDistanceFromPlayer;
	}

	private void DespawnFarEnemies()
	{
		float despawnDistanceSquared =
			DespawnDistanceFromPlayer * DespawnDistanceFromPlayer;

		Vector2 playerPosition = _player.GlobalPosition;

		foreach (Node node in GetTree().GetNodesInGroup("Enemies"))
		{
			if (node is not Node2D enemy)
				continue;
			
			if (enemy.IsQueuedForDeletion())
				continue;
			
			float distanceSquared =
				enemy.GlobalPosition.DistanceSquaredTo(playerPosition);
			
			if (distanceSquared <= despawnDistanceSquared)
				continue;
			
			enemy.QueueFree();
		}
	}

	private void FindPlayer()
	{
		_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
	}

	// Retrieves the enemy container from the current scene, creating one if needed.
	private Node2D GetOrCreateEnemyContainer()
	{
		Node parent = GetTree().CurrentScene ?? GetParent();

		if (parent == null)
		{
			GD.PushWarning($"{Name} could not find a valid parent for EnemyContainer.");
			return null;
		}

		Node2D container = parent.GetNodeOrNull<Node2D>("EnemyContainer");

		if (container != null)
			return container;

		container = new Node2D
		{
			Name = "EnemyContainer"
		};

		parent.AddChild(container);
		GD.PushWarning($"{Name} couldn't find an EnemyContainer. Created a temporary EnemyContainer node. Please create a proper EnemyContainer node to avoid this warning.");
		return container;
	}
}
