namespace MiniMario;

/// <summary>
/// Global constants for game physics, rendering, and gameplay parameters
/// public state 
public static class Constants
{
    // ===== SCREEN & RENDERING ========
    public const int ScreenWidth = 1280;
    public const int ScreenHeight = 720;
    public const int TileSize = 32;
    
    // ===== PHYSICS =====
    public const float Gravity = 1800f;              // pixels/sec² - pulls entities down
    public const float TerminalVelocity = 900f;      // max fall speed (pixels/sec)
    public const float Friction = 0.85f;             // ground friction multiplier
    public const float AirResistance = 0.95f;        // air friction multiplier
    
    // ===== PLAYER MOVEMENT ========
    public const float MoveSpeed = 220f;             // horizontal movement speed (pixels/sec)
    public const float MoveAcceleration = 2500f;     // how fast player accelerates
    public const float JumpForce = -620f;            // initial jump velocity (negative = up)
    public const float DoubleJumpForce = -520f;      // second jump is slightly weaker
    public const float CoyoteTime = 0.1f;            // grace period after leaving edge (seconds)
    public const float JumpBufferTime = 0.15f;       // jump input buffer window (seconds)
    public const int PlayerWidth = 28;
    public const int PlayerHeight = 44;
    public const int PlayerMaxLives = 3;
    public const float InvincibilityTime = 1.5f;     // seconds of invincibility after hit
    
    // ===== CAMERA =====
    public const float CameraLerpSpeed = 5f;         // camera smoothing factor
    public const float CameraLookAhead = 80f;        // pixels ahead of player
    
    // ===== ENEMIES =====
    public const float WalkerSpeed = 80f;
    public const float JumperSpeed = 60f;
    public const float JumperJumpForce = -500f;
    public const float JumperJumpInterval = 2.0f;    // seconds between jumps
    public const float EnemyStompBounce = -400f;     // bounce when stomping enemy.
    public const int EnemySize = 32;
    
    // ===== SCORING =====
    public const int CoinValue = 10;
    public const int EnemyKillValue = 50;
    
    // ===== TILE TYPES =====
    public const int TileEmpty = 0;
    public const int TileGround = 1;
    public const int TilePlatform = 2;
    public const int TileSpike = 3;
    public const int TileCoin = 4;
    public const int TileEnemySpawn = 5;
    public const int TileCheckpoint = 6;
    public const int TileLevelEnd = 7;
}
