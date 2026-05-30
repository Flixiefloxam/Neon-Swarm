using Godot;

namespace NeonSwarm.Player;

public partial class PlayerController : CharacterBody2D
{
	[Export] public float MoveSpeed = 300.0f;
	[Export] public float BodyRadius { get; set; } = 12f; // The radius of the player's body, used for calculating collisions and push effects.

	private Vector2 _moveDirection = Vector2.Zero;

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
		_moveDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		
		// Apply input direction to velocity, multiplied by move speed
		Velocity = _moveDirection * MoveSpeed;
		MoveAndSlide();
	}
}
