using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a wall sprite.
/// </summary>
internal class Wall : Sprite
{
    private static readonly Dictionary<string, Texture2D> Textures = new();
    private static Vector2 origin;

    private readonly GridComponent grid;
    private float rotation;
    private Rectangle destination;
    private string textureName = default!;
    private bool isUpdateNeeded = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Wall"/> class.
    /// </summary>
    /// <param name="logic">The wall logic.</param>
    /// <param name="grid">The grid component.</param>
    public Wall(GameLogic.Wall logic, GridComponent grid)
    {
        this.grid = grid;
        this.Logic = logic;

        this.grid.DrawDataChanged += (s, e) => this.UpdateDestination();
    }

    /// <summary>
    /// Gets the wall logic.
    /// </summary>
    public GameLogic.Wall Logic { get; private set; }

    /// <summary>
    /// Updates the wall logic.
    /// </summary>
    /// <param name="logic">The new wall logic.</param>
    public void UpdateLogic(GameLogic.Wall logic)
    {
        this.Logic = logic;
        this.isUpdateNeeded = true;
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.isUpdateNeeded)
        {
            this.UpdateTextureName();
            this.UpdateTextureRotation();
            this.UpdateDestination();
            this.isUpdateNeeded = false;
        }
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        Texture2D texture = Textures[this.textureName];
        SpriteBatchController.SpriteBatch.Draw(
            texture,
            destinationRectangle: this.destination,
            sourceRectangle: null,
            color: Color.White,
            rotation: this.rotation,
            origin: origin,
            effects: SpriteEffects.None,
            layerDepth: 1.0f);
    }

    private void UpdateTextureName()
    {
        this.textureName = this.GetTextureName();
        if (!Textures.ContainsKey(this.textureName))
        {
            var texture = ContentController.Content.Load<Texture2D>("Images/" + this.textureName);
            Textures.Add(this.textureName, texture);
            origin = texture.Bounds.Size.ToVector2() / 2f;
        }
    }

    private void UpdateTextureRotation()
    {
        this.rotation = this.GetRotation();
    }

    private void UpdateDestination()
    {
        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        this.destination = new Rectangle(
            gridLeft + (this.Logic.X * tileSize) + drawOffset,
            gridTop + (this.Logic.Y * tileSize) + drawOffset,
            tileSize,
            tileSize);
    }

    private string GetTextureName()
    {
        GameLogic.Wall?[] neighbors = this.grid.Logic.GetWallNeighbors(this.Logic);

        int nullCount = neighbors.Count(x => x is null);
        bool[] isNull = neighbors.Select(x => x is null).ToArray();

        return nullCount switch
        {
            0 => "Wall",
            1 => "WallU",
            2 when isNull[0] == isNull[2] => "WallUD",
            2 => "WallUR",
            3 => "WallURD",
            4 => "WallURDL",
            _ => "Wall",
        };
    }

    private float GetRotation()
    {
        GameLogic.Wall?[] neighbors = this.grid.Logic.GetWallNeighbors(this.Logic);

        int nullCount = neighbors.Count(x => x is null);
        bool[] isNull = neighbors.Select(x => x is null).ToArray();

        return nullCount switch
        {
            0 => 0,
            1 when isNull[0] => MathF.PI * 3 / 2,
            1 when isNull[1] => 0,
            1 when isNull[2] => MathF.PI / 2,
            1 => MathF.PI,
            2 when isNull[0] && isNull[2] => MathF.PI / 2,
            2 when isNull[1] && isNull[3] => 0,
            2 when isNull[0] && isNull[1] => MathF.PI * 3 / 2,
            2 when isNull[1] && isNull[2] => 0,
            2 when isNull[2] && isNull[3] => MathF.PI / 2,
            2 => MathF.PI,
            3 when !isNull[0] => 0,
            3 when !isNull[1] => MathF.PI / 2,
            3 when !isNull[2] => MathF.PI,
            3 => MathF.PI * 3 / 2,
            4 => 0,
            _ => 0,
        };
    }
}
