using Godot;
using NeonSwarm.Components;
using NeonSwarm.UI;
using NeonSwarm.Upgrades;

namespace NeonSwarm.Core;

public partial class GameManager : Node
{
	[Export] public NodePath PlayerPath { get; set; } = "../Player";
	[Export] public NodePath HudPath { get; set; } = "../Hud";
	[Export] public NodePath EnemySpawnerPath { get; set; } = "../EnemySpawner";
	[Export] public NodePath EnemyCrowdManagerPath { get; set; } = "../EnemyCrowdManager";
	[Export] public NodePath UpgradeManagerPath { get; set; } = "../UpgradeManager";
	[Export] public NodePath EnemyContainerPath { get; set; } = "../EnemyContainer";

	[Export] public bool ClearProjectilesOnGameOver { get; set; } = true;

	private Node2D _player;
	private HealthComponent _playerHealth;
	private Hud _hud;
	private Node _enemySpawner;
	private Node _enemyCrowdManager;
	private UpgradeManager _upgradeManager;
	private bool _isChoosingUpgrade;
	private ExperienceComponent _playerExperience;
	private Node _enemyContainer;
	private int _pendingUpgradeSelections = 0;

	private bool _isGameOver = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = GetNodeOrNull<Node2D>(PlayerPath);
		if (_player == null)
		{
			GD.PushError($"{Name} could not find Player.");
			return;
		}

		_playerHealth = _player.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (_playerHealth == null)
		{
			GD.PushError($"{Name} could not find player's HealthComponent.");
			return;
		}

		_hud = GetNodeOrNull<Hud>(HudPath);
		if (_hud == null)
		{
			GD.PushError($"{Name} could not find Hud.");
		}
		else
		{
			_hud.UpgradeSelected += OnUpgradeSelected;
		}
		
		_playerExperience = _player.GetNodeOrNull<ExperienceComponent>("ExperienceComponent");
		if (_playerExperience == null)
		{
			GD.PushError($"{Name} could not find player's ExperienceComponent.");
		}
		else
		{
			_playerExperience.LeveledUp += OnPlayerLeveledUp;
		}
		
		_upgradeManager = GetNodeOrNull<UpgradeManager>(UpgradeManagerPath);
		if (_upgradeManager == null)
			GD.PushError($"{Name} could not find upgrade manager.");
		
		_enemySpawner = GetNodeOrNull<Node>(EnemySpawnerPath);
		if (_enemySpawner == null)
			GD.PushError($"{Name} could not find enemy spawner.");
		
		_enemyCrowdManager = GetNodeOrNull<Node>(EnemyCrowdManagerPath);
		if (_enemyCrowdManager == null)
			GD.PushError($"{Name} could not find enemy crowd manager.");

		_enemyContainer = GetNodeOrNull<Node>(EnemyContainerPath);
		if (_enemyContainer == null)
			GD.PushError($"{Name} could not find enemy container.");

		_playerHealth.Died += OnPlayerDied;
	}

	private void OnPlayerLeveledUp(int newLevel)
	{
		if (_isGameOver)
			return;

		_pendingUpgradeSelections++;

		TryBeginUpgradeSelection();

	}

	private void TryBeginUpgradeSelection()
	{
		if (_isGameOver || _isChoosingUpgrade)
			return;

		if (_upgradeManager == null || _hud == null)
			return;
		
		while (_pendingUpgradeSelections > 0)
		{
			Godot.Collections.Array<UpgradeDefinition> choices = 
				_upgradeManager.GenerateUpgradeChoices();
			
			_pendingUpgradeSelections--;
			
			if (choices.Count == 0)
			{
				GD.PushWarning("Player leveled up, but no valid upgrades were available.");
				continue;
			}
			
			_isChoosingUpgrade = true;

			PauseGameplay();

			_hud.ShowLevelUpChoices(choices);

			return;
		}
	}

	private void OnUpgradeSelected(UpgradeDefinition upgrade)
	{
		if (!_isChoosingUpgrade)
			return;
		
		if (_upgradeManager == null)
			return;
		
		if (!_upgradeManager.ApplyUpgrade(upgrade))
			return;
		
		_isChoosingUpgrade = false;

		_hud.HideLevelUpScreen();

		TryBeginUpgradeSelection(); // Try to show another upgrade incase the player gained multiple levels at once

		if (!_isChoosingUpgrade)
			ResumeGameplay();
	}

	private void OnPlayerDied()
	{
		if (_isGameOver)
			return;
		
		_isGameOver = true;

		PauseGameplay(ClearProjectilesOnGameOver);

		_hud?.ShowGameOverScreen();
	}

	// Pauses active gameplay by freezing the player, weapon, enemy movement, and enemy spawning. Optionally clears all projectiles instead of freezing them.
	private void PauseGameplay(bool clearProjectiles = false)
	{
		DisableProcessingRecursive(_player);
		DisableProcessingRecursive(_enemySpawner);
		DisableProcessingRecursive(_enemyCrowdManager);
		DisableProcessingRecursive(_enemyContainer);

		_hud?.PauseGameTimer();

		if (clearProjectiles)
		{
			ClearProjectiles();
		}
		else
		{
			FreezeProjectiles();
		}
	}

	private void ResumeGameplay()
	{
		EnableProcessingRecursive(_player);
		EnableProcessingRecursive(_enemySpawner);
		EnableProcessingRecursive(_enemyCrowdManager);
		EnableProcessingRecursive(_enemyContainer);

		_hud?.ResumeGameTimer();

		ResumeProjectiles();
	}

	// This stops the provided node processing.
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

	private static void EnableProcessing(Node node)
	{
		if (node == null)
			return;

		node.SetProcess(true);
		node.SetPhysicsProcess(true);
		node.SetProcessInput(true);
		node.SetProcessUnhandledInput(true);
	}

	private static void EnableProcessingRecursive(Node node)
	{
		if (node == null)
			return;

		EnableProcessing(node);

		foreach (Node child in node.GetChildren())
		{
			EnableProcessingRecursive(child);
		}
	}

	private void ResumeProjectiles()
	{
		foreach (Node projectile in GetTree().GetNodesInGroup("Projectiles"))
		{
			EnableProcessingRecursive(projectile);
		}
	}

    public override void _ExitTree()
	{
		if (_playerHealth != null)
			_playerHealth.Died -= OnPlayerDied;
		
		if (_playerExperience != null)
			_playerExperience.LeveledUp -= OnPlayerLeveledUp;
		
		if (_hud != null)
			_hud.UpgradeSelected -= OnUpgradeSelected;
	}

}
