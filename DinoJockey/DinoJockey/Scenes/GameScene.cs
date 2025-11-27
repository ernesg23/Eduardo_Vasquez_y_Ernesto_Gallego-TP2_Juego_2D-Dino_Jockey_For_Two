using DinoJockey.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Input;
using MonoGameLibrary.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGameGum;

namespace DinoJockey.Scenes;

public class GameScene: Scene
{
    private float _timer = 0;
    private TextureAtlas _atlas;
    private Texture2D _floor;
    private Texture2D _background;
    private GameInstance _game1;
    private GameInstance _game2;
    private SpriteFont _font;
    private Texture2D _pixelTexture;
    private Effect _effect;
    private Song _song;
    private SoundEffect _jump;
    private SoundEffect _push;
    private bool _musicWasPlayed = false;
    public override void Initialize()
    {
        base.Initialize();
        LoadContent();
    }
    public override void LoadContent()
    {
        // En juego ocultamos el mouse
        Core.Instance.IsMouseVisible = false;
        int halfHeigthScreen = Core.GraphicsDevice.Viewport.Height / 2;
        int widthScreen = Core.GraphicsDevice.Viewport.Width;
        _atlas = TextureAtlas.FromFile(Content, "images/full-atlas.xml");
        _floor = Core.Content.Load<Texture2D>("images/floor-pattern");
        _song = Core.Content.Load<Song>("audio/music");
        _background = Core.Content.Load<Texture2D>("images/background-pattern-black");
        _font = Core.Content.Load<SpriteFont>("fonts/mainFont");
        _effect = Core.Content.Load<Effect>("effects/ColorReplace");
        _pixelTexture = new Texture2D(Core.GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        _push = Core.Content.Load<SoundEffect>("audio/push");
        _jump = Core.Content.Load<SoundEffect>("audio/jump");

        _game1 = new GameInstance(
            Core.Audio,
            _push,
            _jump,
            _effect.Clone(),
            new Vector4(0.0f, 0.224f, 0.035f, 1f),
            _pixelTexture,
            _atlas,
            _floor,
            _background,
            _font,
            "Jugador 1", 
            Vector2.Zero,
            new Rectangle(0, 0, widthScreen, (int)(halfHeigthScreen * 0.98f) ),
            GameSettings.Player1.KeyboardKey,
            GameSettings.Player1.GamePadIndex,
            GameSettings.Player1.GamePadButton
        );
        _game2 = new GameInstance(
            Core.Audio,
            _push,
            _jump,
            _effect.Clone(),
            new Vector4(0.224f, 0.0f, 0.200f, 1f),
            _pixelTexture,
            _atlas,
            _floor,
            _background,
            _font,
            "Jugador 2",
            new Vector2( 0, halfHeigthScreen + halfHeigthScreen * 0.02f ),
            new Rectangle(
                0, 
                halfHeigthScreen + (int)(halfHeigthScreen * 0.02f), 
                widthScreen, 
                (int)(halfHeigthScreen * 0.98f)
            ),
            GameSettings.Player2.KeyboardKey,
            GameSettings.Player2.GamePadIndex,
            GameSettings.Player2.GamePadButton
        );
    }
    public override void Update(GameTime gameTime)
    {
        _game1.Update(gameTime, Core.Input.Keyboard, Core.Input.GamePads[(int)GameSettings.Player1.GamePadIndex]);
        _game2.Update(gameTime, Core.Input.Keyboard, Core.Input.GamePads[(int)GameSettings.Player2.GamePadIndex]);
        CheckReady(gameTime);
        CheckPlaying(gameTime);
        checkSong();
    }
    private void checkSong()
    {
        if(_game1.Status == GameStatus.Starting && _game2.Status == GameStatus.Starting && !_musicWasPlayed)
        {
            Core.Audio.PlaySong(_song);
            _musicWasPlayed = true;
        }
    }
    private void CheckReady(GameTime gameTime)
    {
        if(_game1.Status == GameStatus.ReadyToStart && _game2.Status == GameStatus.ReadyToStart)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(_timer >= 1f)
            {
                _game1.Start();
                _game2.Start();
                _timer = 0;
            }
        }
    }
    private void CheckPlaying(GameTime gameTime)
    {
        if (_game1.Status == GameStatus.GameOver || _game2.Status == GameStatus.GameOver)
        {
            Core.Audio.PauseAudio();
            string winner;
            if (_game1.Status == GameStatus.GameOver && _game2.Status != GameStatus.GameOver)
            {
                winner = $"Ganador: {_game2.Name}";
                _game2.Win();
            }
            else if (_game1.Status != GameStatus.GameOver && _game2.Status == GameStatus.GameOver)
            {
                winner = $"Ganador: {_game1.Name}";
                _game1.Win();
            }
            else if (_game1.Score > _game2.Score)
            {
                winner = $"Ganador: {_game1.Name} (puntaje)";
                _game1.Win();
            }
            else if (_game1.Score < _game2.Score)
            {
                winner = $"Ganador: {_game2.Name} (puntaje)";
                _game2.Win();
            }
            else
                winner = "Empate";
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if( _timer >= 2f)
            {
                UnloadContent();
                Core.ChangeScene(new GameOverScene(winner));
            }
        }
    }
    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.GraphicsDevice.Clear(Color.Black);
        _game1.Draw(Core.SpriteBatch, gameTime);
        _game2.Draw(Core.SpriteBatch, gameTime);
    }
}