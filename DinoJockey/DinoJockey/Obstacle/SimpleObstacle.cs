using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;

namespace DinoJockey.Obstacle;

public class SimpleObstacle
{
    public AnimatedSprite Sprite;
    public Vector2 Position;
    public float VelocityX { get; private set; }
    private ObstacleType _type;

    public Rectangle Bounds => new Rectangle(
        (int)(Position.X - Sprite.Width / 2),
        (int)(Position.Y - Sprite.Height / 2),
        (int)Sprite.Width,
        (int)Sprite.Height
    );

    public Vector2 Center => new Vector2(
        Position.X + Sprite.Width / 2f,
        Position.Y + Sprite.Height / 2f
    );

    public SimpleObstacle(AnimatedSprite sprite, ObstacleType type, Vector2 position, float velocityX)
    {
        _type = type;
        Sprite = sprite;
        Sprite.CenterOrigin();
        Sprite.Scale = Vector2.One * 1.2f;
        Position = position;
        VelocityX = velocityX;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Sprite.Draw(spriteBatch, Position);
    }

    // Método para dibujar los bounds (rectángulo de colisión)
    public void DrawBounds(SpriteBatch spriteBatch, Texture2D pixelTexture, Color color)
    {
        if (pixelTexture != null)
        {
            spriteBatch.Draw(
                pixelTexture,
                Bounds,
                color * 0.5f // Semi-transparente para ver el sprite detrás
            );
        }
    }

    public void DrawBoundsOutline(SpriteBatch spriteBatch, Texture2D pixelTexture, Color color, int thickness = 2)
    {
        if (pixelTexture != null)
        {
            // Dibujar líneas para el contorno
            // Línea superior
            spriteBatch.Draw(pixelTexture,
                new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, thickness),
                color);

            // Línea inferior
            spriteBatch.Draw(pixelTexture,
                new Rectangle(Bounds.Left, Bounds.Bottom - thickness, Bounds.Width, thickness),
                color);

            // Línea izquierda
            spriteBatch.Draw(pixelTexture,
                new Rectangle(Bounds.Left, Bounds.Top, thickness, Bounds.Height),
                color);

            // Línea derecha
            spriteBatch.Draw(pixelTexture,
                new Rectangle(Bounds.Right - thickness, Bounds.Top, thickness, Bounds.Height),
                color);
        }
    }

    public void DrawDebugInfo(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
    {
        if (font != null && pixelTexture != null)
        {
            DrawBoundsOutline(spriteBatch, pixelTexture, Color.Red);

            string debugText = $"X: {(int)Position.X}\nY: {(int)Position.Y}";
            Vector2 textSize = font.MeasureString(debugText);
            Vector2 textPosition = new Vector2(
                Position.X + Sprite.Width / 2 - textSize.X / 2,
                Position.Y - textSize.Y - 5
            );

            spriteBatch.Draw(pixelTexture,
                new Rectangle((int)textPosition.X, (int)textPosition.Y, (int)textSize.X, (int)textSize.Y),
                Color.Black * 0.7f);

            spriteBatch.DrawString(font, debugText, textPosition, Color.White);
        }
    }

    public void Update(GameTime gameTime)
    {
        Sprite.Update(gameTime);
        checkType(gameTime);
    }

    private void checkType(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_type == ObstacleType.Simple)
        {
            Position.X -= VelocityX * deltaTime;
        }
    }
}
