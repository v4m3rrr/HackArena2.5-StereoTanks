using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents the main effect.
/// </summary>
internal static class MainEffect
{
    private static readonly Texture2D Texture;

    static MainEffect()
    {
        var content = ContentController.Content;
        Texture = content.Load<Texture2D>("Images/main_effect");
    }

    /// <summary>
    /// Gets or sets the rotation of the main effect.
    /// </summary>
    public static float Rotation { get; set; }

    /// <summary>
    /// Draws the main effect.
    /// </summary>
    public static void Draw()
    {
        var spriteBatch = SpriteBatchController.SpriteBatch;

        spriteBatch.Draw(
            texture: Texture,
            position: ScreenController.CurrentSize.ToVector2() / 2f,
            sourceRectangle: null,
            color: GameClientCore.ThemeColor,
            rotation: Rotation,
            origin: Texture.Bounds.Size.ToVector2() / 2f,
            scale: 2.0f * ScreenController.Scale,
            effects: SpriteEffects.None,
            layerDepth: 0.1f);
    }
}
