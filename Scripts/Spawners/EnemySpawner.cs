using Godot;

namespace NeonSwarm.Spawners;

public partial class EnemySpawner : Node
{
	[Export] public PackedScene EnemyScene { get; set; }
	[Export] public float SpawnRate { get; set; } = 0.5f; // Enemies spawned per second.
	[Export] public int MaxAliveEnemies { get; set; } = 50;
	[Export] public float SpawnDistanceFromPlayer { get; set; } = 700f;
	[Export] public NodePath EnemyContainerPath { get; set; }

	private Node2D _player;
	private Node2D _enemyContainer;
	private readonly RandomNumberGenerator _rng = new();
	private float _spawnAccumulator = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rng.Randomize();

		_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		_enemyContainer = GetOrCreateEnemyContainer();

		if (_enemyContainer == null)
    		GD.PushError($"{Name} could not create or find an enemy container.");
		if (_player == null)
			GD.PushWarning($"{Name} could not find a Player node.");
		if (EnemyScene == null)
			GD.PushWarning($"{Name} has no EnemyScene assigned.");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_player == null || _enemyContainer == null || EnemyScene == null || SpawnRate <= 0f)
			return;

		if (GetTree().GetNodeCountInGroup("Enemies") >= MaxAliveEnemies)
			return;

		_spawnAccumulator += (float)delta * SpawnRate;

		while (_spawnAccumulator >= 1f)
		{
			if (GetTree().GetNodeCountInGroup("Enemies") >= MaxAliveEnemies)
			{
				_spawnAccumulator = 0f; // Reset the accumulator if we've reached the max enemy count to prevent a backlog of spawns.
				break;
			}

			SpawnEnemy();
			_spawnAccumulator -= 1f; // Subtract one spawn from the accumulator to allow for consistent spawning even if there are frame rate drops or a high spawn rate.
		}
	}

	// Spawns a single enemy at a random position around the player. The enemy is added to the specified container node and positioned at the calculated spawn position.
	private void SpawnEnemy()
	{
		Node2D enemyInstance = EnemyScene.Instantiate<Node2D>();

		_enemyContainer.AddChild(enemyInstance);
		enemyInstance.GlobalPosition = GetSpawnPosition();
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

	// Retrieves the enemy container node based on the specified EnemyContainerPath, falling back to an existing node or creating one if needed.
	private Node2D GetOrCreateEnemyContainer()
	{
		Node2D container = GetNodeOrNull<Node2D>(EnemyContainerPath);

		if (container != null)
			return container;

		Node parent = GetTree().CurrentScene ?? GetParent();

		if (parent == null)
		{
			GD.PushWarning($"{Name} could not find a valid parent for EnemyContainer.");
			return null;
		}

		container = parent.GetNodeOrNull<Node2D>("EnemyContainer");

		if (container != null)
		{
			GD.PushWarning($"{Name} wasn't given an EnemyContainerPath, but found a node named 'EnemyContainer'. Using that node as the enemy container. Please assign EnemyContainerPath to avoid this warning.");
        	return container;
		}

		container = new Node2D
		{
			Name = "EnemyContainer"
		};

		parent.AddChild(container);
		GD.PushWarning($"{Name} has no EnemyContainerPath assigned and couldn't find an EnemyContainer. Created a temporary EnemyContainer node. Please assign EnemyContainerPath to avoid this warning.");
		return container;
	}
}
