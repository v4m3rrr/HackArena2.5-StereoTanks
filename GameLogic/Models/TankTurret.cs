using Newtonsoft.Json;

namespace GameLogic;

/// <summary>
/// Represents a tank turret.
/// </summary>
public class TankTurret
{
    /// <summary>
    /// Gets the direction of the turret.
    /// </summary>
    [JsonProperty]
    public Direction Direction { get; private set; } = EnumUtils.Random<Direction>();

    /// <summary>
    /// Rotates the turret.
    /// </summary>
    /// <param name="rotation">The rotation to apply.</param>
    public void Rotate(Rotation rotation)
    {
        this.Direction = rotation switch
        {
            Rotation.Left => EnumUtils.Previous(this.Direction),
            Rotation.Right => EnumUtils.Next(this.Direction),
            _ => throw new NotImplementedException(),
        };
    }
}
