using Microsoft.Xna.Framework;

namespace GameClient;

/// <summary>
/// Represents a sprite.
/// </summary>
internal interface ISprite
{
    /// <summary>
    /// Loads the content of the sprite.
    /// </summary>
    static virtual void LoadContent()
    {
    }

    /// <summary>
    /// Updates the sprite.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Draws the sprite.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    void Draw(GameTime gameTime);
}
