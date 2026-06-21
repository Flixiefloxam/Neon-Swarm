using Godot;

namespace NeonSwarm.Spawners;

public partial class EnemySpawner : Node
{
	[Export] public PackedScene EnemyScene { get; set; }

	[ExportGroup("Spawning")]
	[Export] public float StartingSpawnRate { get; set; } = 0.5f; // Starting enemies spawned per second.
	[Export] public float SpawnRateIncreasePerMinute { get; set; } = 0.35f;
	[Export] public float MaxSpawnRate { get; set; } = 5f; // Maximum spawn rate per second for performance reasons.
	[Export] public float SpawnDistanceFromPlayer { get; set; } = 700f; // How far from the player the enemies spawn

	[ExportGroup("Alive Cap")]
	[Export] public int StartingMaxAliveEnemies { get; set; } = 50; // Starting cap for simultaneously alive enemies
	[Export] public float MaxAliveIncreasePerMinute { get; set; } = 35f;
	[Export] public int AbsoluteMaxAliveEnemies { get; set; } = 250; // Highest the enemy cap can go for performance reasons.

	[ExportGroup("Despawning")]
	[Export] public float DespawnDistanceFromPlayer { get; set; } = 1600f; // Max distance from the player enemies can be before being despawned
	[Export] public float DespawnCheckInterval { get; set; } = 1f; // Time between each check to see if any enemies need to be despawned in seconds

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
		_spawnAccumulator = 1f; // Have it so that one enemy spawn as soon as you load into the level so you don't need to wait.

		FindPlayer();
		_enemyContainer = GetOrCreateEnemyContainer();

		if (_enemyContainer == null)
    		GD.PushError($"{Name} could not create or find an enemy container.");

		if (_player == null)
			GD.PushWarning($"{Name} could not find a Player node.");

		if (EnemyScene == null)
			GD.PushWarning($"{Name} has no EnemyScene assigned.");
		
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
		if (_despawnAccumulator >= DespawnCheckInterval)
		{
			_despawnAccumulator = Mathf.Max(DespawnCheckInterval, 0.1f);
			DespawnFarEnemies();
		}

		if (EnemyScene == null)
			return;
		
		float currentSpawnRate = GetCurrentSpawnRate();

		if (currentSpawnRate <= 0f)
			return;
		
		int currentMaxAliveEnemies = GetCurrentMaxAliveEnemies();

		if (GetAliveEnemyCount() >= currentMaxAliveEnemies)
			return;

		_spawnAccumulator += deltaFloat * currentSpawnRate;

		while (_spawnAccumulator >= 1f)
		{
			if (GetAliveEnemyCount() >= currentMaxAliveEnemies)
			{
				// Avoid storing up a huge spawn backlog while capped.
				_spawnAccumulator = 0f;
				break;
			}

			SpawnEnemy();
			_spawnAccumulator -= 1f; // Subtract one spawn from the accumulator to allow for consistent spawning even if there are frame rate drops or a high spawn rate.
		}
	}

	private float GetCurrentSpawnRate()
	{
		float minutesAlive = _elapsedRunTime / 60f;

		return Mathf.Min(
			StartingSpawnRate + SpawnRateIncreasePerMinute * minutesAlive,
			MaxSpawnRate
		);
	}

	private int GetCurrentMaxAliveEnemies()
	{
		float minutesAlive = _elapsedRunTime / 60f;

		int scaledMaxAlive =
			StartingMaxAliveEnemies +
			(int)Mathf.Floor(MaxAliveIncreasePerMinute * minutesAlive);
		
		// If the starting enemy cap is bigger than the absolute enemy cap for some reason, then just use that.
		int absoluteCap = Mathf.Max(AbsoluteMaxAliveEnemies, StartingMaxAliveEnemies);

		return Mathf.Min(scaledMaxAlive, absoluteCap);
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
	private void SpawnEnemy()
	{
		Node2D enemyInstance = EnemyScene.Instantiate<Node2D>();

		Vector2 spawnPosition = GetSpawnPosition();
		enemyInstance.Position = _enemyContainer.ToLocal(spawnPosition);

		_enemyContainer.AddChild(enemyInstance);
	}

	// Calculates a random spawn position around the player at a specified distance. The spawn position is determined by generating a random angle and placing the enemy at that angle from the player.
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
			
			if(enemy.IsQueuedForDeletion())
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
