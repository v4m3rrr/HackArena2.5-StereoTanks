using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;
using SkiaSharp;
using Svg.Skia;

namespace GameClient;

/// <summary>
/// Represents a scalable 2D texture.
/// </summary>
/// <remarks>
/// This class is used to load SVG assets
/// and scale them to fit the size of the component.
/// </remarks>
internal class ScalableTexture2D : TextureComponent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScalableTexture2D"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor sets the <see cref="AssetPath"/> property to an empty string.
    /// </remarks>
    public ScalableTexture2D()
    {
        this.Transform.SizeChanged += (s, e) => this.Reload();
        this.AssetPath = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalableTexture2D"/> class.
    /// </summary>
    /// <param name="assetPath">The path to the SVG asset that will be loaded as a texture.</param>
    public ScalableTexture2D(string assetPath)
        : this()
    {
        this.AssetPath = assetPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalableTexture2D"/> class.
    /// </summary>
    /// <param name="texture">
    /// The static texture that will be used to create a new scalable texture.
    /// </param>
    /// <remarks>
    /// This constructor also sets the <see cref="Transform.Ratio"/>
    /// to the ratio of the image.
    /// </remarks>
    public ScalableTexture2D(Static texture)
    {
        this.AssetPath = texture.AssetPath;
        this.Texture = texture.Texture;
        this.IsLoaded = true;
        this.Transform.Ratio = texture.Transform.Ratio;
        texture.TextureChanged += (s, e) => this.Texture = texture.Texture;
    }

    /// <summary>
    /// Gets or sets the path to the SVG asset that will be loaded as a texture.
    /// </summary>
    public string AssetPath { get; set; }

    /// <inheritdoc/>
    public override void Load()
    {
        this.Load();
    }

    /// <summary>
    /// Loads the texture from the SVG asset.
    /// </summary>
    /// <param name="overrideRatio">
    /// A value indicating whether <see cref="Transform.Ratio"/>
    /// should be overriden by the SVG ratio.
    /// </param>
    public virtual void Load(bool overrideRatio = true)
    {
        var content = ContentController.Content;
        var svg = new SKSvg();
        _ = svg.Load(PathUtils.GetAbsolutePath(content.RootDirectory + "/" + this.AssetPath));
        int width = this.Transform.Size.X;
        int height = this.Transform.Size.Y;

        using var skBitMap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(skBitMap);
        canvas.Clear(SKColors.Transparent);

        var svgSize = svg.Picture!.CullRect;
        float scaleX = width / svgSize.Width;
        float scaleY = height / svgSize.Height;

        if (overrideRatio)
        {
            this.Transform.Ratio = (svgSize.Width / svgSize.Height).ToRatio();
        }

        if (this.Transform.Ratio == new Ratio(1, 1))
        {
            scaleX = scaleY = Math.Min(scaleX, scaleY);
        }

        float translateX = (width - (svgSize.Width * scaleX)) / 2;
        float translateY = (height - (svgSize.Height * scaleY)) / 2;

        canvas.Translate(translateX, translateY);
        canvas.Scale(scaleX, scaleY);

        canvas.DrawPicture(svg.Picture);

        using var image = skBitMap.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = image.AsStream();
        this.Texture = Texture2D.FromStream(ScreenController.GraphicsDevice, stream);

        this.IsLoaded = true;
    }

    private void Reload()
    {
        if (this.IsLoaded)
        {
            this.IsLoaded = false;
            this.Texture?.Dispose();
            this.Load();
        }
    }

    /// <summary>
    /// Represents a static texture that can be used to create a new scalable texture.
    /// </summary>
    /// <param name="assetPath">
    /// The path to the SVG asset that will be loaded as a texture.
    /// </param>
    public class Static(string assetPath) : ScalableTexture2D(assetPath)
    {
        /// <summary>
        /// Occurs when the texture is changed.
        /// </summary>
        public event EventHandler? TextureChanged;

        /// <inheritdoc/>
        public override void Load(bool overrideRatio = true)
        {
            base.Load(overrideRatio);
            this.TextureChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
