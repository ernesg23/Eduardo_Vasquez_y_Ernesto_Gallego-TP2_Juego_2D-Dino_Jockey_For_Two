using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace dino_jockey_for_two;

public class GameSession
{
    // Factor de transición día/noche (0=día, 1=noche)
    private float _nightFactor;

    private float _nightTarget;
    // Velocidad para lograr 0->1 en 2 segundos (0.5 por segundo)
    private const float NightLerpRate = 0.5f;
    public bool RestartReady;
    public bool Restart;
    private double _startCountdown = -1;
    public bool CanStart;
    public bool IsReady;
    public bool Winner;
    public bool IsOver => Player.IsDead;
    private Rectangle _viewport;
    private readonly Sprite _floorSprite;
    private readonly float _floorY;
    private Vector2 _floorPosition1;
    private Vector2 _floorPosition2;
    public readonly string Name;
    private int Score;
    public readonly Player Player;
    private readonly List<Obstacle> _obstacles;
    private Texture2D _debugTexture;
    private bool _debugTextureInitialized;
    private Texture2D _obstacleTexture;
    private List<Texture2D> _obstacleTextures;
    private float _currentObstacleSpeed;
    private readonly Random _random = new Random();
    private double _scoreAccumulator;
    private double _spawnDelayTimer = -1;
    private readonly SpriteFont _font;

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
        _floorSprite.LayerDepth = 0.1f; // Piso por encima del fondo
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

        // Inicializar estado de día/noche según puntaje actual
        _nightTarget = IsNightByScore() ? 1f : 0f;
        _nightFactor = _nightTarget;
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

        // Reiniciar estado día/noche
        _nightTarget = IsNightByScore() ? 1f : 0f;
        _nightFactor = _nightTarget;
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

        // Reiniciar transición día/noche
        _nightTarget = IsNightByScore() ? 1f : 0f;
        _nightFactor = _nightTarget;
    }

    private void CreateObstacleTexture()
    {
        // Backward-compat placeholder: build multiple obstacle textures with different colors/sizes
        _obstacleTextures = new List<Texture2D>();
        CreateColoredObstacleTexture(30, 50, Color.ForestGreen);
        CreateColoredObstacleTexture(40, 60, Color.OrangeRed);
        CreateColoredObstacleTexture(50, 90, Color.MediumPurple);
        CreateColoredObstacleTexture(35, 70, Color.CadetBlue);

        // Keep legacy single texture as the first entry for any fallback
        _obstacleTexture = _obstacleTextures[0];
    }

    private void CreateColoredObstacleTexture(int width, int height, Color color)
    {
        var tex = new Texture2D(Player.GetGraphicsDevice(), width, height);
        var data = new Color[width * height];
        for (var i = 0; i < data.Length; i++)
            data[i] = new Color(color.R, color.G, color.B, (byte)255); // asegurar opaco
        tex.SetData(data);
        _obstacleTextures.Add(tex);
    }

    private void UpdateNightTransition(GameTime gameTime)
    {
        // Determinar objetivo según el puntaje
        _nightTarget = IsNightByScore() ? 1f : 0f;
        // Avanzar suavemente hacia el objetivo en 2 segundos
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float step = NightLerpRate * dt;
        if (_nightFactor < _nightTarget)
        {
            _nightFactor = MathHelper.Min(_nightFactor + step, _nightTarget);
        }
        else if (_nightFactor > _nightTarget)
        {
            _nightFactor = MathHelper.Max(_nightFactor - step, _nightTarget);
        }
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        // Actualizar transición día/noche suave
        UpdateNightTransition(gameTime);

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

        var last = _obstacles[^1];

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

    private int GetObstacleStyleIndex()
    {
        if (_obstacleTextures == null || _obstacleTextures.Count == 0) return 0;
        var idx = (Score / 150) % _obstacleTextures.Count; // cambia cada 150 puntos
        if (idx < 0) idx = 0;
        return idx;
    }

    private bool IsNightByScore()
    {
        // Cambia cada 100 puntos (100-199 noche, 200-299 día, etc.)
        int segment = (Score / 100);
        return (segment % 2) == 1;
    }

    private void SpawnObstacle()
    {
        // Seleccionar textura según puntaje actual
        var selected = _obstacleTextures is { Count: > 0 }
            ? _obstacleTextures[GetObstacleStyleIndex()]
            : _obstacleTexture;

        float obstacleY = _floorY - selected.Height + GameConfig.ObstacleSpawnYOffset;
        Vector2 startPosition = new Vector2(_viewport.X + _viewport.Width, obstacleY);

        var obstacleRegion = new TextureRegion(selected, 0, 0, selected.Width, selected.Height);
        var obstacleSprite = new Sprite(obstacleRegion)
        {
            Scale = Vector2.One,
            LayerDepth = 0.5f
        };

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
            // Puntaje idéntico para ambos jugadores: tasa constante por segundo, independiente de la posición
            _scoreAccumulator += gameTime.ElapsedGameTime.TotalSeconds * 10;
            var newScore = (int)_scoreAccumulator;
            if (newScore > Score)
                Score = newScore;
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

        // Fondo y tintes con transición suave (2s)
        float nightF = MathHelper.Clamp(_nightFactor, 0f, 1f);
        Color bgColor = Color.Lerp(Color.White, Color.Black, nightF);
        spriteBatch.Draw(_debugTexture, _viewport, bgColor);

        Color worldTint = Color.Lerp(Color.White, new Color(220, 220, 220, 255), nightF);
        Color uiColor = Color.Lerp(Color.Black, Color.White, nightF);

        _floorSprite.Tint = worldTint;
        Player.SetTint(worldTint);

        // Dibujar piso y jugador
        _floorSprite.Draw(spriteBatch, _floorPosition1);
        _floorSprite.Draw(spriteBatch, _floorPosition2);
        Player.Draw(spriteBatch);

        // Dibujar obstáculos con tinte acorde
        foreach (var obstacle in _obstacles)
        {
            obstacle.SetTint(worldTint);
            obstacle.Draw(spriteBatch);
        }

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
            spriteBatch.DrawString(_font, message, pos, uiColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.95f);
        }

        // -----------------------------
        // Puntaje siempre visible
        // -----------------------------
        string scoreText = $"Puntaje: {Score}";
        spriteBatch.DrawString(
            _font,
            scoreText,
            new Vector2(_viewport.X + 10 + _viewport.Width * 0.7f, _viewport.Y + 10),
            uiColor,
            0f,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            0.95f
        );
    }
    private void InitializeDebugTexture(GraphicsDevice graphicsDevice)
    {
        if (!_debugTextureInitialized)
        {
            _debugTexture = new Texture2D(graphicsDevice, 1, 1);
            _debugTexture.SetData([Color.White]);
            _debugTextureInitialized = true;
        }
    }

    public void UnloadContent()
    {
        _debugTexture?.Dispose();
        _debugTexture = null;
        _debugTextureInitialized = false;

        if (_obstacleTextures != null)
        {
            foreach (var tex in _obstacleTextures)
                tex?.Dispose();
            _obstacleTextures.Clear();
        }
        _obstacleTexture = null;
    }
}