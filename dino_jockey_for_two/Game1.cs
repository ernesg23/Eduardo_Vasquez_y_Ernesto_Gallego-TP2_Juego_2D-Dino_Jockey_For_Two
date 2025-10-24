using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace dino_jockey_for_two;

public class Game1 : Core
{
    private enum GameState { Menu, Playing }
    private GameState _currentState = GameState.Menu;

    private GameSession _game1;
    private GameSession _game2;
    private InputManager _inputManager = new InputManager();

    // Recursos UI
    private Texture2D _titleTexture;
    private Texture2D _buttonTexture;
    private Rectangle _btnStart;
    private Rectangle _btnExit;

    public Game1() : base("Dino Jockey", GameConfig.ScreenWidth, GameConfig.ScreenHeight, GameConfig.FullScreen) { }

    protected override void LoadContent()
    {
        // Cargar recursos del menú
        _titleTexture = Content.Load<Texture2D>("images/dinotitle.png");   // PNG del título
        _buttonTexture = Content.Load<Texture2D>("images/button-for-8-bit-games-start-beginning-vector.png"); // textura simple de botón

        int screenWidth = Window.ClientBounds.Width;
        int screenHeight = Window.ClientBounds.Height;

        _btnStart = new Rectangle(screenWidth / 2 - 100, screenHeight / 2, 200, 50);
        _btnExit = new Rectangle(screenWidth / 2 - 100, screenHeight / 2 + 70, 200, 50);

        // Recursos del juego
        var floor = TextureAtlas.FromFile(Content, "images/floor-definition.xml");
        var dinoAtlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
        int halfHeight = Window.ClientBounds.Height / 2;

        _game1 = new GameSession(
            viewport: new Rectangle(0, 0, Window.ClientBounds.Width, halfHeight),
            floorSprite: floor.CreateSprite("floor"),
            dinoAtlas: dinoAtlas,
            jumpKey: GameConfig.Player1JumpKey,
            name: "Player 1"
        );

        _game2 = new GameSession(
            viewport: new Rectangle(0, halfHeight, Window.ClientBounds.Width, halfHeight),
            floorSprite: floor.CreateSprite("floor"),
            dinoAtlas: dinoAtlas,
            jumpKey: GameConfig.Player2JumpKey,
            name: "Player 2"
        );
    }

    protected override void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        if (_currentState == GameState.Menu)
        {
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (_btnStart.Contains(mouse.Position))
                {
                    _currentState = GameState.Playing;
                }
                else if (_btnExit.Contains(mouse.Position))
                {
                    Exit();
                }
            }
        }
        else if (_currentState == GameState.Playing)
        {
            if (keyboard.IsKeyDown(Keys.Escape))
                _currentState = GameState.Menu;

            _inputManager.Update(gameTime);
            _game1.Update(gameTime, _inputManager);
            _game2.Update(gameTime, _inputManager);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        if (_currentState == GameState.Menu)
        {
            // Dibujar título
            SpriteBatch.Draw(_titleTexture, new Vector2(200, 50), Color.White);

            // Botón Iniciar
            SpriteBatch.Draw(_buttonTexture, _btnStart, Color.White);
/* 
            // Botón Salir
            SpriteBatch.Draw(_buttonTexture, _btnExit, Color.White);
            SpriteBatch.DrawString(_font, "Salir", new Vector2(_btnExit.X + 70, _btnExit.Y + 15), Color.Black); */
        }
        else if (_currentState == GameState.Playing)
        {
            _game1.Draw(SpriteBatch);
            _game2.Draw(SpriteBatch);
        }

        SpriteBatch.End();
        base.Draw(gameTime);
    }
}
