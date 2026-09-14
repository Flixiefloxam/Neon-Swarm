using System;
using System.Runtime;
using Godot;
using NeonSwarm.Weapons;

namespace NeonSwarm.Player;

public partial class PlayerVisualController : Node
{
	[ExportGroup("Eyes")]
	[Export] public Node2D Eyes { get; set; } // The parent node that holds both eyes. This node will get moved towards the enemy being shot at.

	[Export] public float MaxEyeOffset { get; set; } = 2f; // How far from their neutral position the eyes can move when looking at something.
	[Export] public float EyeMoveSpeed { get; set; } = 12f; // How fast the eyes will move towards a new position when looking.
	[Export] public float ReturnDelay { get; set; } = 1f; // How long the eyes need to be without a target.

	[ExportGroup("Movement")]
	[Export] public Node2D VisualDeformTransform { get; set; } // The node that's deformed for player visual deformation.

	[Export] public float StretchAmount { get; set; } = 0.06f; // How much the player's sprite gets deformed along the axis of movement.
	[Export] public float SquashAmount { get; set; } = 0.04f; // How much the player's sprite gets deformed perpendicular to the axis of movement.
	[Export] public float DeformResponsiveness { get; set; } = 12f; // How quickly the player's sprite squashes/stretches and returns to normal.
	[Export] public float DeformRotationResponsiveness { get; set; } = 10f; // How quickly the player's deformation will rotate to face a new movement direction.
	[Export] public float MaxVisualLag { get; set; } = 2.5f; // Maximum distance the player's sprite can trail behind the player's actual position.
	[Export] public float VisualLagResponsiveness { get; set; } = 10f; // How quickly the player's sprite move toward the target lag position.

	private BasicGun _basicGun; // The players eyes look towards the current target of BasicGun.
	private Vector2 _restingEyesPosition; // Where the eyes are in their resting position.
	private float _timeWithoutTarget; // How longs it's been since the player's eyes had a valid target to look at.
	private Vector2 _currentLookingTargetPosition; // What position the eyes are currently looking at.
	private Vector2 _restingVisualScale; // The player's visual scale when they're not bein deformed due to movement
	private float _restingDeformRotation; // The starting rotation of the VisualDeformTransform node.
	private float _restingVisualRotation; // The starting rotation of the _visuals node.
	private PlayerController _playerController;
	private Node2D _body; // The node containing player visuals. Needs to be the child of VisualDeformTransform.
	private Node2D _visuals; // The root node of all the player visuals
	private Vector2 _restingVisualPosition; // The starting position of the _visuals node.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Node player = GetParent();
		
		_basicGun = player.GetNodeOrNull<BasicGun>("BasicGun");
		if (_basicGun == null)
		{
			GD.PushError($"{Name} could not find BasicGun node.");
		}

		_playerController = player as PlayerController;
		if (_playerController == null)
		{
			GD.PushError($"{Name} must be a child of PlayerController.");
		}

		if (VisualDeformTransform == null) 
		{
			GD.PushWarning($"{Name} does not have a visuals node assigned.");
		}
		else
		{
			_restingVisualScale = VisualDeformTransform.Scale;
			_restingDeformRotation = VisualDeformTransform.Rotation;
		}

		_body = VisualDeformTransform.GetNodeOrNull<Node2D>("Body");
		if (_body == null)
		{
			GD.PushError($"{Name} could not find Body node.");
		}
		else
		{
			_restingVisualRotation = _body.Rotation;
		}

		if (Eyes == null)
		{
			GD.PushError(
				$"{Name} does not have an eyes node assigned."
			);
			return;
		}
		else
		{
			_restingEyesPosition = Eyes.Position;
		}

		_visuals = GetNodeOrNull<Node2D>("../Visuals");
		if (_visuals == null)
		{
			GD.PushError($"{Name} could not find Visuals node");
		}
		else
		{
			_restingVisualPosition = _visuals.Position;
		}

		_timeWithoutTarget = 0f;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float deltaFloat = (float)delta;
		UpdateEyeVisuals(deltaFloat);
		UpdateDeformVisuals(deltaFloat);
		UpdateVisualLag(deltaFloat);
	}

	private void UpdateVisualLag(float delta)
	{
		if (_visuals == null || _playerController == null)
			return;
		
		Vector2 velocity = _playerController.Velocity;
		Vector2 targetPosition = _restingVisualPosition;

		if (!velocity.IsZeroApprox())
		{
			Vector2 movementDirection = velocity.Normalized();

			float speedRatio = Mathf.Clamp(
				velocity.Length() / Mathf.Max(_playerController.MoveSpeed, 0.001f),
				0f,
				1f
			);

			Vector2 lagOffset = -movementDirection * MaxVisualLag * speedRatio;

			targetPosition = _restingVisualPosition + lagOffset;
		}

		float weight = 1f - Mathf.Exp(-VisualLagResponsiveness * delta);

		_visuals.Position = _visuals.Position.Lerp(targetPosition, weight);
	}

	private void UpdateDeformVisuals(float delta)
	{
		if (VisualDeformTransform == null || _playerController == null || _body == null)
			return;
		
		Vector2 targetScale = _restingVisualScale;
		Vector2 velocity = _playerController.Velocity;

		if (!velocity.IsZeroApprox())
		{
			float targetAngle = velocity.Angle();
			float rotationWeight = 1f - Mathf.Exp(-DeformRotationResponsiveness * delta);

			VisualDeformTransform.Rotation = Mathf.LerpAngle(VisualDeformTransform.Rotation, _restingDeformRotation + targetAngle, rotationWeight);

			targetScale = _restingVisualScale * new Vector2(
				1f + StretchAmount,
				1f - SquashAmount
			);
		}

		_body.Rotation = _restingVisualRotation - (VisualDeformTransform.Rotation - _restingDeformRotation);

		float scaleWeight = 1f - Mathf.Exp(-DeformResponsiveness * delta);

		VisualDeformTransform.Scale = VisualDeformTransform.Scale.Lerp(targetScale, scaleWeight);
	}

	private void UpdateEyeVisuals(float delta)
	{
		if (Eyes == null || _basicGun == null)
			return;
		
		Vector2 targetPosition = _restingEyesPosition;

		if (TryGetLookTarget())
		{
			_timeWithoutTarget = ReturnDelay;
		}
		else
		{
			_timeWithoutTarget -= delta;
		}

		if (_timeWithoutTarget > 0)
		{
			Vector2 direction = (_currentLookingTargetPosition - Eyes.GlobalPosition).Normalized();
			targetPosition = _restingEyesPosition + direction * MaxEyeOffset;
		}
		
		Eyes.Position = Eyes.Position.Lerp(targetPosition, 1f - Mathf.Exp(-EyeMoveSpeed * delta)); // smoothly moves eyes toward new position
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
