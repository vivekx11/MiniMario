using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MiniMario;

/// <summary>
/// Main game manager - handles game loop, state management, and resources
/// </summary>
public class GameManager : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // Textures dictionary for all game graphics
    public Dictionary<string, Texture2D> Textures { get; private set; }
    
    // Input helper
    public InputHelper Input { get; private set; }
    
    // Game states
    private Dictionary<string, IGameState> _states;
    private IGameState _currentState;
    private string _currentStateName;
    
    // High score tracking
    public int HighScore { get; private set; }
    
    public GameManager()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // Set window size
        _graphics.PreferredBackBufferWidth = Constants.ScreenWidth;
        _graphics.PreferredBackBufferHeight = Constants.ScreenHeight;
        
        Window.Title = "Mini Mario - Advanced Platformer";
    }
    
    protected override void Initialize()
    {
        Input = new InputHelper();
        Textures = new Dictionary<string, Texture2D>();
        _states = new Dictionary<string, IGameState>();
        
        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Create all textures programmatically
        CreateTextures();
        
        // Initialize game states
        InitializeStates();
        
        // Start with menu state
        ChangeState("menu");
    }
    
    /// <summary>
    /// Creates all textures needed for the game
    /// </summary>
    private void CreateTextures()
    {
        // Basic pixel texture
        Textures["pixel"] = TextureHelper.CreateSolidTexture(GraphicsDevice, Color.White);
        
        // Ground tile
        Textures["ground"] = TextureHelper.CreateBorderedRectangle(
            GraphicsDevice, Constants.TileSize, Constants.TileSize,
            new Color(80, 80, 80), new Color(100, 100, 100), 2);
        
        // Platform tile
        Textures["platform"] = TextureHelper.CreateBorderedRectangle(
            GraphicsDevice, Constants.TileSize, Constants.TileSize / 2,
            new Color(139, 69, 19), new Color(160, 82, 45), 2);
        
        // Spike tile
        Textures["spike"] = CreateSpikeTexture();
        
        // Coin tile
        Textures["coin"] = CreateCoinTexture();
        
        // Checkpoint
        Textures["checkpoint"] = TextureHelper.CreateSolidTexture(GraphicsDevice, Color.LightGreen);
        
        // Level end flag
        Textures["levelEnd"] = TextureHelper.CreateSolidTexture(GraphicsDevice, Color.Gold);
        
        // Background gradient
        Textures["gradient"] = TextureHelper.CreateGradient(
            GraphicsDevice, Constants.ScreenWidth, Constants.ScreenHeight,
            new Color(20, 20, 60), new Color(60, 60, 120));
    }
    
    /// <summary>
    /// Creates a spike texture
    /// </summary>
    private Texture2D CreateSpikeTexture()
    {
        int size = Constants.TileSize;
        Texture2D texture = new Texture2D(GraphicsDevice, size, size);
        Color[] data = new Color[size * size];
        
        // Fill with transparent
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Color.Transparent;
        }
        
        // Draw triangle spikes
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Create upward pointing triangles
                int triangleIndex = x / 8;
                int xInTriangle = x % 8;
                
                // Triangle shape
                if (y > size - 16 && xInTriangle >= 4 - (size - y) / 4 && xInTriangle <= 4 + (size - y) / 4)
                {
                    data[y * size + x] = Color.Red;
                }
            }
        }
        
        texture.SetData(data);
        return texture;
    }
    
    /// <summary>
    /// Creates a coin texture with shine effect
    /// </summary>
    private Texture2D CreateCoinTexture()
    {
        int size = Constants.TileSize;
        Texture2D texture = new Texture2D(GraphicsDevice, size, size);
        Color[] data = new Color[size * size];
        
        int centerX = size / 2;
        int centerY = size / 2;
        int radius = size / 3;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;
                float distance = (float)System.Math.Sqrt(dx * dx + dy * dy);
                
                if (distance < radius)
                {
                    // Gold coin with shine
                    if (distance < radius * 0.5f)
                    {
                        data[y * size + x] = Color.Yellow;
                    }
                    else
                    {
                        data[y * size + x] = Color.Gold;
                    }
                }
                else
                {
                    data[y * size + x] = Color.Transparent;
                }
            }
        }
        
        texture.SetData(data);
        return texture;
    }
    
    /// <summary>
    /// Initializes all game states
    /// </summary>
    private void InitializeStates()
    {
        _states["menu"] = new MenuState(this);
        _states["playing"] = new PlayingState(this);
        _states["gameover"] = new GameOverState(this);
    }
    
    /// <summary>
    /// Changes the current game state
    /// </summary>
    public void ChangeState(string stateName)
    {
        if (!_states.ContainsKey(stateName))
        {
            return;
        }
        
        // Exit current state
        _currentState?.OnExit();
        
        // Enter new state
        _currentState = _states[stateName];
        _currentStateName = stateName;
        _currentState.OnEnter();
        
        // Special handling for playing state
        if (stateName == "playing" && _currentState is PlayingState playingState)
        {
            playingState.RestartGame();
        }
        
        // Special handling for game over state
        if (stateName == "gameover" && _currentState is GameOverState gameOverState)
        {
            if (_states["playing"] is PlayingState playing)
            {
                // Get score from playing state (we'll need to expose this)
                gameOverState.SetFinalScore(0); // Placeholder
            }
        }
    }
    
    /// <summary>
    /// Updates high score if new score is higher
    /// </summary>
    public void UpdateHighScore(int score)
    {
        if (score > HighScore)
        {
            HighScore = score;
        }
        
        // Update game over state with final score
        if (_states["gameover"] is GameOverState gameOverState)
        {
            gameOverState.SetFinalScore(score);
        }
    }
    
    protected override void Update(GameTime gameTime)
    {
        // Update input
        Input.Update();
        
        // Update current state
        _currentState?.Update(gameTime);
        // Update
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 20, 60));
        
        // Draw current state
        _currentState?.Draw(_spriteBatch);
        
        base.Draw(gameTime);
    }
}
