// this is file import 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MiniMario;

public class Game1 : Game
{
    // ===== GRAPHICS & RENDERING ======
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixelTexture; // Simple 1x1 white texture for drawing rectangles
    private SpriteFont _font; // For displaying score (we'll create a fallback if not available)
    
    // ===== PLAYER PROPERTIES =====
    private Vector2 _playerPosition;
    private Vector2 _playerVelocity;
    private Rectangle _playerBounds;
    private const float PlayerWidth = 32f;
    private const float PlayerHeight = 48f;
    private const float PlayerSpeed = 200f;
    private const float JumpForce = -450f;
    private const float Gravity = 1200f;
    private bool _isOnGround = false;
    
    // ===== PLATFORMS =====
    private List<Rectangle> _platforms;
    
    // ===== COINS =====
    private List<Rectangle> _coins;
    private int _score = 0;
    
    // ===== ENEMY =====
    private Vector2 _enemyPosition;
    private float _enemySpeed = 80f;
    private float _enemyDirection = 1f; // 1 = right, -1 = left
    private Rectangle _enemyBounds;
    private const float EnemyWidth = 32f;
    private const float EnemyHeight = 32f;
    
    // ===== CAMERA =====
    private Vector2 _cameraPosition;
    
    // ===== GAME WORLD =====
    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // Set window size
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
    }

    protected override void Initialize()
    {
        // Initialize player at starting position
        _playerPosition = new Vector2(100, 300);
        _playerVelocity = Vector2.Zero;
        
        // Initialize platforms (ground and floating platforms)
        _platforms = new List<Rectangle>
        {
            // Ground platform
            new Rectangle(0, 600, 2000, 120),
            
            // Floating platforms
            new Rectangle(300, 500, 200, 30),
            new Rectangle(600, 400, 200, 30),
            new Rectangle(900, 300, 200, 30),
            new Rectangle(1200, 450, 200, 30),
            new Rectangle(1500, 350, 200, 30),
        };
        
        // Initialize coins
        _coins = new List<Rectangle>
        {
            new Rectangle(350, 450, 20, 20),
            new Rectangle(650, 350, 20, 20),
            new Rectangle(950, 250, 20, 20),
            new Rectangle(1250, 400, 20, 20),
            new Rectangle(1550, 300, 20, 20),
            new Rectangle(500, 550, 20, 20),
            new Rectangle(800, 550, 20, 20),
        };
        
        // Initialize enemy
        _enemyPosition = new Vector2(600, 350);
        
        // Initialize camera
        _cameraPosition = Vector2.Zero;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel texture for drawing colored rectangles
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        
        // Try to load a font, but don't crash if it doesn't exist
        try
        {
            _font = Content.Load<SpriteFont>("Font");
        }
        catch
        {
            // Font not available, we'll draw score without it
            _font = null;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // Exit game with Escape key
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // ===== PLAYER INPUT & MOVEMENT =====
        HandlePlayerInput(deltaTime);
        
        // ===== APPLY GRAVITY =====
        ApplyGravity(deltaTime);
        
        // ===== UPDATE PLAYER POSITION =====
        _playerPosition += _playerVelocity * deltaTime;
        
        // Update player collision bounds
        _playerBounds = new Rectangle(
            (int)_playerPosition.X,
            (int)_playerPosition.Y,
            (int)PlayerWidth,
            (int)PlayerHeight
        );
        
        // ===== COLLISION DETECTION =====
        HandlePlatformCollisions();
        
        // ===== COIN COLLECTION =====
        CollectCoins();
        
        // ===== ENEMY MOVEMENT =====
        UpdateEnemy(deltaTime);
        
        // ===== CAMERA FOLLOW =====
        UpdateCamera();

        base.Update(gameTime);
    }

    /// <summary>
    /// Handles keyboard input for player movement and jumping
    /// </summary>
    private void HandlePlayerInput(float deltaTime)
    {
        KeyboardState keyState = Keyboard.GetState();
        
        // Horizontal movement (left/right arrow keys)
        if (keyState.IsKeyDown(Keys.Left))
        {
            _playerVelocity.X = -PlayerSpeed;
        }
        else if (keyState.IsKeyDown(Keys.Right))
        {
            _playerVelocity.X = PlayerSpeed;
        }
        else
        {
            // Stop horizontal movement when no key is pressed
            _playerVelocity.X = 0;
        }
        
        // Jumping (spacebar) - only when on ground
        if (keyState.IsKeyDown(Keys.Space) && _isOnGround)
        {
            _playerVelocity.Y = JumpForce;
            _isOnGround = false; // Player is now in the air
        }
    }

    /// <summary>
    /// Applies gravity to pull the player downward
    /// </summary>
    private void ApplyGravity(float deltaTime)
    {
        // Add gravity to vertical velocity
        _playerVelocity.Y += Gravity * deltaTime;
        
        // Cap falling speed to prevent going too fast
        if (_playerVelocity.Y > 600f)
        {
            _playerVelocity.Y = 600f;
        }
    }

    /// <summary>
    /// Checks and handles collisions between player and platforms
    /// </summary>
    private void HandlePlatformCollisions()
    {
        _isOnGround = false;
        
        foreach (Rectangle platform in _platforms)
        {
            // Check if player intersects with platform
            if (_playerBounds.Intersects(platform))
            {
                // Calculate overlap on each axis
                int overlapLeft = _playerBounds.Right - platform.Left;
                int overlapRight = platform.Right - _playerBounds.Left;
                int overlapTop = _playerBounds.Bottom - platform.Top;
                int overlapBottom = platform.Bottom - _playerBounds.Top;
                
                // Find the smallest overlap (this is the collision direction)
                int minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), 
                                         Math.Min(overlapTop, overlapBottom));
                
                // Resolve collision based on direction
                if (minOverlap == overlapTop && _playerVelocity.Y > 0)
                {
                    // Collision from top (player landing on platform)
                    _playerPosition.Y = platform.Top - PlayerHeight;
                    _playerVelocity.Y = 0;
                    _isOnGround = true;
                }
                else if (minOverlap == overlapBottom && _playerVelocity.Y < 0)
                {
                    // Collision from bottom (player hitting head)
                    _playerPosition.Y = platform.Bottom;
                    _playerVelocity.Y = 0;
                }
                else if (minOverlap == overlapLeft)
                {
                    // Collision from left
                    _playerPosition.X = platform.Left - PlayerWidth;
                }
                else if (minOverlap == overlapRight)
                {
                    // Collision from right
                    _playerPosition.X = platform.Right;
                }
                
                // Update bounds after position correction
                _playerBounds.X = (int)_playerPosition.X;
                _playerBounds.Y = (int)_playerPosition.Y;
            }
        }
    }

    /// <summary>
    /// Checks if player collects any coins and updates score
    /// </summary>
    private void CollectCoins()
    {
        for (int i = _coins.Count - 1; i >= 0; i--)
        {
            if (_playerBounds.Intersects(_coins[i]))
            {
                _coins.RemoveAt(i); // Remove collected coin
                _score += 10; // Increase score
            }
        }
    }

    /// <summary>
    /// Updates enemy position with simple patrol movement
    /// </summary>
    private void UpdateEnemy(float deltaTime)
    {
        // Move enemy back and forth
        _enemyPosition.X += _enemySpeed * _enemyDirection * deltaTime;
        
        // Reverse direction at boundaries (patrol between x: 500 and x: 800)
        if (_enemyPosition.X > 800)
        {
            _enemyDirection = -1f;
        }
        else if (_enemyPosition.X < 500)
        {
            _enemyDirection = 1f;
        }
        
        // Update enemy bounds
        _enemyBounds = new Rectangle(
            (int)_enemyPosition.X,
            (int)_enemyPosition.Y,
            (int)EnemyWidth,
            (int)EnemyHeight
        );
    }

    /// <summary>
    /// Updates camera to follow the player smoothly
    /// </summary>
    private void UpdateCamera()
    {
        // Camera follows player horizontally, centered on screen
        float targetX = _playerPosition.X - ScreenWidth / 2 + PlayerWidth / 2;
        
        // Smooth camera movement (lerp)
        _cameraPosition.X = MathHelper.Lerp(_cameraPosition.X, targetX, 0.1f);
        
        // Keep camera within world bounds
        _cameraPosition.X = MathHelper.Clamp(_cameraPosition.X, 0, 2000 - ScreenWidth);
        
        // Keep camera Y fixed (no vertical following)
        _cameraPosition.Y = 0;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Create camera transformation matrix
        Matrix cameraTransform = Matrix.CreateTranslation(-_cameraPosition.X, -_cameraPosition.Y, 0);
        
        // Begin drawing with camera transform
        _spriteBatch.Begin(transformMatrix: cameraTransform);
        
        // ===== DRAW PLATFORMS =====
        foreach (Rectangle platform in _platforms)
        {
            DrawRectangle(platform, Color.SaddleBrown);
        }
        
        // ===== DRAW COINS =====
        foreach (Rectangle coin in _coins)
        {
            DrawRectangle(coin, Color.Gold);
        }
        
        // ===== DRAW ENEMY =====
        DrawRectangle(_enemyBounds, Color.Red);
        
        // ===== DRAW PLAYER =====
        DrawRectangle(_playerBounds, Color.Green);
        
        _spriteBatch.End();
        
        // ===== DRAW UI (without camera transform) =====
        _spriteBatch.Begin();
        
        // Draw score
        string scoreText = $"Score: {_score}";
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, scoreText, new Vector2(10, 10), Color.White);
        }
        else
        {
            // Fallback: draw simple text representation
            DrawRectangle(new Rectangle(10, 10, 100, 30), Color.Black * 0.5f);
        }
        
        // Draw controls hint
        string controls = "Arrow Keys: Move | Space: Jump | ESC: Exit";
        if (_font != null)
        {
            _spriteBatch.DrawString(_font, controls, new Vector2(10, 40), Color.White);
        }
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>
    /// Helper method to draw a filled rectangle with a specific color
    /// </summary>
    private void DrawRectangle(Rectangle rect, Color color)
    {
        _spriteBatch.Draw(_pixelTexture, rect, color);
    }
}
