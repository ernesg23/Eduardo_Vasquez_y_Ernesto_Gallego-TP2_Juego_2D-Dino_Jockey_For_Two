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
    public string Name;
    public int Score = 0;
    private Player _player;
    private List<Obstacle> _obstacles;
    private TimeSpan _obstacleSpawnTimer;
    private Texture2D _debugTexture;
    private bool _debugTextureInitialized;
    private Random _random;
    private float _currentObstacleSpawnInterval;
    private Texture2D _obstacleTexture;

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
        _floorY = _viewport.Height + _viewport.Y - _floorSprite.Height;
        _player = new Player(dinoAtlas, _floorY, jumpKey);
        _obstacles = new List<Obstacle>();
        _obstacleSpawnTimer = TimeSpan.Zero;
        _random = new Random();
        _currentObstacleSpawnInterval = GameConfig.ObstacleSpawnInterval;

        CreateObstacleTexture();
    }

    private void CreateObstacleTexture()
    {
        _obstacleTexture = new Texture2D(_player.GetGraphicsDevice(), 40, 60);
        Color[] data = new Color[40 * 60];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Color.Green; // Obstáculos verdes para distinguirlos
        }
        _obstacleTexture.SetData(data);
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        HandleInitialAnimation();
        _player.Update(gameTime, inputManager);
        UpdateObstacleSpawning(gameTime);
        UpdateObstacles(gameTime);
        CheckCollisions();
        UpdateScore(gameTime);
        IncreaseDifficulty();
    }

    private void HandleInitialAnimation()
    {
        if (!_player.StartAnim) return;

        float targetX = _viewport.Width * GameConfig.PlayerStartPositionRatio;
        if (_player.Position.X < targetX)
        {
            _player.Position = new Vector2(_player.Position.X + GameConfig.PlayerInitialSpeed, _player.Position.Y);
        }
        else
        {
            _player.Position = new Vector2(targetX, _player.Position.Y);
            _player.StartAnim = false;
        }
    }

    private void UpdateObstacleSpawning(GameTime gameTime)
    {
        if (_player.StartAnim || _player.IsDead) return;

        _obstacleSpawnTimer += gameTime.ElapsedGameTime;

        if (_obstacleSpawnTimer.TotalMilliseconds >= _currentObstacleSpawnInterval)
        {
            SpawnObstacle();
            _obstacleSpawnTimer = TimeSpan.Zero;
        }
    }

    private void SpawnObstacle()
    {
        float obstacleY = _floorY - _obstacleTexture.Height + GameConfig.ObstacleSpawnYOffset;
        Vector2 startPosition = new Vector2(_viewport.Width, obstacleY);

        // Se crea TextureRegion a partir de la Texture2D y se usa para construir el Sprite
        var obstacleRegion = new TextureRegion(_obstacleTexture, 0, 0, _obstacleTexture.Width, _obstacleTexture.Height);
        var obstacleSprite = new Sprite(obstacleRegion);
        obstacleSprite.Scale = Vector2.One;

        Obstacle obstacle = new Obstacle(
            obstacleSprite,
            startPosition,
            GameConfig.ObstacleSpeed,
            _viewport.Width
        );

        _obstacles.Add(obstacle);
    }

    private void UpdateObstacles(GameTime gameTime)
    {
        for (int i = _obstacles.Count - 1; i >= 0; i--)
        {
            _obstacles[i].Update(gameTime);
            if (_obstacles[i].ShouldRemove)
            {
                _obstacles.RemoveAt(i);
            }
        }
    }

    private void CheckCollisions()
    {
        if (_player.IsDead) return;

        foreach (var obstacle in _obstacles)
        {
            if (_player.Collider.CollidesWith(obstacle.Collider))
            {
                _player.Kill();
                break;
            }
        }
    }

    private void UpdateScore(GameTime gameTime)
    {
        if (!_player.IsDead && !_player.StartAnim)
        {
            Score += (int)gameTime.ElapsedGameTime.TotalMilliseconds / 100;
        }
    }

    private void IncreaseDifficulty()
    {
        if (!_player.IsDead && !_player.StartAnim)
        {
            float newInterval = GameConfig.ObstacleSpawnInterval - (Score * 0.01f);
            _currentObstacleSpawnInterval = Math.Max(newInterval, GameConfig.ObstacleMinSpawnInterval);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        InitializeDebugTexture(spriteBatch.GraphicsDevice);

        _floorSprite.Draw(spriteBatch, new Vector2(0, _floorY));
        _player.Draw(spriteBatch);

        foreach (var obstacle in _obstacles)
        {
            obstacle.Draw(spriteBatch);
        }

        if (_debugTexture != null)
        {
            spriteBatch.Draw(_debugTexture, _player.Collider.Bounds, Color.Red * 0.5f);

            foreach (var obstacle in _obstacles)
            {
                spriteBatch.Draw(_debugTexture, obstacle.Collider.Bounds, Color.Blue * 0.5f);
            }
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
  