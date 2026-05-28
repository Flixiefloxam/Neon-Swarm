using Godot;
using NeonSwarm.Enemies;
using NeonSwarm.Components;

namespace NeonSwarm.Player;

public partial class PlayerController : CharacterBody2D
{
	[Export] public float MoveSpeed = 300.0f;
	[Export] public float EnemyPushStrength { get; set; } = 250f; // The strength of the knockback applied to enemies when the player collides with them. Higher values result in stronger knockback.
	// [Export] public float EnemyPushReach { get; set; } = 36f;
	// [Export] public float EnemyPushHalfWidth { get; set; } = 14f;
	[Export] public NodePath EnemyPushAreaPath { get; set; } = "EnemyPushArea"; // The path to the Area2D node used for detecting and pushing enemies on collision.
	[Export] public float BodyRadius { get; set; } = 12f; // The radius of the player's body, used for calculating collisions and push effects.
	[Export] public float EnemyPushReachPadding { get; set; } = 18f;
	[Export] public float EnemyPushHalfWidthPadding { get; set; } = 2f;
	[Export] public float EnemyPushSideBias { get; set; } = 0.1f; // How much to bias the push direction towards the side when pushing enemies. This prevent enemies from sticking to the player's face when moving directly into them, and makes the push feel more natural.

	private Area2D _enemyPushArea;
	private Vector2 _moveDirection = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Player");
		MotionMode = MotionModeEnum.Floating;

		_enemyPushArea = GetNodeOrNull<Area2D>(EnemyPushAreaPath);
		if (_enemyPushArea == null)
			GD.PushWarning($"{Name} could not find EnemyPushArea.");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// Get the input direction as vector
		_moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		
		//apply input direction to velocity, multiplied by move speed
		Velocity = _moveDirection * MoveSpeed;
		MoveAndSlide();

		PushEnemiesInMoveDirection();
	}

	private void PushEnemiesInMoveDirection()
	{
		if (_enemyPushArea == null || _moveDirection == Vector2.Zero)
			return;

		Vector2 pushDirection = _moveDirection.Normalized();
		Vector2 sideDirection = new Vector2(-pushDirection.Y, pushDirection.X); // Perpendicular direction to the push direction.
		
		foreach (Area2D area in _enemyPushArea.GetOverlappingAreas())
		{
			if (area is not HurtboxComponent hurtbox)
				continue;
			if (hurtbox.Faction != DamageFaction.Enemy)
				continue;
			if (hurtbox.GetParent() is not BaseEnemy enemy)
				continue;
			
			Vector2 toEnemy = enemy.GlobalPosition - GlobalPosition;

			float forwardDistance = toEnemy.Dot(pushDirection);
			float sideDistance = Mathf.Abs(toEnemy.Dot(sideDirection));

			float pushReach = BodyRadius + enemy.BodyRadius + EnemyPushReachPadding;
			float pushHalfWidth = BodyRadius + EnemyPushHalfWidthPadding;

			if (forwardDistance <= 0f)
				continue; // Enemy is behind the player, so don't push.
			if (forwardDistance > pushReach)
				continue; // Enemy is out of reach in the forward direction, so don't push.
			if (sideDistance > pushHalfWidth)
				continue; // Enemy is too far to the side, so don't push.

			Vector2 awayFromPlayer = toEnemy.Normalized();
			float sideAmount = toEnemy.Dot(sideDirection);

			float sideSign; // Determine the direction to push on the side axis so enemies on the left get pushed left and enemies on the right get pushed right.

			if (Mathf.Abs(sideAmount) > 0.01f)
			{
				sideSign = Mathf.Sign(sideAmount);
			}
			else
			{
				// If the enemy is almost exactly centered in front of the player,
				// pick a stable side based on its instance ID.
				sideSign = enemy.GetInstanceId() % 2 == 0 ? 1f : -1f;
			}

			Vector2 slideDirection = sideDirection * sideSign; // The direction to slide along the player's face.
			Vector2 finalPushDirection = (awayFromPlayer + slideDirection * EnemyPushSideBias).Normalized(); // Combine the away direction with the slide direction to get the final push direction.

			enemy.ApplyPush(finalPushDirection, EnemyPushStrength);
		}
	}
}
