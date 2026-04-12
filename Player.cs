using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MiniMario;

/// <summary>
/// Player character with advanced platformer physics
/// </summary>
public class Player
{
    // Position and movement
    public Vector2 Position;
    public Vector2 Velocity;
    private bool _isGrounded;
    private bool _facingRight = true;
    
    // Jump mechanics
    private bool _hasDoubleJump;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    
    // Player stats
    public int Lives = Constants.PlayerMaxLives;
    public int Score = 0;
    public int CoinsCollected = 0;
    
    // Damage and invincibility
    private float _invincibilityTimer;
    private bool _isInvincible => _invincibilityTimer > 0;
    
    // Spawn point
    private Vector2 _spawnPoint;
    
    // Animation state
    private enum AnimationState { Idle, Walking, Jumping, Falling }
    private AnimationState _currentState = AnimationState.Idle;
    
    public Rectangle Bounds => new Rectangle(
        (int)Position.X,
        (int)Position.Y,
        Constants.PlayerWidth,
        Constants.PlayerHeight
    );
    
    public bool IsAlive => Lives > 0;
    
    /// <summary>
    /// Initializes player at spawn position
    /// </summary>
    public void Initialize(Vector2 spawnPoint)
    {
        _spawnPoint = spawnPoint;
        Position = spawnPoint;
        Velocity = Vector2.Zero;
        _isGrounded = false;
        _hasDoubleJump = false;
    }
    
    /// <summary>
    /// Updates player physics and input
    /// </summary>
    public void Update(GameTime gameTime, InputHelper input, TileMap tileMap)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Update invincibility timer
        if (_invincibilityTimer > 0)
        {
            _invincibilityTimer -= deltaTime;
        }
        
        // Handle input
        HandleInput(input, deltaTime);
        
        // Apply gravity
        ApplyGravity(deltaTime);
        
        // Update position
        Position += Velocity * deltaTime;
        
        // Handle collision with tiles
        HandleTileCollision(tileMap);
        
        // Update animation state
        UpdateAnimationState();
        
        // Update coyote time and jump buffer
        UpdateJumpTimers(deltaTime);
        
