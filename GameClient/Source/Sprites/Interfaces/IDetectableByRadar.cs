using Microsoft.Xna.Framework;

namespace GameClient.Sprites;

/// <summary>
/// Represents a sprite that can be detected by the radar.
/// </summary>
internal interface IDetectableByRadar
{
    /// <summary>
    /// Gets or sets the opacity of the sprite.
    /// </summary>
    /// <value>
    /// The opacity of the sprite between 0 and 1.
    /// </value>
    float Opacity { get; set; }

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
