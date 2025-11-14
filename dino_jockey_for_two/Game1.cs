using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace dino_jockey_for_two
{
    public class Game1() : Core("Dino Jockey", GameConfig.ScreenWidth, GameConfig.ScreenHeight, GameConfig.FullScreen)
    {
        private enum AppState { Menu, Playing }
        private AppState _state = AppState.Menu;

        private GameSession _game1;
        private GameSession _game2;
        private readonly InputManager _inputManager = new InputManager();
        private SpriteFont _font;
        private MenuScreen _menu;
        private Point _lastWindowSize;

        protected override void LoadContent()
        {
            _font = Content.Load<SpriteFont>("fonts/DinoFont");
            _menu = new MenuScreen(GraphicsDevice, _font);
            _lastWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
            _menu.Resize(_lastWindowSize);
            _menu.StartRequested += OnStartRequested;
        }

        private void OnStartRequested()
        {
            // Load gameplay content lazily on start
            var floor = TextureAtlas.FromFile(Content, "images/floor-definition.xml");
            var dinoAtlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
            var halfHeight = Window.ClientBounds.Height / 2;

            _game1 = new GameSession(
                viewport: new Rectangle(0, 0, Window.ClientBounds.Width, halfHeight),
                floorSprite: floor.CreateSprite("floor"),
                dinoAtlas: dinoAtlas,
                jumpKey: GameConfig.Player1JumpKey,
                name: "Player 1",
                font: _font
            );

            _game2 = new GameSession(
                viewport: new Rectangle(0, halfHeight, Window.ClientBounds.Width, halfHeight),
                floorSprite: floor.CreateSprite("floor"),
                dinoAtlas: dinoAtlas,
                jumpKey: GameConfig.Player2JumpKey,
                name: "Player 2",
                font: _font
            );

            _state = AppState.Playing;
            _menu?.Dispose();
            _menu = null;
        }

        protected override void Update(GameTime gameTime)
        {
            if (_state == AppState.Menu)
            {
                // Handle resize
                var sizeNow = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
                if (sizeNow != _lastWindowSize)
                {
                    _lastWindowSize = sizeNow;
                    _menu?.Resize(sizeNow);
                }

                _menu?.Update(gameTime);
            }
            else // Playing
            {
                _inputManager.Keyboard.Update();

                var ended = _game1.IsOver || _game2.IsOver;

                if (!ended)
                {
                    _game1.Update(gameTime, _inputManager);
                    _game2.Update(gameTime, _inputManager);

                    if (_game1.IsReady && _game2.IsReady && !_game1.CanStart && !_game2.CanStart)
                    {
                        _game1.BeginCountdown(3.0);
                        _game2.BeginCountdown(3.0);
                    }
                }
                else
                {
                    if (_game1.IsOver && !_game2.IsOver)
                        _game2.Winner = true;
                    else if (_game2.IsOver && !_game1.IsOver)
                        _game1.Winner = true;

                    if (!_game1.RestartReady && _inputManager.Keyboard.WasKeyJustPressed(_game1.Player.JumpKey))
                        _game1.RestartReady = true;

                    if (!_game2.RestartReady && _inputManager.Keyboard.WasKeyJustPressed(_game2.Player.JumpKey))
                        _game2.RestartReady = true;

                    if (_game1.RestartReady && _game2.RestartReady)
                    {
                        _game1.ResetSession();
                        _game2.ResetSession();
                    }

                    _game1.Player.UpdateAnimationOnly(gameTime);
                    _game2.Player.UpdateAnimationOnly(gameTime);
                }
            }

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

            if (_state == AppState.Menu)
            {
                var viewport = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
                _menu?.Draw(SpriteBatch, viewport);
            }
            else
            {
                _game1.Draw(SpriteBatch);
                _game2.Draw(SpriteBatch);

                string victoryMessage = null;
                var targetViewport = Rectangle.Empty;

                if (_game1.Winner)
                {
                    victoryMessage = $"{_game1.Name} ha ganado";
                    targetViewport = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height / 2);
                }
                else if (_game2.Winner)
                {
                    victoryMessage = $"{_game2.Name} ha ganado";
                    targetViewport = new Rectangle(0, Window.ClientBounds.Height / 2, Window.ClientBounds.Width, Window.ClientBounds.Height / 2);
                }

                if (victoryMessage != null)
                {
                    var size = _font.MeasureString(victoryMessage);
                    var pos = new Vector2(
                        targetViewport.Center.X - size.X / 2,
                        targetViewport.Center.Y - size.Y / 2 - 40
                    );
                    SpriteBatch.DrawString(_font, victoryMessage, pos, Color.Black);
                }
            }

            SpriteBatch.End();
            base.Draw(gameTime);
        }

    }
}
