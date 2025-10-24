using System;
using System.Collections.Generic;
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
    private Vector2 _floorPosition1;
    private Vector2 _floorPosition2;
    public string Name;
    public int Score = 0;
    private Player _player;
    private List<Obstacle> _obstacles;
    private Texture2D _debugTexture;
    private bool _debugTextureInitialized;
    private Texture2D _obstacleTexture;
    private float _currentObstacleSpeed;
    private Random _random = new Random();
    private double _scoreAccumulator = 0;
    private double _spawnDelayTimer = -1;

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
        _floorSprite.Scale = Vector2.One;
        _floorSprite.Origin = new Vector2(0, _floorSprite.Height / 2);

        _floorY = _viewport.Height + _viewport.Y - _floorSprite.Height;
        _floorPosition1 = new Vector2(0, _floorY);
        _floorPosition2 = new Vector2(_floorSprite.Width, _floorY);

        _player = new Player(dinoAtlas, _floorY, jumpKey);
        _obstacles = new List<Obstacle>();

        _currentObstacleSpeed = GameConfig.ObstacleSpeed;

        CreateObstacleTexture();
    }

    private void CreateObstacleTexture()
    {
        _obstacleTexture = new Texture2D(_player.GetGraphicsDevice(), 40, 60);
        Color[] data = new Color[40 * 60];
        for (int i = 0; i < data.Length; i++)
            data[i] = Color.Green;
        _obstacleTexture.SetData(data);
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        HandleInitialAnimation();
        HandleFloorScrolling(gameTime);
        _player.Update(gameTime, inputManager);
        UpdateObstacleSpawning(gameTime);
        UpdateObstacles(gameTime);
        CheckCollisions();
        UpdateScore(gameTime);
        IncreaseDifficulty();
    }

    private void HandleFloorScrolling(GameTime gameTime)
    {
        if (_player.IsDead) return; // detener el suelo al morir

        float deltaX = GameConfig.FloorScrollSpeed * _currentObstacleSpeed *
                       (float)gameTime.ElapsedGameTime.TotalSeconds;

        _floorPosition1.X -= deltaX;
        _floorPosition2.X -= deltaX;

        if (_floorPosition1.X <= -_floorSprite.Width)
            _floorPosition1.X = _floorPosition2.X + _floorSprite.Width;

        if (_floorPosition2.X <= -_floorSprite.Width)
            _floorPosition2.X = _floorPosition1.X + _floorSprite.Width;
    }

    private void HandleInitialAnimation()
    {
        if (!_player.StartAnim) return;

        float targetX = _viewport.Width * GameConfig.PlayerStartPositionRatio;
        if (_player.Position.X < targetX)
            _player.Position = new Vector2(_player.Position.X + GameConfig.PlayerInitialSpeed, _player.Position.Y);
        else
        {
            _player.Position = new Vector2(targetX, _player.Position.Y);
            _player.StartAnim = false;
        }
    }

    private void UpdateObstacleSpawning(GameTime gameTime)
    {
        if (_player.StartAnim || _player.IsDead) return;

        if (_obstacles.Count == 0)
        {
            SpawnObstacle();
            return;
        }

        var last = _obstacles[_obstacles.Count - 1];

        if (_spawnDelayTimer < 0 && last.Position.X < _viewport.Width)
            _spawnDelayTimer = 0.5 + _random.NextDouble() * 1.5; // entre 0.5 y 2s

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
            if (!_player.IsDead)
                _obstacles[i].Update(gameTime);

            if (_obstacles[i].ShouldRemove)
                _obstacles.RemoveAt(i);
        }
    }

    private void CheckCollisions()
    {
        if (_player.IsDead) return;

        bool collidedSide = false;

        foreach (var obstacle in _obstacles)
        {
            if (_player.Collider.CollidesWith(obstacle.Collider))
            {
                Rectangle playerBounds = _player.Collider.Bounds;
                Rectangle obstacleBounds = obstacle.Collider.Bounds;
                Rectangle intersection = Rectangle.Intersect(playerBounds, obstacleBounds);

                if (intersection.Width < intersection.Height)
                {
                    _player.Velocity = new Vector2(-_currentObstacleSpeed, _player.Velocity.Y);
                    collidedSide = true;
                }

                break;
            }
        }

        if (!collidedSide)
            _player.Velocity = new Vector2(0, _player.Velocity.Y);

        if (_player.Position.X - _player.Collider.Bounds.Width/2 <= _viewport.X && !_player.StartAnim)
            _player.Kill();
    }

    private void UpdateScore(GameTime gameTime)
    {
        if (!_player.IsDead && !_player.StartAnim)
        {
            _scoreAccumulator += gameTime.ElapsedGameTime.TotalSeconds * 10;
            Score = (int)_scoreAccumulator;
        }
    }

    private void IncreaseDifficulty()
    {
        if (!_player.IsDead && !_player.StartAnim)
            _currentObstacleSpeed = GameConfig.ObstacleSpeed + (Score * 0.01f);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        InitializeDebugTexture(spriteBatch.GraphicsDevice);

        _floorSprite.Draw(spriteBatch, _floorPosition1);
        _floorSprite.Draw(spriteBatch, _floorPosition2);
        _player.Draw(spriteBatch);

        foreach (var obstacle in _obstacles)
            obstacle.Draw(spriteBatch);

        if (_debugTexture != null)
        {
            spriteBatch.Draw(_debugTexture, _player.Collider.Bounds, Color.Red * 0.5f);

            foreach (var obstacle in _obstacles)
                spriteBatch.Draw(_debugTexture, obstacle.Collider.Bounds, Color.Blue * 0.5f);
        }
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
