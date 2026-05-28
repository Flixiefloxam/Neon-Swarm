using System;

namespace NeonSwarm.Components;

[Flags]
public enum DamageFaction
{
    None = 0,
    Player = 1 << 0,
    Enemy = 1 << 1,
    Neutral = 1 << 2,
    All = Player | Enemy | Neutral
}
