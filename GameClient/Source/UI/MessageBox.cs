using System;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI;

/// <summary>
/// Represents a message box.
/// </summary>
/// <typeparam name="T">The type of the content of the message box.</typeparam>
/// <remarks>
/// The type <typeparamref name="T"/> must implement the <see cref="IButtonContent{T}"/> interface,
/// because the message box is displayed as an overlay and the content must allow check,
/// if the mouse is hovering over it.
/// </remarks>
internal class MessageBox<T> : Component, IOverlayComponent, IStyleable<MessageBox<T>>
    where T : Component
{
    private bool showedInThisTick;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBox{T}"/> class.
    /// </summary>
    /// <param name="box">The content of the message box.</param>
    public MessageBox(T box)
    {
        this.Parent = null;
        this.Transform.Type = TransformType.Relative;

        this.Box = box;
        this.Box.Parent = this;

        ScreenController.ScreenChanged += this.ScreenController_ScreenChanged;
    }

    /// <summary>
    /// Gets the content of the message box.
    /// </summary>
    public T Box { get; }

    /// <inheritdoc/>
    public IComponent BaseComponent => this.Box;

    /// <summary>
    /// Gets the background of the message box.
    /// </summary>
    /// <remarks>
    /// The background should not have a <see cref="Component.Parent"/>.
    /// </remarks>
    public SolidColor? Background { get; protected init; }

    /// <summary>
    /// Gets or sets a value indicating whether the message
    /// box can be closed by clicking outside of it.
    /// </summary>
    public bool CanBeClosedByClickOutside { get; set; } = true;

    /// <inheritdoc/>
    public int Priority { get; set; } = 1 << 30;

    /// <inheritdoc/>
    public void OnShow()
    {
        this.showedInThisTick = true;
        ScreenController.ScreenChanged += this.ScreenController_ScreenChanged;

        this.Box.Transform.ForceRecalulcation();
        this.Background?.Transform.ForceRecalulcation();
    }

    /// <inheritdoc/>
    public void OnHide()
    {
        ScreenController.ScreenChanged -= this.ScreenController_ScreenChanged;
    }

    /// <inheritdoc/>
    public void ApplyStyle(Style<MessageBox<T>> style)
    {
        style.Apply(this);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.Background?.Draw(gameTime);

        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.Background?.Update(gameTime);

        if (this.CanBeClosedByClickOutside
            && !this.showedInThisTick
            && MouseController.IsLeftButtonClicked())
        {
            var clickedOutsite = this.Box is IButtonContent<T> bc ?
                !bc.IsButtonContentHovered(MouseController.Position)
                : !this.Box.Transform.DestRectangle.Contains(MouseController.Position);

            if (clickedOutsite)
            {
                ScreenController.HideOverlay(this);
            }
        }

        base.Update(gameTime);

        this.showedInThisTick = false;
    }

    private void ScreenController_ScreenChanged(object? sender, EventArgs e)
    {
        this.Transform.ForceRecalulcation();
    }
}
