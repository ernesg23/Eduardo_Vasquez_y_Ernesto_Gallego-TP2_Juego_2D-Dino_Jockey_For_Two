using DinoJockey.Obstacle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using System;
using System.Collections.Generic;

namespace DinoJockey.Game;

public class GameInstance
{
    private Texture2D _floor;
    private Texture2D _background;
    private SpriteFont _font;
    public string Name;
    private Vector2 _position;
    private Rectangle _bounds;
    private Keys _key;
    private float _scoreTimer;

    private List<SimpleObstacle> simpleObstacles = new List<SimpleObstacle>();
    private float _floorScrollX = 0;
    private float _backgroundScrollX = 0;
    private int _count = 3;
    private float _timer = 0;
    private float _obstacleTimer = 0;
    private float _floorPos => _bounds.Bottom - _floor.Height + 1;
    private float _centerX => _bounds.Width / 2;
    public int Score { get; private set; } = 0;
    private const float BaseObstacleSpeed = 200f;
    private const float SpeedGrowthFactor = 0.05f; // Ajusta este valor
    private const float BackgroundSpeedFactor = 0.3f; // más lento que el suelo
    private Random _random = new Random();
    private float _nextObstacleTime = 2f; // tiempo objetivo para el próximo obstáculo
    public GameStatus Status { get; private set; } = GameStatus.Waiting;
    private Player _player;
    private TextureAtlas _atlas;
    private Texture2D _pixel;
    private Effect _effect;
    private AudioController _audio;
    private SoundEffect _push;
    private bool _countSound = true;
    private SoundEffect _jump;

    public GameInstance(
        AudioController audio,
        SoundEffect push,
        SoundEffect jump,
        Effect effect,
        Vector4 color,
        Texture2D pixel,
        TextureAtlas atlas,
        Texture2D floor,
        Texture2D background,
        SpriteFont font,
        string name,
        Vector2 position,
        Rectangle bounds,
        Keys key
    )
    {
        _audio = audio;
        _push = push;
        _jump = jump;
        _effect = effect;
        _effect.Parameters["TargetColor"].SetValue(new Vector4(0.325f, 0.325f, 0.325f, 1f));
        _effect.Parameters["Tolerance"].SetValue(0.1f);
        _effect.Parameters["NewColor"].SetValue(color);

        _pixel = pixel;
        _atlas = atlas;
        _floor = floor;
        _background = background;
        _font = font;
        Name = name;
        _position = position;
        _key = key;
        _bounds = bounds;
        _player = new Player(audio, jump, push, atlas, new Vector2(_bounds.Left, _floorPos), _key);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointWrap);

        DrawBackground(spriteBatch, gameTime);
        DrawFloor(spriteBatch, gameTime);

        foreach (var obstacle in simpleObstacles)
            obstacle.Draw(spriteBatch);

        if (Status != GameStatus.Playing)
            DrawPrompt(spriteBatch, gameTime);

        DrawScore(spriteBatch);

