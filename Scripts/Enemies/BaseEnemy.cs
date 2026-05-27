using Godot;
using NeonSwarm.Resources;
using NeonSwarm.Visuals;
using NeonSwarm.Components;

namespace NeonSwarm.Enemies;

public partial class BaseEnemy : CharacterBody2D
{
	[Export] public EnemyStats Stats {get; set;}

	protected HealthComponent Health;
	protected Node2D Target;

	private GlowVisual _glowVisual;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Enemies");
		MotionMode = MotionModeEnum.Floating;

		if (Stats == null)
		{
			GD.PushWarning($"{Name} has no EnemyStats assigned. Using default values.");
			Stats = new EnemyStats();
		}

		Health = GetNodeOrNull<HealthComponent>("HealthComponent");
		Target = GetTree().GetFirstNodeInGroup("Player") as Node2D;

		if (Health == null)
		{
			GD.PushWarning($"{Name} has no HealthComponent.");
		}
		else
		{
			Health.SetMaxHealth(Stats.MaxHealth);
			Health.Died += Die;
		}

		_glowVisual = GetNodeOrNull<GlowVisual>("Visuals");
		if (_glowVisual != null)
		{
			_glowVisual.ApplyColor(Stats.BodyColor);
		}
		else
		{
			GD.PushWarning($"{Name} has no GlowVisual child.");
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
		if (Target == null || Stats == null)
			return;
		
		MoveTowardsTarget();
	}

	protected virtual void MoveTowardsTarget()
	{
		Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();

		Velocity = direction * Stats.MoveSpeed;
		MoveAndSlide();
	}

	protected virtual void Die()
	{
		QueueFree();
	}

	public override void _ExitTree()
	{
		if (Health != null)
			Health.Died -= Die;
	}

}
