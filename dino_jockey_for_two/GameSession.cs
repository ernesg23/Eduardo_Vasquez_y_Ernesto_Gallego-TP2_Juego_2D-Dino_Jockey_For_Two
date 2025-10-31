using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

public class GameSession
{
    public bool RestartReady;
    public bool Restart;
    private double _startCountdown = -1;
    public bool CanStart;
    public bool IsReady;
    public bool Winner;
    public bool IsOver => Player.IsDead;
    private Rectangle _viewport;
    private Sprite _floorSprite;
    private float _floorY;
    private Vector2 _floorPosition1;
    private Vector2 _floorPosition2;
    public string Name;
    public int Score = 0;
    public Player Player;
    private List<Obstacle> _obstacles;
    private Texture2D _debugTexture;
    private bool _debugTextureInitialized;
    private Texture2D _obstacleTexture;
    private float _currentObstacleSpeed;
    private Random _random = new Random();
    private double _scoreAccumulator = 0;
    private double _spawnDelayTimer = -1;
    private SpriteFont _font;

    public GameSession(
        Rectangle viewport,
        Sprite floorSprite,
        TextureAtlas dinoAtlas,
        Keys jumpKey,
        string name,
        SpriteFont font
    )
    {
        Winner = false;
        CanStart = false;
        IsReady = false;
        Name = name;
        _viewport = viewport;
        _floorSprite = floorSprite;
        _floorSprite.LayerDepth = 0f;
        _floorSprite.Scale = Vector2.One;
        _floorSprite.Origin = new Vector2(0, _floorSprite.Height / 2);

        _floorY = _viewport.Height + _viewport.Y - _floorSprite.Height;
        _floorPosition1 = new Vector2(0, _floorY);
        _floorPosition2 = new Vector2(_floorSprite.Width, _floorY);

        Player = new Player(dinoAtlas, _floorY, jumpKey);
        _obstacles = new List<Obstacle>();

        _currentObstacleSpeed = GameConfig.ObstacleSpeed;
        _font = font;

        CreateObstacleTexture();
    }

    public void Reset()
    {
        Restart = false;
        Winner = false;
        CanStart = false;
        IsReady = false;
        Score = 0;
        _scoreAccumulator = 0;
        _spawnDelayTimer = -1;
        _currentObstacleSpeed = GameConfig.ObstacleSpeed;
        _obstacles.Clear();
        Player.Reset();
    }
    public void ResetSession()
    {
        Winner = false;
        RestartReady = false;
        CanStart = false;
        IsReady = false;

        _startCountdown = -1;
        Score = 0;
        _scoreAccumulator = 0;
        _spawnDelayTimer = -1;
        _currentObstacleSpeed = GameConfig.ObstacleSpeed;
        _obstacles.Clear();
        Player.Reset();

        // Reposicionar piso
        _floorPosition1 = new Vector2(0, _floorY);
        _floorPosition2 = new Vector2(_floorSprite.Width, _floorY);
    }

    private void CreateObstacleTexture()
    {
        _obstacleTexture = new Texture2D(Player.GetGraphicsDevice(), 40, 60);
        Color[] data = new Color[40 * 60];
        for (int i = 0; i < data.Length; i++)
            data[i] = Color.Green;
        _obstacleTexture.SetData(data);
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        // 1. Confirmación de "listo" para comenzar (solo antes del countdown)
        if (!IsReady && !CanStart && _startCountdown == -1 && !IsOver && !Winner)
        {
            if (inputManager.Keyboard.WasKeyJustPressed(Player.JumpKey))
                IsReady = true;
        }

        // 2. Countdown activo: solo animación
        if (_startCountdown >= 0)
        {
            _startCountdown -= gameTime.ElapsedGameTime.TotalSeconds;
            if (_startCountdown <= 0)
            {
                CanStart = true;
                _startCountdown = -2; // marcado como terminado
            }

            Player.UpdateAnimationOnly(gameTime);
            return;
        }

        // 3. Fin de partida: solo animación (no leer input de salto aquí)
        if (IsOver || Winner)
        {
            Player.UpdateAnimationOnly(gameTime);
            return;
        }

        // 4. Esperando arranque (por el otro jugador / Game1): solo animación
        if (!CanStart)
        {
            Player.UpdateAnimationOnly(gameTime);
            return;
        }

        // 5. Juego en curso
        HandleInitialAnimation();
        HandleFloorScrolling(gameTime);
        Player.Update(gameTime, inputManager);
        UpdateObstacleSpawning(gameTime);
        UpdateObstacles(gameTime);
        CheckCollisions();
        UpdateScore(gameTime);
        IncreaseDifficulty(gameTime);
    }

