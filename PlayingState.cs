// import lib and playong state 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace MiniMario;

/// <summary>
/// Main gameplay state
/// </summary>
public class PlayingState : IGameState
{
    private GameManager _gameManager;
    private Player _player;
    private TileMap _tileMap;
    private Camera _camera;
    private HUD _hud;
    private List<Enemy> _enemies;
    private int _currentLevel = 1;
    private bool _isPaused = false;
    private float _coinAnimationTimer = 0;
    
    public PlayingState(GameManager gameManager)
    {
        _gameManager = gameManager;
        _player = new Player();
        _tileMap = new TileMap();
        _camera = new Camera();
        _hud = new HUD(_gameManager.Textures["pixel"]);
        _enemies = new List<Enemy>();
    }
    
    public void OnEnter()
    {
        // Load level
        LoadLevel(_currentLevel);
        _isPaused = false;
    }
    
    public void OnExit()
    {
    }
    
    /// <summary>
    /// Loads a level and initializes entities
    /// </summary>
    private void LoadLevel(int levelIndex)
    {
        _tileMap.LoadLevel(levelIndex);
        _player.Initialize(_tileMap.SpawnPoint);
        _camera.Initialize(_tileMap.PixelWidth, _tileMap.PixelHeight);
        
        // Spawn enemies
        SpawnEnemies();
    }
    
    /// <summary>
    /// Spawns enemies at designated spawn points
    /// </summary>
    private void SpawnEnemies()
    {
        _enemies.Clear();
        
        for (int y = 0; y < _tileMap.Height; y++)
        {
            for (int x = 0; x < _tileMap.Width; x++)
            {
                if (_tileMap.GetTileAt(x, y) == Constants.TileEnemySpawn)
                {
                    Vector2 spawnPos = new Vector2(x * Constants.TileSize, y * Constants.TileSize);
                    
                    // Alternate between walker and jumper
                    if (_enemies.Count % 2 == 0)
                    {
                        _enemies.Add(new WalkerEnemy(spawnPos));
                    }
                    else
                    {
                        _enemies.Add(new JumperEnemy(spawnPos));
                    }
                }
            }
        }
    }
    
    public void Update(GameTime gameTime)
    {
        // Check for pause
        if (_gameManager.Input.IsKeyPressed(Keys.Escape))
        {
            _isPaused = !_isPaused;
        }
        
        if (_isPaused)
        {
            return;
        }
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Update player
        _player.Update(gameTime, _gameManager.Input, _tileMap);
        
        // Update enemies
        foreach (var enemy in _enemies)
        {
            enemy.Update(gameTime, _tileMap);
            
            // Update jumper enemy with player position
            if (enemy is JumperEnemy jumper)
            {
                jumper.SetPlayerPosition(_player.Position);
            }
        }
        
        // Check player-enemy collisions
        CheckEnemyCollisions();
        
        // Update camera
        _camera.Follow(_player.Position, deltaTime);
        
        // Update coin animation
        _coinAnimationTimer += deltaTime;
        
        // Check for level completion
        CheckLevelCompletion();
        
        // Check for game over
        if (!_player.IsAlive)
        {
            _gameManager.ChangeState("gameover");
        }
    }
    
    /// <summary>
    /// Checks collisions between player and enemies
    /// </summary>
    private void CheckEnemyCollisions()
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            if (!enemy.IsAlive) continue;
            
