namespace GameClient.Sprites;

/// <summary>
/// Represents a double bullet sprite.
/// </summary>
internal class DoubleBullet : Bullet
{
    private static readonly ScalableTexture2D.Static StaticTexture;

    static DoubleBullet()
    {
        StaticTexture = new ScalableTexture2D.Static("Images/Game/double_bullet_ts.svg");
        StaticTexture.Load();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleBullet"/> class.
    /// </summary>
    /// <param name="logic">The double bullet logic.</param>
    /// <param name="grid">The grid component.</param>
    public DoubleBullet(GameLogic.DoubleBullet logic, GridComponent grid)
        : base(logic, grid, StaticTexture)
    {
    }
}