    private void HandleFloorScrolling(GameTime gameTime)
    {
        if (Player.IsDead) return;

        float deltaX = GameConfig.FloorScrollSpeed * _currentObstacleSpeed *
                       (float)gameTime.ElapsedGameTime.TotalSeconds;

        _floorPosition1.X -= deltaX;
        _floorPosition2.X -= deltaX;

        if (_floorPosition1.X <= -_floorSprite.Width)
            _floorPosition1.X = _floorPosition2.X + _floorSprite.Width;

        if (_floorPosition2.X <= -_floorSprite.Width)
            _floorPosition2.X = _floorPosition1.X + _floorSprite.Width;
    }
    public void BeginCountdown(double interval)
    {
        if (_startCountdown == -1)
            _startCountdown = interval;
    }

    private void HandleInitialAnimation()
    {
        if (!Player.StartAnim) return;

        float targetX = _viewport.Width * GameConfig.PlayerStartPositionRatio;
        if (Player.Position.X < targetX)
            Player.Position = new Vector2(Player.Position.X + GameConfig.PlayerInitialSpeed, Player.Position.Y);
        else
        {
            Player.Position = new Vector2(targetX, Player.Position.Y);
            Player.StartAnim = false;
        }
    }

    private void UpdateObstacleSpawning(GameTime gameTime)
    {
        if (Player.StartAnim || Player.IsDead) return;

        if (_obstacles.Count == 0)
        {
            SpawnObstacle();
            return;
        }

        var last = _obstacles[_obstacles.Count - 1];

        if (_spawnDelayTimer < 0 && last.Position.X < _viewport.Width)
            _spawnDelayTimer = 0.5 + _random.NextDouble() * 1.5;

        if (_spawnDelayTimer > 0)
        {
            _spawnDelayTimer -= gameTime.ElapsedGameTime.TotalSeconds;

            if (_spawnDelayTimer <= 0)
            {
                SpawnObstacle();
                _spawnDelayTimer = -1;
            }
        }
    }

    private void SpawnObstacle()
    {
        float obstacleY = _floorY - _obstacleTexture.Height + GameConfig.ObstacleSpawnYOffset;
        Vector2 startPosition = new Vector2(_viewport.Width, obstacleY);

        var obstacleRegion = new TextureRegion(_obstacleTexture, 0, 0, _obstacleTexture.Width, _obstacleTexture.Height);
        var obstacleSprite = new Sprite(obstacleRegion);
        obstacleSprite.Scale = Vector2.One;
        obstacleSprite.LayerDepth = 0.5f;

        Obstacle obstacle = new Obstacle(
            obstacleSprite,
            startPosition,
            _currentObstacleSpeed,
            _viewport.Width
        );

        _obstacles.Add(obstacle);
    }

    private void UpdateObstacles(GameTime gameTime)
    {
        for (int i = _obstacles.Count - 1; i >= 0; i--)
        {
            if (!Player.IsDead)
                _obstacles[i].Update(gameTime);

            if (_obstacles[i].ShouldRemove)
                _obstacles.RemoveAt(i);
        }
    }

