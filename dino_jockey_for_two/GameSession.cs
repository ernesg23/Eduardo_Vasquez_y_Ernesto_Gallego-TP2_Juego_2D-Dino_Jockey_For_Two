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
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        HandleInitialAnimation();
        _player.Update(gameTime, inputManager);
        UpdateObstacles(gameTime);
        CheckCollisions();
        UpdateScore(gameTime);
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

    private void UpdateObstacles(GameTime gameTime)
    {
        _obstacleSpawnTimer += gameTime.ElapsedGameTime;
        
        foreach (var obstacle in _obstacles)
        {
            obstacle.Update(gameTime);
        }
        
        _obstacles.RemoveAll(obstacle => obstacle.ShouldRemove);
    }

    private void CheckCollisions()
    {
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
        if (!_player.IsDead)
        {
            Score += (int)gameTime.ElapsedGameTime.TotalMilliseconds / 100;
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
        _debugTexture = null;
        _debugTextureInitialized = false;
    }
}