        // Check for death (falling off map)
        if (Position.Y > tileMap.PixelHeight)
        {
            TakeDamage();
        }
    }
    
    /// <summary>
    /// Handles keyboard input for movement and jumping
    /// </summary>
    private void HandleInput(InputHelper input, float deltaTime)
    {
        // Horizontal movement
        float targetVelocityX = 0;
        
        if (input.IsKeyDown(Keys.A) || input.IsKeyDown(Keys.Left))
        {
            targetVelocityX = -Constants.MoveSpeed;
            _facingRight = false;
        }
        else if (input.IsKeyDown(Keys.D) || input.IsKeyDown(Keys.Right))
        {
            targetVelocityX = Constants.MoveSpeed;
            _facingRight = true;
        }
        
        // Smooth acceleration
        if (targetVelocityX != 0)
        {
            Velocity.X = MathHelper.Lerp(Velocity.X, targetVelocityX, 
                (_isGrounded ? 1f : Constants.AirResistance) * deltaTime * 10f);
        }
        else
        {
            // Apply friction
            Velocity.X *= _isGrounded ? Constants.Friction : Constants.AirResistance;
            if (Math.Abs(Velocity.X) < 1f) Velocity.X = 0;
        }
        
        // Jump input
        if (input.IsKeyPressed(Keys.Space) || input.IsKeyPressed(Keys.W) || input.IsKeyPressed(Keys.Up))
        {
            _jumpBufferCounter = Constants.JumpBufferTime;
        }
        
        // Execute jump if conditions are met
        if (_jumpBufferCounter > 0)
        {
            // Regular jump (on ground or coyote time)
            if (_isGrounded || _coyoteTimeCounter > 0)
            {
                Velocity.Y = Constants.JumpForce;
                _jumpBufferCounter = 0;
                _coyoteTimeCounter = 0;
                _hasDoubleJump = true;
            }
            // Double jump (in air, not used yet)
            else if (_hasDoubleJump)
            {
                Velocity.Y = Constants.DoubleJumpForce;
                _jumpBufferCounter = 0;
                _hasDoubleJump = false;
            }
        }
        
        // Variable jump height (release jump early = shorter jump)
        if (input.IsKeyReleased(Keys.Space) || input.IsKeyReleased(Keys.W) || input.IsKeyReleased(Keys.Up))
        {
            if (Velocity.Y < 0)
            {
                Velocity.Y *= 0.5f;
            }
        }
    }
    
    /// <summary>
    /// Applies gravity to player
    /// </summary>
    private void ApplyGravity(float deltaTime)
    {
        Velocity.Y += Constants.Gravity * deltaTime;
        
        // Terminal velocity
        if (Velocity.Y > Constants.TerminalVelocity)
        {
            Velocity.Y = Constants.TerminalVelocity;
        }
    }
    
    /// <summary>
    /// Handles collision with tilemap
    /// </summary>
    private void HandleTileCollision(TileMap tileMap)
    {
        bool wasGrounded = _isGrounded;
        Rectangle bounds = Bounds;
        
        tileMap.ResolveCollision(bounds, ref Velocity, ref _isGrounded, true);
        
        Position.X = bounds.X;
        Position.Y = bounds.Y;
        
        // Reset double jump when landing
        if (_isGrounded && !wasGrounded)
        {
            _hasDoubleJump = true;
        }
        
        // Check for spike collision
        CheckSpikeCollision(tileMap);
        
        // Check for coin collection
        CheckCoinCollection(tileMap);
    }
    
    /// <summary>
    /// Checks if player touched spikes
    /// </summary>
    private void CheckSpikeCollision(TileMap tileMap)
    {
        int leftTile = Bounds.Left / Constants.TileSize;
        int rightTile = (Bounds.Right - 1) / Constants.TileSize;
        int topTile = Bounds.Top / Constants.TileSize;
        int bottomTile = (Bounds.Bottom - 1) / Constants.TileSize;
        
        for (int y = topTile; y <= bottomTile; y++)
        {
            for (int x = leftTile; x <= rightTile; x++)
            {
                if (tileMap.GetTileAt(x, y) == Constants.TileSpike)
                {
                    TakeDamage();
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// Checks if player collected coins
    /// </summary>
    private void CheckCoinCollection(TileMap tileMap)
    {
        int centerX = (Bounds.Center.X) / Constants.TileSize;
        int centerY = (Bounds.Center.Y) / Constants.TileSize;
        
        if (tileMap.GetTileAt(centerX, centerY) == Constants.TileCoin)
        {
            tileMap.RemoveCoin(centerX, centerY);
            Score += Constants.CoinValue;
            CoinsCollected++;
        }
    }
    
    /// <summary>
    /// Updates coyote time and jump buffer timers
    /// </summary>
    private void UpdateJumpTimers(float deltaTime)
    {
        // Coyote time - grace period after leaving ground
        if (_isGrounded)
        {
            _coyoteTimeCounter = Constants.CoyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= deltaTime;
        }
        
        // Jump buffer - remember jump input briefly
        if (_jumpBufferCounter > 0)
        {
            _jumpBufferCounter -= deltaTime;
        }
    }
    
    /// <summary>
    /// Updates animation state based on movement
    /// </summary>
    private void UpdateAnimationState()
    {
        if (!_isGrounded)
        {
            _currentState = Velocity.Y < 0 ? AnimationState.Jumping : AnimationState.Falling;
        }
        else if (Math.Abs(Velocity.X) > 10f)
        {
            _currentState = AnimationState.Walking;
        }
        else
        {
            _currentState = AnimationState.Idle;
        }
    }
    
    /// <summary>
    /// Player takes damage and loses a life
    /// </summary>
    public void TakeDamage()
    {
        if (_isInvincible) return;
        
        Lives--;
        _invincibilityTimer = Constants.InvincibilityTime;
        
        if (Lives > 0)
        {
            Respawn();
        }
    }
    
    /// <summary>
    /// Respawns player at spawn point
    /// </summary>
    public void Respawn()
    {
        Position = _spawnPoint;
        Velocity = Vector2.Zero;
        _invincibilityTimer = Constants.InvincibilityTime;
    }
    
    /// <summary>
    /// Player stomps on enemy (jump on top)
    /// </summary>
    public void StompEnemy()
    {
        Velocity.Y = Constants.EnemyStompBounce;
        Score += Constants.EnemyKillValue;
    }
    
    /// <summary>
    /// Draws the player
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        // Flashing effect when invincible
        if (_isInvincible && (int)(_invincibilityTimer * 10) % 2 == 0)
        {
            return; // Skip drawing to create flashing effect
        }
        
        // Draw player body (blue rectangle)
        Color bodyColor = _currentState switch
        {
            AnimationState.Jumping => Color.LightBlue,
            AnimationState.Falling => Color.CornflowerBlue,
            _ => Color.Blue
        };
        
        spriteBatch.Draw(texture, Bounds, bodyColor);
        
        // Draw eyes (white rectangles)
        int eyeOffsetX = _facingRight ? 4 : -4;
        Rectangle leftEye = new Rectangle(
            (int)Position.X + 8 + eyeOffsetX,
            (int)Position.Y + 12,
            6, 8
        );
        Rectangle rightEye = new Rectangle(
            (int)Position.X + 16 + eyeOffsetX,
            (int)Position.Y + 12,
            6, 8
        );
        
        spriteBatch.Draw(texture, leftEye, Color.White);
        spriteBatch.Draw(texture, rightEye, Color.White);
        
        // Draw pupils (black dots)
        Rectangle leftPupil = new Rectangle(
            (int)Position.X + 10 + eyeOffsetX,
            (int)Position.Y + 15,
            3, 4
        );
        Rectangle rightPupil = new Rectangle(
            (int)Position.X + 18 + eyeOffsetX,
            (int)Position.Y + 15,
            3, 4
        );
        
        spriteBatch.Draw(texture, leftPupil, Color.Black);
        spriteBatch.Draw(texture, rightPupil, Color.Black);
        
        // Draw direction indicator (small rectangle on facing side)
        if (_facingRight)
        {
            Rectangle indicator = new Rectangle((int)Position.X + Constants.PlayerWidth - 2, (int)Position.Y + 20, 2, 8);
            spriteBatch.Draw(texture, indicator, Color.White);
        }
        else
        {
            Rectangle indicator = new Rectangle((int)Position.X, (int)Position.Y + 20, 2, 8);
            spriteBatch.Draw(texture, indicator, Color.White);
        }
    }
}
