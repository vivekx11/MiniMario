using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MiniMario;

/// <summary>
/// Game over state showing final score and options
/// </summary>
public class GameOverState : IGameState
{
    private GameManager _gameManager;
    private int _finalScore;
    private bool _isNewHighScore;
    private float _animationTimer;
    
    public GameOverState(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    
    public void OnEnter()
    {
        _animationTimer = 0;
    }
    
    public void OnExit()
    {
    }
    
    /// <summary>
    /// Sets the final score for display
    /// </summary>
    public void SetFinalScore(int score)
    {
        _finalScore = score;
        _isNewHighScore = score > _gameManager.HighScore;
    }
    
    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _animationTimer += deltaTime;
        
        // Check for restart
        if (_gameManager.Input.IsKeyPressed(Keys.R))
        {
            _gameManager.ChangeState("playing");
        }
        
        // Check for menu
        if (_gameManager.Input.IsKeyPressed(Keys.M))
        {
            _gameManager.ChangeState("menu");
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw background
        Rectangle background = new Rectangle(0, 0, Constants.ScreenWidth, Constants.ScreenHeight);
        spriteBatch.Draw(_gameManager.Textures["gradient"], background, Color.White);
        
        // Draw "GAME OVER" title
        DrawCenteredText(spriteBatch, "GAME OVER", Constants.ScreenHeight / 2 - 150, Color.Red, 4);
        
        // Draw final score
        DrawCenteredText(spriteBatch, $"FINAL SCORE: {_finalScore}", 
            Constants.ScreenHeight / 2 - 50, Color.White, 2);
        
        // Draw new high score message (if applicable)
        if (_isNewHighScore)
        {
            // Blinking effect
            if ((int)(_animationTimer * 3) % 2 == 0)
            {
                DrawCenteredText(spriteBatch, "NEW HIGH SCORE!", 
                    Constants.ScreenHeight / 2, Color.Gold, 2);
            }
        }
        
        // Draw options
        DrawCenteredText(spriteBatch, "Press R to Restart", 
            Constants.ScreenHeight / 2 + 80, Color.LightGray, 1);
        
        DrawCenteredText(spriteBatch, "Press M for Menu", 
            Constants.ScreenHeight / 2 + 120, Color.LightGray, 1);
        
        // Draw high score
        DrawCenteredText(spriteBatch, $"High Score: {_gameManager.HighScore}", 
            Constants.ScreenHeight - 80, Color.Gold, 1);
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
            
            // Add wave animation
            float wave = (float)Math.Sin(_animationTimer * 2 + i * 0.3f) * 5 * scale;
            charPos.Y += wave;
            //Object
            Rectangle charRect = new Rectangle((int)charPos.X, (int)charPos.Y, 8 * scale, 14 * scale);
            
            // Draw shadow
            Rectangle shadow = charRect;
            shadow.X += 2;
            shadow.Y += 2;
            spriteBatch.Draw(_gameManager.Textures["pixel"], shadow, Color.Black * 0.5f);
            
            // Draw character
            spriteBatch.Draw(_gameManager.Textures["pixel"], charRect, color);
        }
    }
}
