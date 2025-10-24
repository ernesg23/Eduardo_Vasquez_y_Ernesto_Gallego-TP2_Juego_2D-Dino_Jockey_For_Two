using Microsoft.Xna.Framework.Input;

public static class GameConfig
{
    public const int ScreenWidth = 1920;
    public const int ScreenHeight = 1080;
    public const bool FullScreen = true;
    
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
    public const float ObstacleSpeed = 5f;
    public const int ObstacleSpawnInterval = 2000;
    public const float ObstacleMinSpawnInterval = 800f;
    public const float ObstacleSpawnIntervalDecrement = 50f;
    public const float ObstacleSpawnYOffset = 10f;
    public static readonly Keys Player1JumpKey = Keys.Up;
    public static readonly Keys Player2JumpKey = Keys.Z;
}
