using System.Runtime;
using Godot;
using NeonSwarm.Weapons;

namespace NeonSwarm.Player;

public partial class PlayerVisualController : Node
{
	[ExportGroup("Eyes")]
	[Export] public float MaxEyeOffset { get; set; } = 2f; // How far from their neutral position the eyes can move when looking at something.
	[Export] public float EyeMoveSpeed { get; set; } = 12f; // How fast the eyes will move towards a new position when looking.
	[Export] public float ReturnDelay { get; set; } = 1f; // How long the eyes need to be without a target.
	[Export] public Node2D Eyes { get; set; } // The parent node that holds both eyes.

	private BasicGun _basicGun; // The players eyes look towards the current target of BasicGun.
	private Vector2 _restingEyesPosition; // Where the eyes are in their resting position.
	private float _timeWithoutTarget;
	private Vector2 _currentLookingTargetPosition; // What position the eyes are currently looking at.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Node player = GetParent();
		
		_basicGun = player.GetNodeOrNull<BasicGun>("BasicGun");
		if (_basicGun == null) GD.PushError($"{Name} could not find BasicGun node.");
		_timeWithoutTarget = 0f;

		if (Eyes == null)
		{
			GD.PushError(
				$"{Name} does not have an eyes node assigned."
			);
			return;
		}
		_restingEyesPosition = Eyes.Position;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Eyes == null || _basicGun == null)
			return;
		
		float deltaFloat = (float)delta;
		Vector2 targetPosition = _restingEyesPosition;

		if (TryGetLookTarget())
		{
			_timeWithoutTarget = ReturnDelay;
		}
		else
		{
			_timeWithoutTarget -= deltaFloat;
		}

		if (_timeWithoutTarget > 0)
		{
			Vector2 direction = (_currentLookingTargetPosition - Eyes.GlobalPosition).Normalized();
			targetPosition = _restingEyesPosition + direction * MaxEyeOffset;
		}
		
		Eyes.Position = Eyes.Position.Lerp(targetPosition, 1f - Mathf.Exp(-EyeMoveSpeed * deltaFloat)); // smoothly moves eyes toward new position
	}

	// Finds where the eyes should be looking and puts it in _currentLookingTargetPosition.
	private bool TryGetLookTarget()
	{
		Node2D target = _basicGun.CurrentTarget;
		if (IsInstanceValid(target) && !target.IsQueuedForDeletion())
		{
			_currentLookingTargetPosition = target.GlobalPosition;
			return true;
		}
		
		return false;
	}
}
