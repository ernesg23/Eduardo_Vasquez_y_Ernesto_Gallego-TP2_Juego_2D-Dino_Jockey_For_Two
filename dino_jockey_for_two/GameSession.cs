using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

public class GameSession
{
    private Rectangle _viewport;
    private Sprite _floorSprite;
    private float _floorY;
    public string Name;
    public int Score = 0;
    private Player _player;

    public GameSession(
        Rectangle viewport,
        Sprite floorSprite,
        TextureAtlas dinoAtlas,
        Keys jumpKey,
        string name
    )
    {
        Name = name;
        _viewport = viewport;
        _floorSprite = floorSprite;
        _floorSprite.LayerDepth = 0f;
        _floorSprite.Scale = new Vector2(1f, 1f);
        _floorY = _viewport.Height - _floorSprite.Height;
        _player = new Player(dinoAtlas, _floorY, jumpKey);
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        if (_player.StartAnim && _player.Position.X < _viewport.Width * 0.5f)
            _player.Position.X += 5f;
        else
        {
            _player.Position.X = _viewport.Width * 0.5f;
            _player.StartAnim = false;
        }

        _player.Update(gameTime, inputManager);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _floorSprite.Draw(spriteBatch, new Vector2(0, _floorY));
        _player.Draw(spriteBatch);

        Texture2D debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        debugTexture.SetData(new[] { Color.White });

        spriteBatch.Draw(debugTexture, _player.Collider.Bounds, Color.Red * 0.5f);
    }
}
