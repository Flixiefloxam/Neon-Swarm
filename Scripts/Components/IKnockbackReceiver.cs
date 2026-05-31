using Godot;

namespace NeonSwarm.Components;

public interface IKnockbackReceiver
{
    void ApplyKnockback(Vector2 direction, float strength);
}
