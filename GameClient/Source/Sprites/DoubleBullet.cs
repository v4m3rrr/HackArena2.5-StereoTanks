//using Microsoft.Xna.Framework;

//namespace GameClient.Sprites;

///// <summary>
///// Represents a double bullet sprite.
///// </summary>
///// <param name="logic">The double bullet logic.</param>
///// <param name="grid">The grid component.</param>
//internal class DoubleBullet(GameLogic.Bullet logic, GridComponent grid)
//    : Bullet(logic, grid, StaticTexture), ISprite
//{
    

//    /// <inheritdoc/>
//    public static new void LoadContent()
//    {
//        StaticTexture.Load();
//    }

//    /// <inheritdoc/>
//    public override void Update(GameTime gameTime)
//    {
//        StaticTexture.Transform.Size = new Point(this.Grid.TileSize);
//        base.Update(gameTime);
//    }
//}
