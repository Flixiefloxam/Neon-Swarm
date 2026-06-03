using Godot;
using NeonSwarm.Components;

namespace NeonSwarm.Weapons;

public struct ProjectileSpawnData
{
    public Vector2 Direction;

    public float Speed;
	public float Lifetime;
	public float Damage;
	public HitboxDamageMode DamageMode;
	public float DamageCooldown;
	public int HitsUntilDestroyed;
	public DamageFaction TargetFactions;
	public float KnockbackStrength;
}
