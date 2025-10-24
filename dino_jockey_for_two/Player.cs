using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Collider;

public class Player
{
    private Keys _jumpKey;
    private bool _isJumping;
    private double _jumpTime;
    public Vector2 Position;
    public Vector2 Velocity;
    public Box Collider;
    private AnimatedSprite _sprite;
    private Dictionary<string, Animation> _animations;
    private float _floorY;

    public bool IsDead { get; private set; }
    public bool InFloor { get; private set; }
    public bool StartAnim { get; set; }

    public Player(TextureAtlas dinoAtlas, float floorY, Keys jumpKey)
    {
        _animations = new Dictionary<string, Animation>
        {
            ["dino_walk"] = dinoAtlas.GetAnimation("dino_walk"),
            ["dino_jump"] = dinoAtlas.GetAnimation("dino_jump"),
            ["dino_dead"] = dinoAtlas.GetAnimation("dino_dead")
        };

        _sprite = new AnimatedSprite(_animations["dino_walk"]);
        _sprite.CenterOrigin();
        _sprite.Scale = new Vector2(GameConfig.PlayerScale, GameConfig.PlayerScale);

        _floorY = floorY - _sprite.Height / 4;
        _jumpKey = jumpKey;
        
        Collider = new Box(
            Vector2.Zero, 
            (int)(_sprite.Width * GameConfig.ColliderWidthRatio), 
            (int)(_sprite.Height * GameConfig.ColliderHeightRatio)
        );
        
        Reset();
    }

    public void Reset()
    {
        StartAnim = true;
        IsDead = false;
        InFloor = false;
        _isJumping = false;
        _jumpTime = 0;
        Position = new Vector2(-_sprite.Width, _floorY);
        Velocity = Vector2.Zero;
        Collider.MoveCentered(Position);
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        if (IsDead) return;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (!StartAnim)
        {
            HandleJumpInput(inputManager, deltaTime);
        }

        ApplyPhysics();
        UpdatePosition();
        UpdateCollider();
        UpdateAnimation(gameTime);
    }

    private void HandleJumpInput(InputManager inputManager, float deltaTime)
    {
        if (InFloor && inputManager.Keyboard.IsKeyDown(_jumpKey))
        {
            StartJump();
        }
        else if (_isJumping && inputManager.Keyboard.IsKeyDown(_jumpKey))
        {
            ContinueJump(deltaTime);
        }
        else if (_isJumping && inputManager.Keyboard.WasKeyJustReleased(_jumpKey))
        {
            EndJump();
        }
    }

    private void StartJump()
    {
        _isJumping = true;
        InFloor = false;
        Velocity.Y = GameConfig.PlayerJumpForce;
        _jumpTime = 0;
    }

    private void ContinueJump(float deltaTime)
    {
        _jumpTime += deltaTime;
        if (_jumpTime < GameConfig.MaxJumpTime)
        {
            Velocity.Y += GameConfig.PlayerJumpImpulse;
        }
    }

    private void EndJump()
    {
        _isJumping = false;
    }

    private void ApplyPhysics()
    {
        Velocity.Y += GameConfig.Gravity;
        if (Velocity.Y > GameConfig.MaxFallSpeed)
        {
            Velocity.Y = GameConfig.MaxFallSpeed;
        }
    }

    private void UpdatePosition()
    {
        Position += Velocity;

        if (Position.Y >= _floorY)
        {
            Position = new Vector2(Position.X, _floorY);
            Velocity.Y = 0;
            InFloor = true;
            _isJumping = false;
        }
    }

    private void UpdateCollider()
    {
        Collider.MoveCentered(Position);
    }

    private void UpdateAnimation(GameTime gameTime)
    {
        if (IsDead)
        {
            _sprite.Animation = _animations["dino_dead"];
        }
        else if (!InFloor)
        {
            _sprite.Animation = _animations["dino_jump"];
        }
        else
        {
            _sprite.Animation = _animations["dino_walk"];
        }

        _sprite.Update(gameTime);
    }

    public void Kill()
    {
        IsDead = true;
        Velocity = Vector2.Zero;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, Position);
    }
}