    private void CheckCollisions()
    {
        if (Player.IsDead) return;

        bool collidedSide = false;

        foreach (var obstacle in _obstacles)
        {
            if (Player.Collider.CollidesWith(obstacle.Collider))
            {
                Rectangle playerBounds = Player.Collider.Bounds;
                Rectangle obstacleBounds = obstacle.Collider.Bounds;
                Rectangle intersection = Rectangle.Intersect(playerBounds, obstacleBounds);

                if (intersection.Width < intersection.Height)
                {
                    Player.Velocity = new Vector2(-_currentObstacleSpeed, Player.Velocity.Y);
                    collidedSide = true;
                }

                break;
            }
        }

        if (!collidedSide)
            Player.Velocity = new Vector2(0, Player.Velocity.Y);

        if (Player.Position.X - Player.Collider.Bounds.Width / 2 <= _viewport.X && !Player.StartAnim)
            Player.Kill();
    }

    private void UpdateScore(GameTime gameTime)
    {
        if (!Player.IsDead && !Player.StartAnim)
        {
            float relativeX = (Player.Position.X - _viewport.X) / (float)_viewport.Width;

            float scoreFactor = MathHelper.Lerp(0.5f, 1.5f, relativeX);

            _scoreAccumulator += gameTime.ElapsedGameTime.TotalSeconds * 10 * scoreFactor;
            Score = (int)_scoreAccumulator;
        }
    }


    private void IncreaseDifficulty(GameTime gameTime)
    {
        if (Player.IsDead || Player.StartAnim) return;

        float timeFactor = (float)gameTime.TotalGameTime.TotalSeconds * 0.001f;
        float scoreFactor = Score * 0.01f;

        _currentObstacleSpeed = GameConfig.ObstacleSpeed + timeFactor + scoreFactor;
    }


    public void Draw(SpriteBatch spriteBatch)
    {
        InitializeDebugTexture(spriteBatch.GraphicsDevice);

        // Dibujar piso y jugador
        _floorSprite.Draw(spriteBatch, _floorPosition1);
        _floorSprite.Draw(spriteBatch, _floorPosition2);
        Player.Draw(spriteBatch);

        // Dibujar obstáculos
        foreach (var obstacle in _obstacles)
            obstacle.Draw(spriteBatch);

        // Debug: colliders
        if (_debugTexture != null)
        {
            spriteBatch.Draw(_debugTexture, Player.Collider.Bounds, Color.Red * 0.5f);
            foreach (var obstacle in _obstacles)
                spriteBatch.Draw(_debugTexture, obstacle.Collider.Bounds, Color.Blue * 0.5f);
        }

        // -----------------------------
        // Mensajes de estado
        // -----------------------------
        string message = null;

        if (!IsReady)
            message = $"Presiona [{Player.JumpKey}] para empezar";
        else if (_startCountdown > 0)
        {
            int secondsLeft = (int)Math.Ceiling(_startCountdown);
            message = secondsLeft.ToString();
        }
        else if (IsOver || Winner)
        {
            message = RestartReady
                ? "Listo para reiniciar"
                : $"Presiona [{Player.JumpKey}] para reiniciar";
        }

        if (message != null)
        {
            Vector2 size = _font.MeasureString(message);
            Vector2 pos = new Vector2(
                _viewport.Center.X - size.X / 2,
                _viewport.Y + (_viewport.Height / 2) - size.Y / 2
            );
            spriteBatch.DrawString(_font, message, pos, Color.Black);
        }

        // -----------------------------
        // Puntaje siempre visible
        // -----------------------------
        string scoreText = $"Puntaje: {Score}";
        spriteBatch.DrawString(
            _font,
            scoreText,
            new Vector2(_viewport.X + 10 + _viewport.Width * 0.7f, _viewport.Y + 10),
            Color.Black
        );
    }
    private void InitializeDebugTexture(GraphicsDevice graphicsDevice)
    {
        if (!_debugTextureInitialized)
        {
            _debugTexture = new Texture2D(graphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });
            _debugTextureInitialized = true;
        }
    }

    public void UnloadContent()
    {
        _debugTexture?.Dispose();
        _obstacleTexture?.Dispose();
        _debugTexture = null;
        _obstacleTexture = null;
        _debugTextureInitialized = false;
    }
}
