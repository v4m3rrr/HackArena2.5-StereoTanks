using GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient.Sprites;

internal class Tank : Sprite
{
    private Direction tankDirection;
    private Direction barrelDirection;

    private Texture2D tankTexture;
    private Texture2D barrelTexture;

    private Point position = ScreenController.CurrentSize / new Point(2);

    public Tank(Color color)
    {
        this.tankDirection = Direction.Up;
        this.barrelDirection = Direction.Up;

        this.tankTexture = ContentController.Content.Load<Texture2D>("Images/Tank");
        this.barrelTexture = ContentController.Content.Load<Texture2D>("Images/Barrel");

        var colors = new Color[64 * 64];
        this.tankTexture.GetData(colors);
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == Color.White)
            {
                colors[i] = color;
            }
        }
        this.tankTexture.SetData(colors);
    }

    public override void Update(GameTime gameTime)
    {
        if (KeyboardController.IsKeyHit(Keys.A))
        {
            this.tankDirection = KeyboardController.IsKeyDown(Keys.S)
                ? (Direction)(((int)this.tankDirection + 1) % 4)
                : (Direction)(((int)this.tankDirection - 1 + 4) % 4);
        }

        if (KeyboardController.IsKeyHit(Keys.D))
        {
            this.tankDirection = KeyboardController.IsKeyDown(Keys.S)
                ? (Direction)(((int)this.tankDirection - 1 + 4) % 4)
                : (Direction)(((int)this.tankDirection + 1) % 4);
        }

        if (KeyboardController.IsKeyHit(Keys.Q))
        {
            this.barrelDirection = (Direction)(((int)this.barrelDirection - 1 + 4) % 4);
        }

        if (KeyboardController.IsKeyHit(Keys.E))
        {
            this.barrelDirection = (Direction)(((int)this.barrelDirection + 1) % 4);
        }

        if (KeyboardController.IsKeyDown(Keys.W))
        {
            switch (this.tankDirection)
            {
                case Direction.Up:
                    this.position.Y -= 4;
                    break;
                case Direction.Left:
                    this.position.X -= 4;
                    break;
                case Direction.Down:
                    this.position.Y += 4;
                    break;
                case Direction.Right:
                    this.position.X += 4;
                    break;
            }
        }

        if (KeyboardController.IsKeyDown(Keys.S))
        {
            switch (this.tankDirection)
            {
                case Direction.Up:
                    this.position.Y += 3;
                    break;
                case Direction.Left:
                    this.position.X += 3;
                    break;
                case Direction.Down:
                    this.position.Y -= 3;
                    break;
                case Direction.Right:
                    this.position.X -= 3;
                    break;
            }
        }

        //this.TransformBarrel();
        //this.TransformTank();
    }

    public override void Draw(GameTime gameTime)
    {
        float rotation = default;
        switch (this.tankDirection)
        {
            case Direction.Up:
                rotation = 0;
                break;
            case Direction.Right:
                rotation = MathHelper.PiOver2;
                break;
            case Direction.Down:
                rotation = MathHelper.Pi;
                break;
            case Direction.Left:
                rotation = MathHelper.PiOver2 * 3;
                break;
        }
        SpriteBatchController.SpriteBatch.Draw(
            this.tankTexture,
            new Rectangle(this.position + new Point(40, 40), new Point(80, 80)), null, Color.White, rotation, new Vector2(32f, 32f), Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 1.0f);

        switch (this.barrelDirection)
        {
            case Direction.Up:
                rotation = 0;
                break;
            case Direction.Right:
                rotation = MathHelper.PiOver2;
                break;
            case Direction.Down:
                rotation = MathHelper.Pi;
                break;
            case Direction.Left:
                rotation = MathHelper.PiOver2 * 3;
                break;
        }

        SpriteBatchController.SpriteBatch.Draw(
            this.barrelTexture,
            new Rectangle(this.position + new Point(40, 40), new Point(80, 80)), null, Color.White, rotation, new Vector2(32f, 32f), Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 1.0f);

    }
}
