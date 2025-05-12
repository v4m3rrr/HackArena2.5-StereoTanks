namespace GameLogic;

/// <summary>
/// Resolves collisions between bullets and other objects (tanks, bullets).
/// </summary>
/// <param name="grid">The grid containing the bullets and other objects.</param>
/// <param name="damageSystem">The damage system for applying damage to tanks.</param>
/// <param name="healSystem">The heal system for applying healing to tanks.</param>
/// <param name="scoreSystem">The score system for awarding points.</param>
/// <param name="stunSystem">The stun system for applying stun effects to tanks.</param>
internal sealed class BulletCollisionSystem(
    Grid grid,
    DamageSystem damageSystem,
    HealSystem healSystem,
    ScoreSystem scoreSystem,
    StunSystem stunSystem)
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

        bool onlyOneIsDouble = a.Type is BulletType.Double ^ b.Type is BulletType.Double;
        if (!onlyOneIsDouble)
        {
            return;
        }

        var doubleBullet = a.Type is BulletType.Double ? a : b;

        var downgraded = new Bullet(
            doubleBullet.X,
            doubleBullet.Y,
            doubleBullet.Direction,
            BulletType.Basic,
            doubleBullet.Speed,
            doubleBullet.Damage!.Value / 2,
            doubleBullet.Shooter!);

        grid.Bullets.Add(downgraded);
    }

    private void ResolveBulletVsTank(Bullet bullet, Tank tank)
    {
        _ = grid.Bullets.Remove(bullet);

        int dealt = 0;

        switch (bullet.Type)
        {
            case BulletType.Basic or BulletType.Double:
                dealt = damageSystem.ApplyDamage(tank, bullet.Damage!.Value, bullet.Shooter);
                break;

#if STEREO

            case BulletType.Healing:
                const int healingAmount = 20;
                healSystem.Heal(tank, healingAmount);
                break;

            case BulletType.Stun:
                const int ticks = 10;
                var stun = bullet.Shooter?.Tank.Type switch
                {
                    TankType.Heavy => StunBlockEffect.Movement,
                    TankType.Light => StunBlockEffect.AbilityUse,
                    _ => StunBlockEffect.None,
                };
                stunSystem.ApplyStun(tank, stun, ticks);
                break;

#endif
        }

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
