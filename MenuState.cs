using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MiniMario;

/// <summary>
/// Main menu game state
/// </summary>
public class MenuState : IGameState
{
    private GameManager _gameManager;
    private float _blinkTimer;
    private bool _showText = true;
    private float _parallaxOffset;
    
    public MenuState(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    
    public void OnEnter()
    {
        _blinkTimer = 0;
        _showText = true;
        _parallaxOffset = 0;
    }
    
    public void OnExit()
    {
    }
    
    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Update blink timer for "Press ENTER" text
        _blinkTimer += deltaTime;
        if (_blinkTimer > 0.5f)
        {
            _showText = !_showText;
            _blinkTimer = 0;
        }
        
        // Animate parallax background
        _parallaxOffset += 20f * deltaTime;
        if (_parallaxOffset > Constants.ScreenWidth)
        {
            _parallaxOffset = 0;
        }
        
        // Check for start input
        if (_gameManager.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
        {
            _gameManager.ChangeState("playing");
        }
        
        // Check for exit
        if (_gameManager.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            _gameManager.Exit();
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw animated background
        DrawBackground(spriteBatch);
        
        // Draw title
        DrawTitle(spriteBatch);
        
        // Draw "Press ENTER to Start" (blinking)
        if (_showText)
        {
            DrawCenteredText(spriteBatch, "PRESS ENTER TO START", 
                Constants.ScreenHeight / 2 + 50, Color.White);
        }
        
        // Draw high score
        DrawCenteredText(spriteBatch, $"HIGH SCORE: {_gameManager.HighScore}", 
            Constants.ScreenHeight / 2 + 100, Color.Gold);
        
        // Draw controls
        DrawCenteredText(spriteBatch, "CONTROLS: A/D or ARROWS = Move  |  SPACE/W = Jump", 
            Constants.ScreenHeight - 100, Color.LightGray);
        
        DrawCenteredText(spriteBatch, "ESC = Pause/Menu  |  R = Restart", 
            Constants.ScreenHeight - 60, Color.LightGray);
    }
    
    /// <summary>
    /// Draws animated parallax background
    /// </summary>
    private void DrawBackground(SpriteBatch spriteBatch)
    {
        // Dark blue gradient sky
        Rectangle sky = new Rectangle(0, 0, Constants.ScreenWidth, Constants.ScreenHeight);
        spriteBatch.Draw(_gameManager.Textures["gradient"], sky, Color.White);
        
        // Draw stars (simple white dots)
        Random random = new Random(42); // Fixed seed for consistent stars
        for (int i = 0; i < 100; i++)
        {
            int x = random.Next(Constants.ScreenWidth);
            int y = random.Next(Constants.ScreenHeight);
            int size = random.Next(1, 3);
            
            // Parallax effect
            float parallaxX = (x + _parallaxOffset * 0.5f) % Constants.ScreenWidth;
            
            Rectangle star = new Rectangle((int)parallaxX, y, size, size);
            spriteBatch.Draw(_gameManager.Textures["pixel"], star, Color.White * 0.8f);
        }
    }
    
    /// <summary>
    /// Draws game title
    /// </summary>
    private void DrawTitle(SpriteBatch spriteBatch)
    {
        string title = "MINI MARIO";
        int charWidth = 40;
        int charHeight = 60;
        int totalWidth = title.Length * charWidth;
        int startX = (Constants.ScreenWidth - totalWidth) / 2;
        int startY = Constants.ScreenHeight / 2 - 150;
        
        for (int i = 0; i < title.Length; i++)
        {
            Vector2 charPos = new Vector2(startX + i * charWidth, startY);
            
            // Add bounce animation
            float bounce = (float)Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 3 + i * 0.5f) * 10;
            charPos.Y += bounce;
            
            // Draw character shadow
            DrawLargeChar(spriteBatch, title[i], charPos + new Vector2(3, 3), Color.Black * 0.5f, charWidth, charHeight);
            
            // Draw character
            Color charColor = i % 2 == 0 ? Color.Red : Color.Blue;
            DrawLargeChar(spriteBatch, title[i], charPos, charColor, charWidth, charHeight);
        }
    }
    
    /// <summary>
    /// Draws a large character for the title
    /// </summary>
    private void DrawLargeChar(SpriteBatch spriteBatch, char c, Vector2 position, Color color, int width, int height)
    {
        if (c == ' ') return;
        
        Texture2D pixel = _gameManager.Textures["pixel"];
        
        // Draw simple block letter
        Rectangle outline = new Rectangle((int)position.X, (int)position.Y, width, height);
        spriteBatch.Draw(pixel, outline, color);
        
        // Inner detail
        Rectangle inner = new Rectangle((int)position.X + 4, (int)position.Y + 4, width - 8, height - 8);
        spriteBatch.Draw(pixel, inner, color * 0.7f);
    }
    
    /// <summary>
    /// Draws centered text
    /// </summary>
    private void DrawCenteredText(SpriteBatch spriteBatch, string text, int y, Color color)
    {
        int charWidth = 10;
        int totalWidth = text.Length * charWidth;
        int x = (Constants.ScreenWidth - totalWidth) / 2;
        
        DrawSimpleText(spriteBatch, text, new Vector2(x, y), color);
    }
    
    /// <summary>
    /// Draws simple text
    /// </summary>
    private void DrawSimpleText(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
    {
        int charWidth = 10;
        int charHeight = 14;
        
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ') continue;
            
            Vector2 charPos = position + new Vector2(i * charWidth, 0);
            Rectangle charRect = new Rectangle((int)charPos.X, (int)charPos.Y, 8, charHeight);
            
            spriteBatch.Draw(_gameManager.Textures["pixel"], charRect, color);
            
            // Add detail
            Rectangle detail = new Rectangle((int)charPos.X + 2, (int)charPos.Y + 2, 4, charHeight - 4);
            spriteBatch.Draw(_gameManager.Textures["pixel"], detail, color * 0.6f);
        }
    }
}
