using Microsoft.Xna.Framework.Input;

namespace MiniMario;

/// <summary>
/// Helper class for detecting key press events (not just held keys)
/// </summary>
public class InputHelper
{
    private KeyboardState _previousKeyboardState;
    private KeyboardState _currentKeyboardState;
    
    /// <summary>
    /// Updates input state - call once per frame
    /// </summary>
    public void Update()
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();
    }
    
    /// <summary>
    /// Returns true if key is currently held down
    /// </summary>
    public bool IsKeyDown(Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key);
    }
    
    /// <summary>
    /// Returns true only on the frame the key was first pressed
    /// </summary>
    public bool IsKeyPressed(Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }
    
    /// <summary>
    /// Returns true only on the frame the key was released
    /// </summary>
    public bool IsKeyReleased(Keys key)
    {
        // input 
        return !_currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);
    }
}