        spriteBatch.End();
        spriteBatch.Begin(samplerState: SamplerState.PointWrap, effect: _effect);
        _player.Draw(spriteBatch);
        spriteBatch.End();
    }

    private void DrawBackground(SpriteBatch spriteBatch, GameTime gameTime)
    {
        float backgroundScaleX = (float)_bounds.Width / _background.Width;
        float backgroundScaleY = (float)_bounds.Height / _background.Height;

        spriteBatch.Draw(
            _background,
            _bounds,
            new Rectangle(
                (int)_backgroundScrollX,
                0,
                (int)(_background.Width * backgroundScaleX),
                (int)(_background.Height * backgroundScaleY)
            ),
            Color.White
        );
    }

    private void DrawFloor(SpriteBatch spriteBatch, GameTime gameTime)
    {
        float floorScaleX = (float)_bounds.Width / _floor.Width;
        spriteBatch.Draw(
            _floor,
            new Vector2(_bounds.Left, _floorPos),
            new Rectangle((int)_floorScrollX, 0, (int)(_floor.Width * floorScaleX), _floor.Height),
            Color.White
        );
    }

    private void DrawPrompt(SpriteBatch spriteBatch, GameTime gameTime)
    {
        string text;
        if (Status == GameStatus.Waiting)
            text = $"Presione {_key} para jugar.";
        else if (Status == GameStatus.ReadyToStart)
            text = $"Listo para empezar";
        else if (Status == GameStatus.Counting)
            text = $"{_count}";
        else
            return;

        Vector2 size = _font.MeasureString(text);
        spriteBatch.DrawString(
            _font,
            text,
            new Vector2(
                _bounds.Center.X - size.X / 2, // Mejor posicionamiento
                _bounds.Center.Y - size.Y / 2
            ),
            Color.White
        );
    }

    private void DrawScore(SpriteBatch spriteBatch)
    {
        string text = $"Puntaje: {Score}";
        Vector2 size = _font.MeasureString(text);
        spriteBatch.DrawString(
            _font,
            text,
            new Vector2(_bounds.Right - size.X -10, _bounds.Top + 10), // Posición fija en esquina superior izquierda
            Color.White
        );
    }

    public void Update(GameTime gameTime, KeyboardInfo keyboard)
    {
        _player.Update(gameTime, keyboard);

        if (Status == GameStatus.Waiting)
            checkWaiting(gameTime, keyboard);
        else if (Status == GameStatus.Counting)
            checkCounting(gameTime);
        else if (Status == GameStatus.Starting)
            CheckStarting();
        else if (Status == GameStatus.Playing) // Cambiar la condición
        {
            CheckPlaying(gameTime);
            UpdateObstacles(gameTime);
        }
    }

    private void checkWaiting(GameTime gameTime, KeyboardInfo keyboard)
    {
        if (keyboard.WasKeyJustPressed(_key))
        {
            _audio.PlaySoundEffect(_jump);
            Status = GameStatus.ReadyToStart;
        }
    }

    private void checkCounting(GameTime gameTime)
    {
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_countSound)
        {
            _audio.PlaySoundEffect(_push);
            _countSound = false;
        }
        if (_timer >= 1f)
        {
            _count--;
            _countSound = true;
            _timer = 0f;
            if (_count <= 0f)
            {
                _countSound = !_countSound;
                Status = GameStatus.Starting;
                _player.Start();
                _count = 3;
                _timer = 0;
            }
        }
    }

    private void CheckStarting()
    {
        if (_player.Position.X >= _centerX)
        {
            _player.Play(_centerX);
            Status = GameStatus.Playing;
        }
    }

    private void CheckPlaying(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _scoreTimer += deltaTime;
        if (_scoreTimer >= 0.15f)
        {
            _scoreTimer = 0f;

            float progress = _player.Position.X - _bounds.Left;
            if (progress > 0)
                Score += (int)(progress / 80 + 1);
        }

        // Acumular tiempo
        _obstacleTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Cuando se supera el tiempo objetivo, generar obstáculo
        if (_obstacleTimer >= _nextObstacleTime)
        {
            simpleObstacles.Add(
                new SimpleObstacle(
                    _atlas.CreateAnimatedSprite("cactus_1"),
                    ObstacleType.Simple,
                    new Vector2(_bounds.Right, _floorPos),
                    GetDynamicObstacleSpeed()
                )
            );

            // Reiniciar timer
            _obstacleTimer = 0f;

            _nextObstacleTime = GetNextObstacleTime(0.5f, 1.5f);
        }

        float obstacleSpeed = GetDynamicObstacleSpeed();

        _floorScrollX += obstacleSpeed * deltaTime;
        _backgroundScrollX += obstacleSpeed * BackgroundSpeedFactor * deltaTime;

        foreach (var obstacle in simpleObstacles)
        {
            if (_player.CollidesWith(obstacle.Bounds))
            {
                _player.GetPushed(gameTime, obstacle.VelocityX);
            }
        }
        if (_player.AtEdge(_bounds.Left))
        {
            _player.Die();
            Status = GameStatus.GameOver;
            _floorScrollX = 0f;
            _backgroundScrollX = 0f;
        }
    }

    private void UpdateObstacles(GameTime gameTime)
    {
        for (int i = simpleObstacles.Count - 1; i >= 0; i--)
        {
            var obstacle = simpleObstacles[i];
            obstacle.Update(gameTime);

            if (obstacle.Position.X + obstacle.Sprite.Width < _bounds.Left)
                simpleObstacles.RemoveAt(i);
        }
    }

    public void Start()
    {
        Status = GameStatus.Counting;
    }
    public void Win()
    {
        Status = GameStatus.Winner;
    }
    private float GetDynamicObstacleSpeed()
    {
        return BaseObstacleSpeed + (float)Math.Log(Score + 1) * 50f;
    }
    private float GetNextObstacleTime(float minTime, float maxTime)
    {
        Random _random = new Random();
        // Genera un número aleatorio entre 0.0 y 1.0
        double value = _random.NextDouble();
        // Escala ese número al rango [minTime, maxTime]
        return (float)(minTime + value * (maxTime - minTime));
    }

}