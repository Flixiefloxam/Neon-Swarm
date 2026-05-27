using Godot;
using NeonSwarm.Enemies;

namespace NeonSwarm.Weapons;

public partial class Projectile : Area2D
{
	[Export] public float Speed = 400f;
	[Export] public float Damage = 1f;
	[Export] public float Lifetime = 2f;

	private Vector2 _direction = Vector2.Right;
	private float _lifeRemaining = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Projectiles");

		_lifeRemaining = Lifetime;
		AreaEntered += OnAreaEntered;
	}

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _direction * Speed * (float)delta;

		_lifeRemaining -= (float)delta;
		if (_lifeRemaining <= 0f)
			QueueFree();
    }

	// Sets the direction of the projectile. The projectile will move in this direction at the specified speed.
	public void SetDirection(Vector2 direction)
	{
		if (direction == Vector2.Zero)
			return;

		_direction = direction.Normalized();
		Rotation = _direction.Angle();
	}

	// Called when the projectile enters an area. If the area belongs to an enemy, the enemy takes damage and the projectile is destroyed.
	private void OnAreaEntered(Area2D area)
	{
		BaseEnemy enemy = area.GetParentOrNull<BaseEnemy>();

		if (enemy == null)
			return;
		
		enemy.TakeDamage(Damage);
		QueueFree();
	}
}
