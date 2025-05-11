using System.Threading.Tasks;

namespace GameLogic;

/// <summary>
/// Handles rotation logic for tanks and turrets.
/// </summary>
/// <param name="stunSystem">Stun system to check for stun effects.</param>
internal class RotationSystem(StunSystem stunSystem)
{
    /// <summary>
    /// Attempts to rotate a tank if not blocked by stun.
    /// </summary>
    /// <param name="tank">The tank to rotate.</param>
    /// <param name="rotation">The rotation direction.</param>
    public void TryRotateTank(Tank tank, Rotation rotation)
    {
        if (stunSystem.IsBlocked(tank, StunBlockEffect.TankRotation))
        {
            return;
        }

        tank.Direction = rotation switch
        {
            Rotation.Left => EnumUtils.Previous(tank.Direction),
            Rotation.Right => EnumUtils.Next(tank.Direction),
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Invalid rotation."),
        };
    }

    /// <summary>
    /// Attempts to rotate a turret if not blocked by stun.
    /// </summary>
    /// <param name="turret">The turret to rotate.</param>
    /// <param name="rotation">The rotation direction.</param>
    public void TryRotateTurret(Turret turret, Rotation rotation)
    {
        if (stunSystem.IsBlocked(turret.Tank, StunBlockEffect.TurretRotation))
        {
            return;
        }

        turret.Direction = rotation switch
        {
            Rotation.Left => EnumUtils.Previous(turret.Direction),
            Rotation.Right => EnumUtils.Next(turret.Direction),
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Invalid rotation."),
        };
    }
}
