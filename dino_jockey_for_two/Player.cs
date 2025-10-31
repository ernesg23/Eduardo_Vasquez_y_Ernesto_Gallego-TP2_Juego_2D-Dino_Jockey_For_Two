using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Collider;

public class Player
{
    public Keys JumpKey;
    private bool _isJumping;
    private double _jumpTime;
    public Vector2 Position;
    public Vector2 Velocity;
    public Box Collider;

    private AnimatedSprite _sprite;
    private Dictionary<string, Animation> _animations;
    private float _floorY;
    private GraphicsDevice _graphicsDevice;

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
        _sprite.LayerDepth = 0.6f;

        _floorY = floorY - _sprite.Height / 4;
        JumpKey = jumpKey;

        Collider = new Box(
            Vector2.Zero,
            (int)(_sprite.Width * GameConfig.ColliderWidthRatio),
            (int)(_sprite.Height * GameConfig.ColliderHeightRatio)
        );

        _graphicsDevice = dinoAtlas.GetGraphicsDevice();

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

        // Volver a sprite de caminar desde cero
        SetSprite(_animations["dino_walk"]);
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        float deltaTimeMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (!IsDead)
        {
            if (!StartAnim)
                HandleJumpInput(inputManager, deltaTimeMs);

            ApplyPhysics();
            UpdatePosition();
            UpdateCollider();
        }

        UpdateAnimation(gameTime);
    }

    // Solo avanza animación, sin input ni física
    public void UpdateAnimationOnly(GameTime gameTime)
    {
        UpdateAnimation(gameTime);
    }

    private void HandleJumpInput(InputManager inputManager, float deltaTimeMs)
    {
        if (InFloor && inputManager.Keyboard.IsKeyDown(JumpKey))
            StartJump();
        else if (_isJumping && inputManager.Keyboard.IsKeyDown(JumpKey))
            ContinueJump(deltaTimeMs);
        else if (_isJumping && inputManager.Keyboard.WasKeyJustReleased(JumpKey))
            EndJump();
    }

    private void StartJump()
    {
        _isJumping = true;
        InFloor = false;
        Velocity.Y = GameConfig.PlayerJumpForce;
        _jumpTime = 0;
    }

    private void ContinueJump(float deltaTimeMs)
    {
        _jumpTime += deltaTimeMs;
        if (_jumpTime < GameConfig.MaxJumpTime)
            Velocity.Y += GameConfig.PlayerJumpImpulse;
    }

    private void EndJump()
    {
        _isJumping = false;
    }

    private void ApplyPhysics()
    {
        Velocity.Y += GameConfig.Gravity;
        if (Velocity.Y > GameConfig.MaxFallSpeed)
            Velocity.Y = GameConfig.MaxFallSpeed;
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
        // Evitamos reasignar la animación cada frame.
        if (IsDead)
        {
            // Ya en muerte: no cambiar nada; el sprite creado en Kill() se animará solo.
        }
        else if (!InFloor)
        {
            if (_sprite.Animation != _animations["dino_jump"])
                _sprite.Animation = _animations["dino_jump"];
        }
        else
        {
            if (_sprite.Animation != _animations["dino_walk"])
                _sprite.Animation = _animations["dino_walk"];
        }

        _sprite.Update(gameTime);
    }

    public void Kill()
    {
        IsDead = true;
        Velocity = Vector2.Zero;

        // Asegurar arranque desde primer frame: recreamos el AnimatedSprite con la animación de muerte.
        SetSprite(_animations["dino_dead"]);
    }

    private void SetSprite(Animation anim)
    {
        // Preservar propiedades y reiniciar anim desde frame 0 creando un nuevo AnimatedSprite
        var prevScale = _sprite.Scale;
        var prevLayer = _sprite.LayerDepth;

        _sprite = new AnimatedSprite(anim);
        _sprite.CenterOrigin();
        _sprite.Scale = prevScale;
        _sprite.LayerDepth = prevLayer;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, Position);
    }

    public GraphicsDevice GetGraphicsDevice()
    {
        return _graphicsDevice;
    }
}
