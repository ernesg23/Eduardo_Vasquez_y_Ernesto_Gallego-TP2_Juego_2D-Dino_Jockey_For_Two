using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Collider;
using MonoGameLibrary.Graphics;

namespace dino_jockey_for_two;

public class Obstacle
{
    public Vector2 Position { get; private set; }
    public Box Collider { get; private set; }
    private readonly Sprite _sprite;
    private readonly float _speed;
    private float _viewportWidth;

    public bool ShouldRemove => Position.X < -_sprite.Width;

    public Obstacle(Sprite sprite, Vector2 startPosition, float speed, float viewportWidth)
    {
        _sprite = sprite;
        Position = startPosition;
        _speed = speed;
        _viewportWidth = viewportWidth;
        
        Collider = new Box(
            Vector2.Zero,
            (int)(_sprite.Width * GameConfig.ColliderWidthRatio),
            (int)(_sprite.Height * GameConfig.ColliderHeightRatio)
        );
        Collider.MoveCentered(Position);
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Position = new Vector2(Position.X - (_speed * deltaTime * 60), Position.Y);
        Collider.MoveCentered(Position);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, Position);
    }
}