using System;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a secondary map item sprite.
/// </summary>
internal class SecondaryItem : ISprite, IDetectableByRadar
{
    private static readonly ScalableTexture2D.Static UnknownStaticTexture = new("Images/Game/MapItems/unknown.svg");
    private static readonly ScalableTexture2D.Static LaserStaticTexture = new("Images/Game/MapItems/laser.svg");
    private static readonly ScalableTexture2D.Static DoubleBulletStaticTexture = new("Images/Game/MapItems/double_bullet.svg");
    private static readonly ScalableTexture2D.Static RadarStaticTexture = new("Images/Game/MapItems/radar.svg");
    private static readonly ScalableTexture2D.Static MineStaticTexture = new("Images/Game/MapItems/mine.svg");

    private readonly ScalableTexture2D texture;
    private readonly GameLogic.SecondaryItem logic;
    private readonly GridComponent grid;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecondaryItem"/> class.
    /// </summary>
    /// <param name="logic">The secondary item logic.</param>
    /// <param name="grid">The grid component.</param>
    public SecondaryItem(GameLogic.SecondaryItem logic, GridComponent grid)
    {
        this.logic = logic;
        this.grid = grid;
        this.grid.DrawDataChanged += (s, e) => this.UpdateDestination();

        this.texture = new ScalableTexture2D(GetStaticTexture(logic.Type))
        {
            Color = GameClientCore.ThemeColor,
            Transform =
            {
                Type = TransformType.Absolute,
            },
        };

        this.UpdateDestination();
    }

    /// <inheritdoc/>
    float IDetectableByRadar.Opacity
    {
        get => this.texture.Opacity;
        set => this.texture.Opacity = value;
    }

    /// <inheritdoc/>
    public static void LoadContent()
    {
        UnknownStaticTexture.Load();
        LaserStaticTexture.Load();
        DoubleBulletStaticTexture.Load();
        RadarStaticTexture.Load();
        MineStaticTexture.Load();
    }

    /// <inheritdoc/>
    IDetectableByRadar IDetectableByRadar.Copy()
    {
        return new SecondaryItem(this.logic, this.grid);
    }

    /// <inheritdoc/>
    public void Update(GameTime gameTime)
    {
        this.texture.Update(gameTime);
    }

    /// <inheritdoc/>
    public void Draw(GameTime gameTime)
    {
        this.texture.Draw(gameTime);
    }

    private static ScalableTexture2D.Static GetStaticTexture(SecondaryItemType type)
    {
        return type switch
        {
            SecondaryItemType.Unknown => UnknownStaticTexture,
            SecondaryItemType.DoubleBullet => DoubleBulletStaticTexture,
            SecondaryItemType.Laser => LaserStaticTexture,
            SecondaryItemType.Radar => RadarStaticTexture,
            SecondaryItemType.Mine => MineStaticTexture,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    private static void UpdateStaticTextureSize(Point size)
    {
        UnknownStaticTexture.Transform.Size = size;
        LaserStaticTexture.Transform.Size = size;
        DoubleBulletStaticTexture.Transform.Size = size;
        RadarStaticTexture.Transform.Size = size;
        MineStaticTexture.Transform.Size = size;
    }

    private void UpdateDestination()
    {
        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        var size = new Point(tileSize, tileSize);
        var location = new Point(
             gridLeft + (this.logic.X * tileSize) + drawOffset,
             gridTop + (this.logic.Y * tileSize) + drawOffset);

        this.texture.Transform.Size = size;
        this.texture.Transform.Location = location;

        UpdateStaticTextureSize(size);
    }
}
