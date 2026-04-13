using Microsoft.Xna.Framework;
using System;

namespace MiniMario;

/// <summary>
/// Camera system with smooth following and parallax support
/// </summary>
public class Camera
{
    public Vector2 Position;
    private Vector2 _targetPosition;
    private Rectangle _worldBounds;
    
    /// <summary>
    /// Initializes camera with world boundaries

    public void Initialize(int worldWidth, int worldHeight)
    {
        _worldBounds = new Rectangle(0, 0, worldWidth, worldHeight);
        Position = Vector2.Zero;
    }
    
    /// <summary>
    /// Updates camera to follow target (player) smoothly
    /// </summary>
    public void Follow(Vector2 targetPosition, float deltaTime)
    {
        // Calculate target camera position (center player on screen)
        _targetPosition.X = targetPosition.X - Constants.ScreenWidth / 2 + Constants.PlayerWidth / 2;
        _targetPosition.Y = targetPosition.Y - Constants.ScreenHeight / 2 + Constants.PlayerHeight / 2;
        
        // Add look-ahead based on player velocity (optional enhancement)
        // _targetPosition.X += playerVelocity.X * Constants.CameraLookAhead;
        
        // Smooth camera movement (lerp)
        Position.X = MathHelper.Lerp(Position.X, _targetPosition.X, Constants.CameraLerpSpeed * deltaTime);
        Position.Y = MathHelper.Lerp(Position.Y, _targetPosition.Y, Constants.CameraLerpSpeed * deltaTime);
        
        // Clamp camera to world bounds
        Position.X = MathHelper.Clamp(Position.X, 0, Math.Max(0, _worldBounds.Width - Constants.ScreenWidth));
        Position.Y = MathHelper.Clamp(Position.Y, 0, Math.Max(0, _worldBounds.Height - Constants.ScreenHeight));
    }
    
    /// <summary>
    /// Gets transformation matrix for rendering
    /// </summary>
    public Matrix GetTransformMatrix()
    {
        return Matrix.CreateTranslation(-Position.X, -Position.Y, 0);
    }
    
    /// <summary>
    /// Gets parallax offset for background layers
    /// </summary>..
    public Vector2 GetParallaxOffset(float parallaxFactor)
    {
        return new Vector2(Position.X * parallaxFactor, Position.Y * parallaxFactor);
    }
}
