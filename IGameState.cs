// import the file 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniMario;

/// <summary>
/// Interface for game state pattern (Menu, Playing, GameOver, etc.)
/// </summary>
public interface IGameState
{
    /// <summary>
    /// Called every frame to update game logic
    /// </summary>
    void Update(GameTime gameTime);
    
    /// <summary>
    /// Called every frame to render graphics
    /// </summary>
    void Draw(SpriteBatch spriteBatch);
    
    /// <summary>
    /// Called when entering this state
    /// </summary>
    void OnEnter();
    
    /// <summary>
    /// Called when leaving this state
    /// </summary>
    void OnExit();
}
//Exit 
