using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.UI;

/// <summary>
/// Represents a circle texture component.
/// </summary>
internal class Circle : TextureComponent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> class.
    /// </summary>
    /// <param name="thickness">The thickness of the circle.</param>
    public Circle(int thickness)
    {
        this.Thickness = thickness;
        this.Transform.SizeChanged += (s, e) => this.LoadTexture();
    }

    /// <summary>
    /// Gets the radius of the circle.
    /// </summary>
    public int Radius => Math.Min(this.Transform.Size.X, this.Transform.Size.Y) / 2;

    /// <summary>
    /// Gets the radius of the circle as a float.
    /// </summary>
    public float RadiusF => Math.Min(this.Transform.Size.X, this.Transform.Size.Y) / 2f;

    /// <summary>
    /// Gets the thickness of the circle.
    /// </summary>
    public int Thickness { get; }

    /// <inheritdoc/>
    protected override void LoadTexture()
    {
        this.Texture?.Dispose();

        var radius = this.Radius;
        var diameter = radius * 2;

        var opacities = new float[diameter * diameter];
        this.FillCircleOutline(opacities);

        var texture = new Texture2D(ScreenController.GraphicsDevice, diameter, diameter);
        var data = new Color[diameter * diameter];

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Color.White * opacities[i];
        }

        texture.SetData(data);
        this.Texture = texture;
    }

    private void FillCircleOutline(float[] opacities)
    {
        var radius = this.Radius;
        var radiusF = this.RadiusF;
        var diameter = radius * 2;

        for (int x = 0; x < diameter; x++)
        {
            for (int y = 0; y < diameter; y++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), new Vector2(radiusF, radiusF));
                distance -= 0.25f; // Make the circle a bit smaller to avoid aliasing
                float opacity = 1f;

                if (distance <= radius && distance >= radius - this.Thickness)
                {
                    if (distance >= radius - 1)
                    {
                        opacity = 1f - (distance - (radius - 1f));
                    }
                    else if (distance <= radius - this.Thickness + 1f)
                    {
                        opacity = distance + this.Thickness - radius;
                    }
                }
                else
                {
                   opacity = 0f;
                }

                opacities[x + (y * diameter)] = opacity;
            }
        }
    }
}
