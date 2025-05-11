using System.Diagnostics;

namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a declared tank stub.
/// </summary>
/// <param name="owner">The owner of the tank.</param>
/// <param name="declaredTankType">The declared tank type.</param>
[DebuggerDisplay("{Type}")]
public class DeclaredTankStub(Player owner, TankType declaredTankType)
    : Tank(-1, -1, Direction.Up, owner)
{
    /// <inheritdoc/>
    /// <value>The declared tank type.</value>
    public override TankType Type { get; } = declaredTankType;
}

#endif
