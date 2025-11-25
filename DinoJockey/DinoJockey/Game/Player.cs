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

public class Player
{
    private readonly Dictionary<PlayerStatus, Animation> _animations = new Dictionary<PlayerStatus, Animation>();
    private AnimatedSprite _sprite = new AnimatedSprite();
    public Vector2 Position;
    private Rectangle _bounds => new Rectangle(
        (int)(Position.X + _sprite.Width / 4),
        (int)(Position.Y - _sprite.Height / 2),
        (int)_sprite.Width / 4,
        (int)_sprite.Height
    );
    private Keys _key;
    private float _floorY;

    // Variables para el salto dinámico
    private float _jumpVelocity = 0f;
    private const float GRAVITY = 1300f;
    private const float JUMP_FORCE = -600f;
    private const float MAX_JUMP_HEIGHT = 250f;
    private const float HOLD_JUMP_FORCE = -400f; // Fuerza adicional mientras se mantiene
    private bool _isJumping = false;
    private bool _isHoldingJump = false;
    private float _currentJumpHeight = 0f;
    private AudioController _audio;
    private SoundEffect _jump;
    private SoundEffect _push;
    public bool WasPushed;

    public PlayerStatus Status { get; private set; } = PlayerStatus.Waiting;
    private PlayerStatus _prevStatus;

    public Player(AudioController audio, SoundEffect jump, SoundEffect push, TextureAtlas atlas, Vector2 position, Keys key)
    {
        _audio = audio;
        _jump = jump;
        _push = push;
        _animations.Add(PlayerStatus.InFloor, atlas.GetAnimation("dino_walk"));
        _animations.Add(PlayerStatus.Starting, _animations[PlayerStatus.InFloor]);
        _animations.Add(PlayerStatus.Waiting, _animations[PlayerStatus.Starting]);
        _animations.Add(PlayerStatus.Jumping, atlas.GetAnimation("dino_jump"));
        _animations.Add(PlayerStatus.Falling, _animations[PlayerStatus.Jumping]);
        _animations.Add(PlayerStatus.Dead, atlas.GetAnimation("dino_dead"));

        _sprite.Animation = _animations[Status];
        _sprite.CenterOrigin();
        _sprite.Scale = Vector2.One * 1.5f;

        _key = key;
        _floorY = position.Y;

        Position = position;
        Position.X -= _sprite.Width / 2f;
    }

    public void Update(GameTime gameTime, KeyboardInfo keyboard)
    {
        checkStatus(gameTime, keyboard);
        _sprite.Update(gameTime);
    }

    private void checkStatus(GameTime gameTime, KeyboardInfo keyboard)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Status == PlayerStatus.Starting)
        {
            checkPrevStatus();
            Position.X += 200f * deltaTime;
        }
        else if (Status == PlayerStatus.InFloor)
        {
            checkPrevStatus();

            // Solo saltar si se presiona la tecla y no estamos ya saltando
            if (keyboard.IsKeyDown(_key) && !_isJumping)
                StartJump();
        }
        else if (Status == PlayerStatus.Jumping || Status == PlayerStatus.Falling)
        {
            checkPrevStatus();
            UpdateJump(deltaTime, keyboard);
        }
        else if(Status == PlayerStatus.Dead)
        {
            checkPrevStatus();
        }
    }

    private void StartJump()
    {
        _audio.PlaySoundEffect(_jump);
        _jumpVelocity = JUMP_FORCE;
        _isJumping = true;
        _isHoldingJump = true;
        _currentJumpHeight = 0f;
        Status = PlayerStatus.Jumping;
    }

    private void UpdateJump(float deltaTime, KeyboardInfo keyboard)
    {
        // Actualizar altura actual del salto
        _currentJumpHeight = _floorY - Position.Y;

        // Verificar si el jugador soltó la tecla de salto
        if (keyboard.WasKeyJustReleased(_key))
        {
            _isHoldingJump = false;
        }

        // Aplicar fuerza adicional mientras se mantiene presionado y no se haya alcanzado el máximo
        if (_isHoldingJump && _jumpVelocity < 0 && _currentJumpHeight < MAX_JUMP_HEIGHT)
        {
            _jumpVelocity += HOLD_JUMP_FORCE * deltaTime;
        }

        // Aplicar gravedad siempre
        _jumpVelocity += GRAVITY * deltaTime;

        // Aplicar movimiento vertical
        Position.Y += _jumpVelocity * deltaTime;

        // Verificar si llegó al suelo
        if (Position.Y >= _floorY)
            Land();
        // Verificar si está cayendo después del pico del salto
        else if (_jumpVelocity > 0 && Status == PlayerStatus.Jumping)
            Status = PlayerStatus.Falling;

        // Forzar caída si se alcanza la altura máxima
        if (_currentJumpHeight >= MAX_JUMP_HEIGHT && _jumpVelocity < 0)
        {
            _jumpVelocity = 0; // Comenzar a caer
            Status = PlayerStatus.Falling;
        }
    }

    private void Land()
    {
        Position.Y = _floorY;
        _jumpVelocity = 0f;
        _isJumping = false;
        _isHoldingJump = false;
        _currentJumpHeight = 0f;
        Status = PlayerStatus.InFloor;
    }

    private void checkPrevStatus()
    {
        if (_prevStatus != Status)
            _sprite.Animation = _animations[Status];
        _prevStatus = Status;
    }

    public void Play(float positionX)
    {
        Position.X = positionX;
        Position.Y = _floorY;
        Status = PlayerStatus.InFloor;
        _jumpVelocity = 0f;
        _isJumping = false;
        _isHoldingJump = false;
        _currentJumpHeight = 0f;
    }

    public void Start()
    {
        Status = PlayerStatus.Starting;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, Position);
    }
    public bool CollidesWith(Rectangle otherBounds)
    {
        return _bounds.Intersects(otherBounds);
    }
    public void GetPushed(GameTime gameTime,float velocityX)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Position.X -= velocityX * deltaTime;
    }
    public void DrawBounds(SpriteBatch spriteBatch, Texture2D pixelTexture, Color color)
    {
        if (pixelTexture != null)
        {
            spriteBatch.Draw(
                pixelTexture,
                _bounds,
                color * 0.5f // Semi-transparente para ver el sprite detrás
            );
        }
    }
    public bool AtEdge(float edge)
    {
        return Position.X - _sprite.Width * 0.5f < edge;
    }
    public void Die()
    {
        Status = PlayerStatus.Dead;
    }
}