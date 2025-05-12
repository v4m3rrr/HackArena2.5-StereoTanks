namespace GameLogic;

/// <summary>
/// Handles periodic updates of player stun effects.
/// </summary>
internal sealed class StunSystem
{
    private readonly Dictionary<Tank, Dictionary<IStunEffect, int>> stunMap = [];

    /// <summary>
    /// Applies a stun to the specified tank.
    /// </summary>
    /// <param name="tank">The target tank to apply the stun to.</param>
    /// <param name="effect">The stun effect as flag to apply.</param>
    /// <param name="ticks">The number of ticks the stun should last.</param>
    public void ApplyStun(Tank tank, StunBlockEffect effect, int ticks)
    {
        var stunEffect = new StunEffect(ticks, effect);
        this.ApplyStun(tank, stunEffect);
    }

    /// <summary>
    /// Applies a stun to the specified tank.
    /// </summary>
    /// <param name="tank">The target tank.</param>
    /// <param name="stun">The stun effect to apply.</param>
    public void ApplyStun(Tank tank, IStunEffect stun)
    {
        if (!this.stunMap.TryGetValue(tank, out var effects))
        {
            effects = [];
            this.stunMap[tank] = effects;
        }

        if (effects.TryGetValue(stun, out int existingDuration))
        {
            if (existingDuration >= stun.StunTicks)
            {
                return;
            }
        }

        effects[stun] = stun.StunTicks;
    }

    /// <summary>
    /// Applies multiple stuns to the tank.
    /// </summary>
    /// <param name="tank">The tank to stun.</param>
    /// <param name="stuns">The collection of stuns to apply.</param>
    public void ApplyStuns(Tank tank, IEnumerable<IStunEffect> stuns)
    {
        foreach (var stun in stuns)
        {
            this.ApplyStun(tank, stun);
        }
    }

    /// <summary>
    /// Updates stun effects for all tanks.
    /// </summary>
    public void Update()
    {
        foreach (var (tank, effects) in this.stunMap)
        {
            foreach (var stun in effects.Keys.ToList())
            {
                effects[stun]--;

                if (effects[stun] <= 0)
                {
                    _ = effects.Remove(stun);
                }
            }
        }

        foreach (var tank in this.stunMap.Keys.ToList())
        {
            if (this.stunMap[tank].Count == 0)
            {
                _ = this.stunMap.Remove(tank);
            }
        }
    }

    /// <summary>
    /// Checks whether the tank is currently blocked by a given stun effect type.
    /// </summary>
    /// <param name="tank">The tank to check.</param>
    /// <param name="effect">The block effect to check against.</param>
    /// <returns>
    /// <see langword="true"/> if tank is blocked;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool IsBlocked(Tank tank, StunBlockEffect effect)
    {
        return this.stunMap.TryGetValue(tank, out var effects)
            && effects.Keys.Any(e => e.StunBlockEffect.HasFlag(effect));
    }

    private class StunEffect(int stunTicks, StunBlockEffect stunBlockEffect)
        : IStunEffect
    {
        /// <inheritdoc/>
        public int StunTicks { get; } = stunTicks;

        /// <inheritdoc/>
        public StunBlockEffect StunBlockEffect { get; } = stunBlockEffect;
    }
}
