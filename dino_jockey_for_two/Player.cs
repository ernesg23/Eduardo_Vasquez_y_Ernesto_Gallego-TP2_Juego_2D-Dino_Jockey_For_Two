using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Collider;
using System.Threading;
using System;

public class Player
{
    private const float JUMP_FORCE = -15f;
    private const float IMPULSE_JUMP = -.4f;
    private const float GRAVITY = .7f;
    private const float MAX_FALL_SPEED = 12f;
    private const int MAX_JUMP_TIME = 250;

    private Keys _jumpKey;
    private bool _isJumping;
    private double _jumpTime;
    public Vector2 Position;
    public Vector2 Velocity;
    public Box Collider;
    private AnimatedSprite _sprite;
    private Dictionary<string, Animation> _animations;

    public bool IsDead { get; private set; }
    public bool InFloor { get; private set; }
    public bool StartAnim { get; set; }

    private float _floorY;

    public Player(TextureAtlas dinoAtlas, float floorY, Keys jumpKey)
    {
        _animations = new Dictionary<string, Animation>();
        foreach (var anim in new[] { "dino_walk", "dino_jump", "dino_dead" })
            _animations.Add(anim, dinoAtlas.GetAnimation(anim));

        _sprite = new AnimatedSprite(_animations["dino_walk"]);
        _sprite.CenterOrigin();
        _sprite.Scale = new Vector2(1.5f, 1.5f);

        _floorY = floorY - _sprite.Height/4;
        Position = new Vector2(0f, _floorY);
        Velocity = Vector2.Zero;

        Collider = new Box(Vector2.Zero, (int)(_sprite.Width * .65f), (int)(_sprite.Height * .82f));
        Collider.MoveCentered(Position);

        _jumpKey = jumpKey;
        Reset();
    }

    public void Reset()
    {
        StartAnim = true;
        IsDead = false;
        InFloor = false;
        _isJumping = true;
        _jumpTime = 0;
        Position = new Vector2(-_sprite.Width, _floorY - _sprite.Height/4);
        Velocity = Vector2.Zero;
    }

    public void Update(GameTime gameTime, InputManager inputManager)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (!StartAnim && !IsDead)
        {
            if (InFloor && inputManager.Keyboard.IsKeyDown(_jumpKey))
            {
                _isJumping = true;
                InFloor = false;
                Velocity.Y = JUMP_FORCE;
                _jumpTime = 0;
            }
            else if (_isJumping && inputManager.Keyboard.IsKeyDown(_jumpKey))
            {
                _jumpTime += deltaTime;
                if (_jumpTime < MAX_JUMP_TIME)
                    Velocity.Y += IMPULSE_JUMP;
            }
            else if (_isJumping && inputManager.Keyboard.WasKeyJustReleased(_jumpKey))
                _isJumping = false;
        }

        Velocity.Y += GRAVITY;
        if (Velocity.Y > MAX_FALL_SPEED)
            Velocity.Y = MAX_FALL_SPEED;

        Position += Velocity;
        Collider.MoveCentered(Position);

        if (Position.Y >= _floorY)
        {
            Position = new Vector2(Position.X, _floorY);
            Velocity.Y = 0;
            InFloor = true;
            _isJumping = false;
        }

        if (IsDead)
            _sprite.Animation = _animations["dino_dead"];
        else if (!InFloor)
            _sprite.Animation = _animations["dino_jump"];
        else
            _sprite.Animation = _animations["dino_walk"];

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
