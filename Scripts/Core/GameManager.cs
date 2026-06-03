using Godot;
using NeonSwarm.Components;
using NeonSwarm.UI;

namespace NeonSwarm.Core;

public partial class GameManager : Node
{
	[Export] public NodePath PlayerPath { get; set; } = "../Player";
	[Export] public NodePath HudPath { get; set; } = "../Hud";
	[Export] public NodePath EnemySpawnerPath { get; set; } = "../EnemySpawner";
	[Export] public NodePath EnemyCrowdManagerPath { get; set; } = "../EnemyCrowdManager";

	[Export] public bool ClearProjectilesOnGameOver { get; set; } = true;

	private Node2D _player;
	private HealthComponent _playerHealth;
	private Hud _hud;
	private Node _enemySpawner;
	private Node _enemyCrowdManager;

	private bool _isGameOver = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = GetNodeOrNull<Node2D>(PlayerPath);
		_hud = GetNodeOrNull<Hud>(HudPath);
		_enemySpawner = GetNodeOrNull<Node>(EnemySpawnerPath);
		_enemyCrowdManager = GetNodeOrNull<Node>(EnemyCrowdManagerPath);

		if (_player == null)
		{
			GD.PushError($"{Name} could not find Player.");
			return;
		}

		if (_hud == null)
			GD.PushError($"{Name} could not find Hud.");

		_playerHealth = _player.GetNodeOrNull<HealthComponent>("HealthComponent");

		if (_playerHealth == null)
		{
			GD.PushError($"{Name} could not find player's HealthComponent.");
			return;
		}

		_playerHealth.Died += OnPlayerDied;
	}

	private void OnPlayerDied()
	{
		if (_isGameOver)
			return;
		
		_isGameOver = true;

		StopGameplay();

		_hud?.StopGameTimer();
		_hud?.ShowGameOverScreen();
	}

	// Stops active gameplay by freezing the player, weapon, enemy movement, and enemy spawning.
	private void StopGameplay()
	{
		DisableProcessing(_player);

		Node basicGun = _player.GetNodeOrNull<Node>("BasicGun");
		DisableProcessing(basicGun);

		DisableProcessing(_enemySpawner);
		DisableProcessing(_enemyCrowdManager);

		if (ClearProjectilesOnGameOver)
		{
			ClearProjectiles();
		}
		else
		{
			FreezeProjectiles();
		}
	}

	// This stops the provided node processing, freezing it.
	private static void DisableProcessing(Node node)
	{
		if (node == null)
			return;
		
		node.SetProcess(false);
		node.SetPhysicsProcess(false);
		node.SetProcessInput(false);
		node.SetProcessUnhandledInput(false);
	}

	// Stops the provided node and its children from processing.
	private static void DisableProcessingRecursive(Node node)
	{
		if (node == null)
			return;

		DisableProcessing(node);

		foreach (Node child in node.GetChildren())
		{
			DisableProcessingRecursive(child);
		}
	}

	// Remove all projectiles
	private void ClearProjectiles()
	{
		foreach (Node projectile in GetTree().GetNodesInGroup("Projectiles"))
		{
			projectile.QueueFree();
		}
	}

	// Disables processing on all projectiles, freezing them.
	private void FreezeProjectiles()
	{
		foreach (Node projectile in GetTree().GetNodesInGroup("Projectiles"))
		{
			DisableProcessingRecursive(projectile); // The recursive method is used here to disable projectile hitboxes as well.
		}
	}

    public override void _ExitTree()
	{
		if (_playerHealth != null)
		_playerHealth.Died -= OnPlayerDied;
	}

}
