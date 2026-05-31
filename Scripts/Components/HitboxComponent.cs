using Godot;
using System.Collections.Generic;

namespace NeonSwarm.Components;

public enum HitboxDamageMode
{
    OnEnter,
    OnCooldown
}

public partial class HitboxComponent : Area2D
{
    [Export] public float Damage { get; set; } = 1f;
    [Export] public DamageFaction TargetFactions { get; set; } = DamageFaction.None;
    [Export] public HitboxDamageMode DamageMode { get; set; } = HitboxDamageMode.OnEnter;
    [Export] public int HitsUntilDestroyed { get; set; } = 0; // 0 means infinite hits
    [Export] public float AttackCooldown { get; set; } = 0.75f;
    [Export] public NodePath OwnerPath { get; set; } = "..";

    [ExportGroup("Knockback")]
    [Export] public float KnockbackStrength { get; set; } = 0f; // How much knockback this hitbox inflicts when it damages something.
    [Export] public NodePath KnockbackOriginPath { get; set; } = ".."; // What node(and it's children) is actually getting knocked back.

    public Vector2 KnockbackDirectionOverride { get; set; } = Vector2.Zero; // This is used as the knockback direction unless it's zero. Used by projectiles for more natural knockback.

    private readonly Dictionary<ulong, float> _cooldowns = new();
    private int _hitsTaken = 0;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateCooldowns((float)delta);

        if (DamageMode != HitboxDamageMode.OnCooldown)
            return;
        
        foreach (Area2D area in GetOverlappingAreas())
        {
            TryDamage(area);
        }
    }

    public bool IsOverlappingTargetFaction()
    {
        foreach (Area2D area in GetOverlappingAreas())
        {
            if (area is not HurtboxComponent hurtbox)
                continue;
            
            if ((hurtbox.Faction & TargetFactions) != 0)
                return true;
        }
        
        return false;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (DamageMode != HitboxDamageMode.OnEnter)
            return;
        
        TryDamage(area);
    }

    private bool TryDamage(Area2D area)
    {
        if (area is not HurtboxComponent hurtbox)
            return false;
            
        if ((hurtbox.Faction & TargetFactions) == 0)
            return false;

        ulong hurtboxId = hurtbox.GetInstanceId();

        if (_cooldowns.TryGetValue(hurtboxId, out float cooldown) && cooldown > 0f)
            return false;

        hurtbox.TakeDamage(Damage);
        TryApplyKnockback(hurtbox);

        _cooldowns[hurtboxId] = AttackCooldown;

        if (HitsUntilDestroyed > 0 && ++_hitsTaken >= HitsUntilDestroyed)
            DestroyOwner();

        return true;
    }

    private void TryApplyKnockback(HurtboxComponent hurtbox)
    {
        if (KnockbackStrength <= 0f)
            return;

        IKnockbackReceiver receiver = FindKnockbackReceiver(hurtbox);

        if (receiver == null)
            return;
        
        Vector2 direction = GetKnockbackDirection(hurtbox);

        if (direction == Vector2.Zero)
            return;
        
        receiver.ApplyKnockback(direction, KnockbackStrength);
    }

    // Checks the hurtbox and then all of it's parents until it find a knockback receiver
    private IKnockbackReceiver FindKnockbackReceiver(HurtboxComponent hurtbox)
    {
        Node node = hurtbox;

        while (node != null)
        {
            if (node is IKnockbackReceiver receiver)
                return receiver;
            
            node = node.GetParent();
        }

        return null;
    }

    private Vector2 GetKnockbackDirection(HurtboxComponent hurtbox)
    {
        if (KnockbackDirectionOverride.LengthSquared() > 0.001f)
            return KnockbackDirectionOverride.Normalized();
        
        Node2D origin = GetNodeOrNull<Node2D>(KnockbackOriginPath);
        Vector2 originPosition = origin?.GlobalPosition ?? GlobalPosition;

        Vector2 direction = hurtbox.GlobalPosition - originPosition;

        if (direction.LengthSquared() <= 0.001f)
            return Vector2.Zero;
        
        return direction.Normalized();
    }

    private void UpdateCooldowns(float delta)
    {
        List<ulong> keys = new(_cooldowns.Keys);
        
        foreach (ulong key in keys)
        {
            _cooldowns[key] -= delta;

            if (_cooldowns[key] <= 0f)
                _cooldowns.Remove(key);
        }
    }

    private void DestroyOwner()
    {
        Node owner = GetNodeOrNull<Node>(OwnerPath);

        if (owner != null)
            owner.QueueFree();
        else
            QueueFree();
    }
}
