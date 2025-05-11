namespace GameLogic;

/// <summary>
/// Resolves collisions between bullets and other objects (tanks, bullets).
/// </summary>
/// <param name="grid">The grid containing the bullets and other objects.</param>
/// <param name="damageSystem">The damage system for applying damage to tanks.</param>
/// <param name="scoreSystem">The score system for awarding points.</param>
internal sealed class BulletCollisionSystem(
    Grid grid,
    DamageSystem damageSystem,
    ScoreSystem scoreSystem)
{
    /// <summary>
    /// Resolves all bullet-related collisions passed from the detection phase.
    /// </summary>
    /// <param name="bullet">The bullet involved in the collision.</param>
    /// <param name="collision">The collision details.</param>
    public void ResolveCollision(Bullet bullet, Collision collision)
    {
        switch (collision)
        {
            case BulletCollision bulletCollision:
                this.ResolveBulletVsBullet(bullet, bulletCollision.Bullet);
                break;

            case TankCollision tankCollision:
                this.ResolveBulletVsTank(bullet, tankCollision.Tank);
                break;

            case Collision when collision.Type is CollisionType.Wall or CollisionType.Border:
                this.ResolveBulletVsWall(bullet, collision);
                break;
        }
    }

    private void ResolveBulletVsBullet(Bullet a, Bullet b)
    {
        _ = grid.Bullets.Remove(a);
        _ = grid.Bullets.Remove(b);

        bool onlyOneIsDouble = a is DoubleBullet ^ b is DoubleBullet;
        if (!onlyOneIsDouble)
        {
            return;
        }

        var doubleBullet = a is DoubleBullet ? a : b;

        var downgraded = new Bullet(
            doubleBullet.X,
            doubleBullet.Y,
            doubleBullet.Direction,
            doubleBullet.Speed,
            doubleBullet.Damage!.Value / 2,
            doubleBullet.Shooter!);

        grid.Bullets.Add(downgraded);
    }

    private void ResolveBulletVsTank(Bullet bullet, Tank tank)
    {
        _ = grid.Bullets.Remove(bullet);

        int dealt = damageSystem.ApplyDamage(tank, bullet.Damage!.Value, bullet.Shooter);

        if (dealt > 0 && bullet.Shooter is not null)
        {
            scoreSystem.AwardScore(bullet.Shooter, dealt / 2);
        }
    }

    private void ResolveBulletVsWall(Bullet bullet, Collision collision)
    {
        _ = grid.Bullets.Remove(bullet);
    }
}
