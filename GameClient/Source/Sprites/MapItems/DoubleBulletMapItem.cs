using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a double bullet map item.
/// </summary>
internal class DoubleBulletMapItem : SecondaryMapItem
{
    private static readonly ScalableTexture2D.Static Texture;
    private readonly ScalableTexture2D texture;

    /// <summary>
    /// Initializes static members of the <see cref="DoubleBulletMapItem"/> class.
    /// </summary>
    static DoubleBulletMapItem()
    {
        Texture = new ScalableTexture2D.Static("Images/Game/MapItems/double_bullet.svg");
        Texture.Load();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleBulletMapItem"/> class.
    /// </summary>
    /// <param name="logic">The map item logic.</param>
    /// <param name="grid">The grid component.</param>
    public DoubleBulletMapItem(GameLogic.SecondaryMapItem logic, GridComponent grid)
        : base(logic, grid)
    {
        this.texture = new ScalableTexture2D(Texture)
        {
            Color = MonoTanks.ThemeColor,
            Transform =
            {
                Type = TransformType.Absolute,
            },
        };

        this.UpdateDestination();
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        this.texture.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        this.texture.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void UpdateDestination()
    {
        base.UpdateDestination();

        Texture.Transform.Size = this.Size;
        this.texture.Transform.Size = this.Size;
        this.texture.Transform.Location = this.Location;
    }
}
