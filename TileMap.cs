using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MiniMario;

/// <summary>
/// Manages the tile-based level layout, collision detection, and rendering
/// </summary>
public class TileMap
{
    private int[,] _tiles;
    private int _width;
    private int _height;
    public Vector2 SpawnPoint { get; private set; }
    public Vector2 CheckpointPosition { get; private set; }
    public Vector2 LevelEndPosition { get; private set; }
    
    public int Width => _width;
    public int Height => _height;
    public int PixelWidth => _width * Constants.TileSize;
    public int PixelHeight => _height * Constants.TileSize;
    
    /// <summary>
    /// Loads a level from a 2D array of tile types
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        int[,] levelData = GetLevelData(levelIndex);
        _tiles = levelData;
        _height = levelData.GetLength(0);
        _width = levelData.GetLength(1);
        
        // Find spawn point and special tiles ..
        FindSpecialTiles();
    }
    
    /// <summary>
    /// Finds spawn points, checkpoints, and level end markers
    /// </summary>
    private void FindSpecialTiles()
    {
        SpawnPoint = new Vector2(100, 100); // Default
        CheckpointPosition = Vector2.Zero;
        LevelEndPosition = Vector2.Zero;
        
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (_tiles[y, x] == Constants.TileCheckpoint)
                {
                    CheckpointPosition = new Vector2(x * Constants.TileSize, y * Constants.TileSize);
                }
                else if (_tiles[y, x] == Constants.TileLevelEnd)
                {
                    LevelEndPosition = new Vector2(x * Constants.TileSize, y * Constants.TileSize);
                }
            }
        }
        
        // Find first empty space above ground for spawn
        for (int x = 2; x < _width; x++)
        {
            for (int y = _height - 1; y >= 0; y--)
            {
                if (_tiles[y, x] == Constants.TileGround && y > 0 && _tiles[y - 1, x] == Constants.TileEmpty)
                {
                    SpawnPoint = new Vector2(x * Constants.TileSize, (y - 2) * Constants.TileSize);
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the tile type at grid coordinates
    /// </summary>
    public int GetTileAt(int gridX, int gridY)
    {
        if (gridX < 0 || gridX >= _width || gridY < 0 || gridY >= _height)
            return Constants.TileEmpty;
        
        return _tiles[gridY, gridX];
    }
    
    /// <summary>
    /// Gets the tile type at world pixel coordinates
    /// </summary>
    public int GetTileAtPosition(float worldX, float worldY)
    {
        int gridX = (int)(worldX / Constants.TileSize);
        int gridY = (int)(worldY / Constants.TileSize);
        return GetTileAt(gridX, gridY);
    }
    
    /// <summary>
    /// Checks if a tile is solid (blocks movement)
    /// </summary>
    public bool IsSolid(int gridX, int gridY)
    {
        int tile = GetTileAt(gridX, gridY);
        return tile == Constants.TileGround;
    }
    
    /// <summary>
    /// Checks if a tile is a one-way platform
    /// </summary>
    public bool IsPlatform(int gridX, int gridY)
    {
        // platform based 
        return GetTileAt(gridX, gridY) == Constants.TilePlatform;
    }
    
    /// <summary>
    /// Removes a coin tile at the specified position
    /// </summary>
    public void RemoveCoin(int gridX, int gridY)
    {
        if (gridX >= 0 && gridX < _width && gridY >= 0 && gridY < _height)
        {
            if (_tiles[gridY, gridX] == Constants.TileCoin)
            {
                _tiles[gridY, gridX] = Constants.TileEmpty;
            }
        }
    }
    
    /// <summary>
    /// Resolves collision between an entity and the tilemap
    /// </summary>
    public void ResolveCollision(Rectangle bounds, ref Vector2 velocity, ref bool isGrounded, bool canUsePlatforms = true)
    {
        isGrounded = false;
        
        // Get tile range that entity overlaps
        int leftTile = bounds.Left / Constants.TileSize;
        int rightTile = (bounds.Right - 1) / Constants.TileSize;
        int topTile = bounds.Top / Constants.TileSize;
        int bottomTile = (bounds.Bottom - 1) / Constants.TileSize;
        
        // Check horizontal collisions
        for (int y = topTile; y <= bottomTile; y++)
        {
            // Check left side
            if (IsSolid(leftTile, y))
            {
                velocity.X = 0;
                bounds.X = (leftTile + 1) * Constants.TileSize;
            }
            // Check right side
            if (IsSolid(rightTile, y))
            {
                velocity.X = 0;
                bounds.X = rightTile * Constants.TileSize - bounds.Width;
            }
        }
        
        // Check vertical collisions
        for (int x = leftTile; x <= rightTile; x++)
        {
            // Check bottom (landing on ground)
            if (velocity.Y >= 0 && (IsSolid(x, bottomTile) || (canUsePlatforms && IsPlatform(x, bottomTile))))
            {
                // Only collide with platforms if coming from above
                if (IsPlatform(x, bottomTile))
                {
                    int platformTop = bottomTile * Constants.TileSize;
                    if (bounds.Bottom - velocity.Y <= platformTop + 8) // Small threshold
                    {
                        velocity.Y = 0;
                        bounds.Y = platformTop - bounds.Height;
                        isGrounded = true;
                    }
                }
                else
                {
                    velocity.Y = 0;
                    bounds.Y = bottomTile * Constants.TileSize - bounds.Height;
                    isGrounded = true;
                }
            }
            
            // Check top (hitting ceiling)
            if (velocity.Y < 0 && IsSolid(x, topTile))
            {
                velocity.Y = 0;
                bounds.Y = (topTile + 1) * Constants.TileSize;
            }
        }
    }
    
    /// <summary>
    /// Draws the tilemap with camera offset
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Vector2 cameraOffset, Dictionary<string, Texture2D> textures)
    {
        // Calculate visible tile range for optimization
        int startX = (int)(cameraOffset.X / Constants.TileSize) - 1;
        int endX = startX + (Constants.ScreenWidth / Constants.TileSize) + 2;
        int startY = (int)(cameraOffset.Y / Constants.TileSize) - 1;
        int endY = startY + (Constants.ScreenHeight / Constants.TileSize) + 2;
        
        startX = System.Math.Max(0, startX);
        endX = System.Math.Min(_width - 1, endX);
        startY = System.Math.Max(0, startY);
        endY = System.Math.Min(_height - 1, endY);
        
        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                int tile = _tiles[y, x];
                if (tile == Constants.TileEmpty) continue;
                
                Rectangle destRect = new Rectangle(
                    x * Constants.TileSize,
                    y * Constants.TileSize,
                    Constants.TileSize,
                    Constants.TileSize
                );
                
                Texture2D texture = GetTextureForTile(tile, textures);
                Color tint = GetTintForTile(tile);
                
                spriteBatch.Draw(texture, destRect, tint);
            }
        }
    }
    
    /// <summary>
    /// Gets the appropriate texture for a tile type
    /// </summary>
    private Texture2D GetTextureForTile(int tileType, Dictionary<string, Texture2D> textures)
    {
        return tileType switch
        {
            Constants.TileGround => textures["ground"],
            Constants.TilePlatform => textures["platform"],
            Constants.TileSpike => textures["spike"],
            Constants.TileCoin => textures["coin"],
            Constants.TileCheckpoint => textures["checkpoint"],
            Constants.TileLevelEnd => textures["levelEnd"],
            _ => textures["pixel"]
        };
    }
    
    /// <summary>
    /// Gets color tint for tile type
    /// </summary>
    private Color GetTintForTile(int tileType)
    {
        return tileType switch
        {
            Constants.TileGround => Color.White,
            Constants.TilePlatform => Color.White,
            Constants.TileSpike => Color.White,
            Constants.TileCoin => Color.White,
            Constants.TileCheckpoint => Color.LightGreen,
            Constants.TileLevelEnd => Color.Gold,
            _ => Color.White
        };
    }
    
    /// <summary>
    /// Returns level data for the specified level index
    /// </summary>
    private int[,] GetLevelData(int levelIndex)
    {
        return levelIndex switch
        {
            1 => GetLevel1(),
            2 => GetLevel2(),
            3 => GetLevel3(),
            _ => GetLevel1()
        };
    }
    
    /// <summary>
    /// Level 1 - Tutorial: Flat terrain with coins and basic enemies
    /// </summary>
    private int[,] GetLevel1()
    {
        return new int[,]
        {
            // 0=Empty, 1=Ground, 2=Platform, 3=Spike, 4=Coin, 5=EnemySpawn, 6=Checkpoint, 7=LevelEnd
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,4,0,0,4,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        };
    }
    
    /// <summary>
    /// Level 2 - Intermediate: Platforms, gaps, spikes, and multiple enemies
    /// </summary>
    private int[,] GetLevel2()
    {
        return new int[,]
        {
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7},
            {0,0,0,0,0,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        };
    }
    
    /// <summary>
    /// Level 3 - Advanced: Vertical sections, tight platforming, many enemies
    /// </summary>
    private int[,] GetLevel3()
    {
        return new int[,]
        {
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,5,0,0,0,0,5,0,0,0,0,0,0,5,0,0,0,0,0,5,0,0,0,0,0,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        };
    }
}
