using Godot;
using NeonSwarm.Enemies;

namespace NeonSwarm.Player;

public partial class PlayerController : CharacterBody2D
{
	[Export] public float MoveSpeed = 300.0f;
	[Export] public float EnemyPushStrength { get; set; } = 250f; // The strength of the knockback applied to enemies when the player collides with them. Higher values result in stronger knockback.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Player");
		MotionMode = MotionModeEnum.Floating;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// Get the input direction as vector
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		
		//apply input direction to velocity, multiplied by move speed
		Velocity = direction * MoveSpeed;
		MoveAndSlide();
		//PushCollidingEnemies();
	}

	private void PushCollidingEnemies()
	{
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision2D collision = GetSlideCollision(i);
			
			if (collision.GetCollider() is not BaseEnemy enemy)
				continue;
			
			Vector2 pushDirection = (enemy.GlobalPosition - GlobalPosition).Normalized();

			if (pushDirection == Vector2.Zero)
				pushDirection = Velocity.Normalized();

			enemy.ApplyKnockback(pushDirection, EnemyPushStrength);
		}
	}
}