            if (_player.Bounds.Intersects(enemy.Bounds))
            {
                // Check if player is stomping (coming from above)
                if (_player.Velocity.Y > 0 && _player.Bounds.Bottom - 10 < enemy.Bounds.Top + 15)
                {
                    // Stomp enemy
                    enemy.TakeDamage(1);
                    _player.StompEnemy();
                    
                    if (!enemy.IsAlive)
                    {
                        _enemies.RemoveAt(i);
                    }
                }
                else
                {
                    // Player takes damage
                    _player.TakeDamage();
                }
            }
        }
    }
    
    /// <summary>
    /// Checks if player reached level end
    /// </summary>
    private void CheckLevelCompletion()
    {
        if (_tileMap.LevelEndPosition != Vector2.Zero)
        {
            Rectangle levelEndBounds = new Rectangle(
                (int)_tileMap.LevelEndPosition.X,
                (int)_tileMap.LevelEndPosition.Y,
                Constants.TileSize,
                Constants.TileSize
            );
            
            if (_player.Bounds.Intersects(levelEndBounds))
            {
                // Go to next level
                _currentLevel++;
                if (_currentLevel > 3)
                {
                    // Game completed!
                    _gameManager.UpdateHighScore(_player.Score);
                    _gameManager.ChangeState("gameover");
                }
                else
                {
                    LoadLevel(_currentLevel);
                }
            }
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw background
        DrawBackground(spriteBatch);
        
        // Draw world with camera transform
        Matrix cameraTransform = _camera.GetTransformMatrix();
        spriteBatch.Begin(transformMatrix: cameraTransform, samplerState: SamplerState.PointClamp);
        
        // Draw tilemap
        _tileMap.Draw(spriteBatch, _camera.Position, _gameManager.Textures);
        
        // Draw enemies
        foreach (var enemy in _enemies)
        {
            enemy.Draw(spriteBatch, _gameManager.Textures["pixel"]);
        }
        
        // Draw player
        _player.Draw(spriteBatch, _gameManager.Textures["pixel"]);
        
        spriteBatch.End();
        
        // Draw HUD (no camera transform)
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _hud.Draw(spriteBatch, _player, _currentLevel);
        
        // Draw pause overlay
        if (_isPaused)
        {
            DrawPauseOverlay(spriteBatch);
        }
        
        spriteBatch.End();
    }
    
    /// <summary>
    /// Draws parallax background layers
    /// </summary>
    private void DrawBackground(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        
        // Sky gradient
        Rectangle sky = new Rectangle(0, 0, Constants.ScreenWidth, Constants.ScreenHeight);
        spriteBatch.Draw(_gameManager.Textures["gradient"], sky, Color.White);
        
        // Parallax stars (3 layers at different speeds)
        DrawStarLayer(spriteBatch, 0.1f, 50, 42);
        DrawStarLayer(spriteBatch, 0.3f, 30, 123);
        DrawStarLayer(spriteBatch, 0.5f, 20, 456);
        
        spriteBatch.End();
    }
    
    /// <summary>
    /// Draws a parallax star layer
    /// </summary>
    private void DrawStarLayer(SpriteBatch spriteBatch, float parallaxFactor, int starCount, int seed)
    {
        Vector2 parallaxOffset = _camera.GetParallaxOffset(parallaxFactor);
        Random random = new Random(seed);
        
        for (int i = 0; i < starCount; i++)
        {
            float x = random.Next(_tileMap.PixelWidth);
            float y = random.Next(_tileMap.PixelHeight);
            int size = random.Next(2, 4);
            
            // Apply parallax
            float screenX = (x - parallaxOffset.X) % Constants.ScreenWidth;
            if (screenX < 0) screenX += Constants.ScreenWidth;
            
            Rectangle star = new Rectangle((int)screenX, (int)y, size, size);
            spriteBatch.Draw(_gameManager.Textures["pixel"], star, Color.White * 0.6f);
        }
    }
    
    /// <summary>
    /// Draws pause overlay
    /// </summary>
    private void DrawPauseOverlay(SpriteBatch spriteBatch)
    {
        // Semi-transparent overlay
        Rectangle overlay = new Rectangle(0, 0, Constants.ScreenWidth, Constants.ScreenHeight);
        spriteBatch.Draw(_gameManager.Textures["pixel"], overlay, Color.Black * 0.7f);
        
        // "PAUSED" text
        DrawCenteredText(spriteBatch, "PAUSED", Constants.ScreenHeight / 2 - 50, Color.White, 3);
        DrawCenteredText(spriteBatch, "Press ESC to Resume", Constants.ScreenHeight / 2 + 20, Color.LightGray, 1);
        DrawCenteredText(spriteBatch, "Press M for Menu", Constants.ScreenHeight / 2 + 50, Color.LightGray, 1);
        
        // Check for menu
        if (_gameManager.Input.IsKeyPressed(Keys.M))
        {
            _gameManager.UpdateHighScore(_player.Score);
            _currentLevel = 1;
            _gameManager.ChangeState("menu");
        }
    }
    
    /// <summary>
    /// Draws centered text with scale
    /// </summary>
    private void DrawCenteredText(SpriteBatch spriteBatch, string text, int y, Color color, int scale)
    {
        int charWidth = 10 * scale;
        int totalWidth = text.Length * charWidth;
        int x = (Constants.ScreenWidth - totalWidth) / 2;
        
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ') continue;
            
            Vector2 charPos = new Vector2(x + i * charWidth, y);
            Rectangle charRect = new Rectangle((int)charPos.X, (int)charPos.Y, 8 * scale, 14 * scale);
            
            spriteBatch.Draw(_gameManager.Textures["pixel"], charRect, color);
        }
    }
    
    /// <summary>
    /// Resets the current level
    /// </summary>
    public void RestartLevel()
    {
        LoadLevel(_currentLevel);
    }
    
    /// <summary>
    /// Resets to level 1
    /// </summary>
    public void RestartGame()
    {
        _currentLevel = 1;
        _player.Lives = Constants.PlayerMaxLives;
        _player.Score = 0;
        _player.CoinsCollected = 0;
        LoadLevel(_currentLevel);
    }
}
