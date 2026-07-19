using Godot;

namespace NeonSwarm.Player;

public partial class PlayerController : CharacterBody2D
{
	[ExportGroup("Movement")]
	[Export] public float MoveSpeed = 300.0f;
	[Export] public float BodyRadius { get; set; } = 12f; // The radius used by EnemyCrowdManager when pushing enemies away from the player.

	[ExportGroup("Pickups")]
	[Export] public float PickupAttractionRadius { get; set; } = 140f;

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
