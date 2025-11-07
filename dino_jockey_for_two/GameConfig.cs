using Microsoft.Xna.Framework.Input;

namespace dino_jockey_for_two;

public static class GameConfig
{
    public const int ScreenWidth = 1280;
    public const int ScreenHeight = 720;
    public const bool FullScreen = false;
    
    public const float FloorScrollSpeed = 40f;
    public const float PlayerInitialSpeed = 5f;
    public const float PlayerStartPositionRatio = 0.5f;
    public const float PlayerJumpForce = -15f;
    public const float PlayerJumpImpulse = -0.4f;
    public const float Gravity = 0.7f;
    public const float MaxFallSpeed = 12f;
    public const int MaxJumpTime = 250;
    public const float ColliderWidthRatio = 0.65f;
    public const float ColliderHeightRatio = 0.82f;
    public const float PlayerScale = 1.5f;
    public const float ObstacleSpeed = 8f;
    public const int ObstacleSpawnInterval = 2000;
    public const float ObstacleMinSpawnInterval = 1000f;
    public const float ObstacleSpawnIntervalDecrement = 50f;
    public const float ObstacleSpawnYOffset = 10f;
    public static readonly Keys Player1JumpKey = Keys.Up;
    public static readonly Keys Player2JumpKey = Keys.Space;
}