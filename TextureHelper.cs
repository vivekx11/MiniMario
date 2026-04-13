using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniMario;

/// <summary>
/// Helper class for creating simple textures programmatically
/// </summary>
public static class TextureHelper
{
    /// <summary>
    /// Creates a 1x1 solid color texture
    /// </summary>
    public static Texture2D CreateSolidTexture(GraphicsDevice graphicsDevice, Color color)
    {
        Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new[] { color });
        return texture;
    }
    
    /// <summary>
    /// Creates a rectangle texture with a border
    /// </summary>
    public static Texture2D CreateBorderedRectangle(GraphicsDevice graphicsDevice, int width, int height, Color fillColor, Color borderColor, int borderWidth = 2)
    {
        Texture2D texture = new Texture2D(graphicsDevice, width, height);
        Color[] data = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Check if pixel is on border
                bool isBorder = x < borderWidth || x >= width - borderWidth || 
                               y < borderWidth || y >= height - borderWidth;
                
                data[y * width + x] = isBorder ? borderColor : fillColor;
            }
        }
        
        texture.SetData(data);
        return texture;
    }
    
    /// <summary>
    /// Creates a gradient texture from top to bottom
    /// </summary>
    public static Texture2D CreateGradient(GraphicsDevice graphicsDevice, int width, int height, Color topColor, Color bottomColor)
    {
        Texture2D texture = new Texture2D(graphicsDevice, width, height);
        Color[] data = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            float t = (float)y / height;
            Color color = Color.Lerp(topColor, bottomColor, t);
            
            for (int x = 0; x < width; x++)
            {
                // entry level
                data[y * width + x] = color;
            }
        }
        
        texture.SetData(data);
        return texture;
    }
}
