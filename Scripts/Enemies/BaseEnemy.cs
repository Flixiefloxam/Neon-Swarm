using Godot;
using NeonSwarm.Resources;
using NeonSwarm.Visuals;

namespace NeonSwarm.Enemies;

public partial class BaseEnemy : CharacterBody2D
{
	[Export] public EnemyStats Stats {get; set;}

	protected float CurrentHealth;
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
		CurrentHealth = Stats.MaxHealth;
		Target = GetTree().GetFirstNodeInGroup("Player") as Node2D;

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

	public void TakeDamage(int damage)
	{
		CurrentHealth -= damage;
		if (CurrentHealth <= 0)
			Die();
	}

	protected virtual void Die()
	{
		QueueFree();
	}

}
