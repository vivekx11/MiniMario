using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniMario;

/// <summary>
/// Heads-up display showing score, lives, and other game info
/// 
public class HUD
{
    private Texture2D _pixelTexture;
    
    public HUD(Texture2D pixelTexture)
    {
        _pixelTexture = pixelTexture;
    }
    
    /// <summary>
    /// Draws the HUD overlay
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Player player, int currentLevel)
    {
        // Draw semi-transparent black bar at top
        Rectangle hudBackground = new Rectangle(0, 0, Constants.ScreenWidth, 50);
        spriteBatch.Draw(_pixelTexture, hudBackground, Color.Black * 0.6f);
        
        // Draw score
        DrawText(spriteBatch, $"SCORE: {player.Score}", new Vector2(20, 15), Color.White);
        
        // Draw lives (hearts)
        for (int i = 0; i < player.Lives; i++)
        {
            DrawHeart(spriteBatch, new Vector2(250 + i * 35, 15));
        }
        
        // Draw coins collected
        DrawCoin(spriteBatch, new Vector2(450, 15));
        DrawText(spriteBatch, $"x {player.CoinsCollected}", new Vector2(480, 15), Color.Gold);
        
        // Draw level number
        DrawText(spriteBatch, $"LEVEL {currentLevel}", new Vector2(Constants.ScreenWidth - 150, 15), Color.White);
    }
    
    /// <summary>
    /// Draws a heart icon representing a life
    /// </summary>
    private void DrawHeart(SpriteBatch spriteBatch, Vector2 position)
    {
        // Simple heart shape using rectangles
        Rectangle center = new Rectangle((int)position.X + 4, (int)position.Y + 4, 12, 12);
        Rectangle leftTop = new Rectangle((int)position.X, (int)position.Y + 2, 8, 8);
        Rectangle rightTop = new Rectangle((int)position.X + 12, (int)position.Y + 2, 8, 8);
        Rectangle bottom = new Rectangle((int)position.X + 6, (int)position.Y + 14, 8, 6);
        
        spriteBatch.Draw(_pixelTexture, center, Color.Red);
        spriteBatch.Draw(_pixelTexture, leftTop, Color.Red);
        spriteBatch.Draw(_pixelTexture, rightTop, Color.Red);
        spriteBatch.Draw(_pixelTexture, bottom, Color.Red);
    }
    
    /// <summary>
    /// Draws a coin icon
    /// </summary>
    private void DrawCoin(SpriteBatch spriteBatch, Vector2 position)
    {
        Rectangle coin = new Rectangle((int)position.X, (int)position.Y, 20, 20);
        spriteBatch.Draw(_pixelTexture, coin, Color.Gold);
        
        // Inner detail
        Rectangle inner = new Rectangle((int)position.X + 6, (int)position.Y + 6, 8, 8);
        spriteBatch.Draw(_pixelTexture, inner, Color.Yellow);
    }
    
    /// <summary>
    /// Draws simple pixel text (fallback when no font available)
    /// </summary>
    private void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
    {
        // Simple bitmap-style text rendering
        int charWidth = 8;
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            Vector2 charPos = position + new Vector2(i * charWidth, 0);
            
            // Draw character as simple rectangles (very basic)
            if (c != ' ')
            {
                DrawCharacter(spriteBatch, c, charPos, color);
            }
        }
    }
    
    /// <summary>
    /// Draws a single character using simple pixel patterns
    /// </summary>
    private void DrawCharacter(SpriteBatch spriteBatch, char c, Vector2 position, Color color)
    {
        // Very simplified character rendering - just draw a small rectangle for each char
        // In a real game, you'd use a proper font or bitmap font
        Rectangle charRect = new Rectangle((int)position.X, (int)position.Y, 7, 12);
        
        // Draw character outline
        if (char.IsLetterOrDigit(c) || c == ':' || c == 'x')
        {
            // Vertical bars
            Rectangle left = new Rectangle((int)position.X, (int)position.Y, 2, 12);
            Rectangle right = new Rectangle((int)position.X + 5, (int)position.Y, 2, 12);
            spriteBatch.Draw(_pixelTexture, left, color);
            spriteBatch.Draw(_pixelTexture, right, color);
            
            // Horizontal bars
            Rectangle top = new Rectangle((int)position.X, (int)position.Y, 7, 2);
            Rectangle middle = new Rectangle((int)position.X, (int)position.Y + 5, 7, 2);
            Rectangle bottom = new Rectangle((int)position.X, (int)position.Y + 10, 7, 2);
            spriteBatch.Draw(_pixelTexture, top, color);
            spriteBatch.Draw(_pixelTexture, middle, color * 0.7f);
            spriteBatch.Draw(_pixelTexture, bottom, color);
        }
    }
}
