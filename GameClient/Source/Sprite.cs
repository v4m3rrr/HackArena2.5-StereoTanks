using Microsoft.Xna.Framework;

namespace GameClient;

/// <summary>
/// Represents a sprite.
/// </summary>
internal abstract class Sprite
{
    /// <summary>
    /// Updates the sprite.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public abstract void Update(GameTime gameTime);

    /// <summary>
    /// Draws the sprite.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public abstract void Draw(GameTime gameTime);
}
