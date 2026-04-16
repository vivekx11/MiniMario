using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MiniMario;

/// <summary>
/// Base class for all enemy types
/// 
public abstract class Enemy
{
    public Vector2 Position;
    public Vector2 Velocity;
    public bool IsAlive = true;
    public int Health;
    protected float _gravity = Constants.Gravity;
    protected bool _isGrounded;
    
    public Rectangle Bounds => new Rectangle(
        (int)Position.X,
        (int)Position.Y,
        Constants.EnemySize,
        Constants.EnemySize
    );
    
    /// <summary>
    /// Updates enemy logic
    /// </summary>
    public abstract void Update(GameTime gameTime, TileMap tileMap);
    
    /// <summary>
    /// Draws the enemy
    /// </summary>
    public abstract void Draw(SpriteBatch spriteBatch, Texture2D texture);
    
    /// <summary>
    /// Applies gravity to the enemy
    /// </summary>
    protected void ApplyGravity(float deltaTime)
    {
        Velocity.Y += _gravity * deltaTime;
        if (Velocity.Y > Constants.TerminalVelocity)
        {
            Velocity.Y = Constants.TerminalVelocity;
        }
    }
    
    /// <summary>
    /// Handles collision with tilemap
    /// </summary>
    // maping
    protected void HandleTileCollision(TileMap tileMap)
    {
        Rectangle bounds = Bounds;
        tileMap.ResolveCollision(bounds, ref Velocity, ref _isGrounded, false);
        Position.X = bounds.X;
        Position.Y = bounds.Y;
    }
    
    /// <summary>
    /// Takes damage and reduces health
    /// </summary>
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            IsAlive = false;
        }
    }
}

/// <summary>
/// Walker enemy - patrols back and forth
/// </summary>
public class WalkerEnemy : Enemy
{
    private float _direction = 1f; // 1 = right, -1 = left
    private float _patrolDistance = 200f;// distance 
    private Vector2 _startPosition;
    
    public WalkerEnemy(Vector2 position)
    {
        Position = position;
        _startPosition = position;
        Health = 1;
        Velocity = Vector2.Zero;
    }
    
    public override void Update(GameTime gameTime, TileMap tileMap)
    {
        if (!IsAlive) return;
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Apply gravity
        ApplyGravity(deltaTime);
        
        // Move horizontally
        Velocity.X = Constants.WalkerSpeed * _direction;
        
        // Update position
        Position += Velocity * deltaTime;
        
        // Handle tile collision
        HandleTileCollision(tileMap);
        
        // Reverse direction at patrol boundaries or walls
        if (Math.Abs(Position.X - _startPosition.X) > _patrolDistance)
        {
            _direction *= -1;
        }
        
        // Check for walls ahead
        int tileAheadX = (int)((Position.X + Constants.EnemySize * _direction) / Constants.TileSize);
        int tileY = (int)((Position.Y + Constants.EnemySize / 2) / Constants.TileSize);
        
        if (tileMap.IsSolid(tileAheadX, tileY))
        {
            _direction *= -1;
        }
        
        // Check for edge (no ground ahead)
        int tileGroundX = (int)((Position.X + Constants.EnemySize / 2 + Constants.EnemySize * _direction) / Constants.TileSize);
        int tileGroundY = (int)((Position.Y + Constants.EnemySize + 4) / Constants.TileSize);
        
        if (_isGrounded && !tileMap.IsSolid(tileGroundX, tileGroundY) && !tileMap.IsPlatform(tileGroundX, tileGroundY))
        {
            _direction *= -1;
        }
    }
    
    public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        if (!IsAlive) return;
        
        // Draw red rectangle for body
        spriteBatch.Draw(texture, Bounds, Color.Red);
        
        // Draw angry eyebrows (two small rectangles)
        Rectangle leftEyebrow = new Rectangle((int)Position.X + 6, (int)Position.Y + 8, 8, 2);
        Rectangle rightEyebrow = new Rectangle((int)Position.X + 18, (int)Position.Y + 8, 8, 2);
        spriteBatch.Draw(texture, leftEyebrow, Color.DarkRed);
        spriteBatch.Draw(texture, rightEyebrow, Color.DarkRed);
        
        // Draw eyes
        Rectangle leftEye = new Rectangle((int)Position.X + 8, (int)Position.Y + 12, 4, 4);
        Rectangle rightEye = new Rectangle((int)Position.X + 20, (int)Position.Y + 12, 4, 4);
        spriteBatch.Draw(texture, leftEye, Color.Black);
        spriteBatch.Draw(texture, rightEye, Color.Black);
    }
}

/// <summary>
/// Jumper enemy - periodically jumps toward player
/// </summary>
public class JumperEnemy : Enemy
{
    private float _jumpTimer;
    private Vector2 _playerPosition;
    
    public JumperEnemy(Vector2 position)
    {
        Position = position;
        Health = 2;
        Velocity = Vector2.Zero;
        _jumpTimer = Constants.JumperJumpInterval;
    }
    
    /// <summary>
    /// Updates player position for AI targeting
    /// </summary>
    public void SetPlayerPosition(Vector2 playerPos)
    {
        _playerPosition = playerPos;
    }
    
    public override void Update(GameTime gameTime, TileMap tileMap)
    {
        if (!IsAlive) return;
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Apply gravity
        ApplyGravity(deltaTime);
        
        // Update position
        Position += Velocity * deltaTime;
        
        // Handle tile collision
        HandleTileCollision(tileMap);
        
        // Jump logic
        _jumpTimer -= deltaTime;
        if (_jumpTimer <= 0 && _isGrounded)
        {
            // Jump toward player
            float directionToPlayer = Math.Sign(_playerPosition.X - Position.X);
            Velocity.Y = Constants.JumperJumpForce;
            Velocity.X = Constants.JumperSpeed * directionToPlayer;
            
            _jumpTimer = Constants.JumperJumpInterval;
        }
        
        // Apply air resistance
        if (!_isGrounded)
        {
            Velocity.X *= Constants.AirResistance;
        }
        else
        {
            Velocity.X *= Constants.Friction;
        }
    }
    
    public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        if (!IsAlive) return;
        
        // Draw orange rectangle for body
        spriteBatch.Draw(texture, Bounds, Color.Orange);
        
        // Draw rounded look (smaller rectangles at corners to simulate roundness)
        Rectangle topLeft = new Rectangle((int)Position.X, (int)Position.Y, 4, 4);
        Rectangle topRight = new Rectangle((int)Position.X + Constants.EnemySize - 4, (int)Position.Y, 4, 4);
        spriteBatch.Draw(texture, topLeft, Color.DarkOrange);
        spriteBatch.Draw(texture, topRight, Color.DarkOrange);
        
        // Draw eyes
        Rectangle leftEye = new Rectangle((int)Position.X + 8, (int)Position.Y + 12, 5, 5);
        Rectangle rightEye = new Rectangle((int)Position.X + 19, (int)Position.Y + 12, 5, 5);
        spriteBatch.Draw(texture, leftEye, Color.Black);
        spriteBatch.Draw(texture, rightEye, Color.Black);
        
        // Draw mouth
        Rectangle mouth = new Rectangle((int)Position.X + 10, (int)Position.Y + 22, 12, 3);
        spriteBatch.Draw(texture, mouth, Color.DarkOrange);
    }
}
