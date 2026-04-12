# Quick Start Guide - Mini Mario

## Installation (First Time)

1. **Install .NET 8 SDK**
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version`

2. **Navigate to Project**
   ```bash
   cd MiniMario
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

## Running the Game

```bash
dotnet run
```

The game window will open at 1280x720 resolution.

## Controls

### Movement
- **A** or **Left Arrow**: Move left
- **D** or **Right Arrow**: Move right
- **Space**, **W**, or **Up Arrow**: Jump
  - Press again in mid-air for double jump
  - Hold for higher jump, release early for lower jump

### System
- **Enter**: Start game (on menu)
- **ESC**: Pause/Resume game
- **M**: Return to menu (when paused or game over)
- **R**: Restart game (on game over screen)

## Gameplay Tips

### Basic Movement
1. Use A/D or arrow keys to move left and right
2. Press Space to jump
3. You can jump again in mid-air (double jump)
4. Release jump early for precise, short hops

### Combat
- Jump on top of enemies to defeat them (+50 points)
- Avoid touching enemies from the side (you'll lose a life)
- You have 3 lives total
- After taking damage, you're invincible for 1.5 seconds (flashing)

### Collectibles
- Collect gold coins for +10 points each
- Reach the gold flag at the end of each level to progress

### Hazards
- Red spikes cause instant damage
- Falling off the map respawns you at the start

### Advanced Techniques
- **Coyote Time**: You can still jump briefly after walking off a platform
- **Jump Buffering**: Press jump just before landing and it will register
- **Variable Jump**: Hold jump for max height, tap for short hops
- **Enemy Stomp Bounce**: Stomping enemies gives you a small bounce

## Level Progression

### Level 1 - Tutorial
- Learn basic movement and jumping
- Practice collecting coins
- Encounter your first enemy
- Simple, flat terrain

### Level 2 - Intermediate
- Jump across gaps using platforms
- Avoid spike hazards
- Face multiple enemies
- Requires timing and precision

### Level 3 - Advanced
- Vertical climbing challenges
- Tight platforming sections
- Many enemies to avoid or defeat
- Tests all your skills

## Troubleshooting

### Game Won't Start
- Make sure .NET 8 SDK is installed: `dotnet --version`
- Try rebuilding: `dotnet clean` then `dotnet build`
- Check for error messages in the console

### Performance Issues
- Close other applications
- The game runs at 60 FPS by default
- All graphics are generated in code (no external files needed)

### Controls Not Working
- Make sure the game window has focus (click on it)
- Try using different keys (A/D vs Arrow keys)
- ESC key exits pause menu, not the game

### Can't Beat a Level
- Practice the jump timing
- Use double jump to reach higher platforms
- Remember you can stomp enemies for points
- Take your time - there's no time limit

## Game Features Checklist

- ✅ Smooth player movement with acceleration
- ✅ Realistic jump physics with gravity
- ✅ Double jump ability
- ✅ Coyote time (grace period after leaving edge)
- ✅ Jump buffering (early input registration)
- ✅ Variable jump height
- ✅ 3 lives with respawn
- ✅ Invincibility frames after damage
- ✅ Two enemy types (Walker and Jumper)
- ✅ Enemy stomp mechanic
- ✅ Tile-based collision detection
- ✅ One-way platforms
- ✅ Spike hazards
- ✅ Coin collection
- ✅ Score tracking
- ✅ High score system
- ✅ Smooth camera following
- ✅ Parallax background
- ✅ Three complete levels
- ✅ Menu system
- ✅ Pause functionality
- ✅ Game over screen
- ✅ HUD with lives and score

## Code Structure

If you want to modify the game, here are the key files:

- **Constants.cs**: Adjust game physics and parameters
- **Player.cs**: Modify player behavior and controls
- **Enemy.cs**: Change enemy AI and behavior
- **TileMap.cs**: Edit level layouts (see GetLevel1/2/3 methods)
- **GameManager.cs**: Main game loop and state management

## Customization Examples

### Make Player Jump Higher
In `Constants.cs`, change:
```csharp
public const float JumpForce = -620f;  // Change to -800f for higher jumps
```

### Add More Lives
In `Constants.cs`, change:
```csharp
public const int PlayerMaxLives = 3;  // Change to 5 for more lives
```

### Adjust Gravity
In `Constants.cs`, change:
```csharp
public const float Gravity = 1800f;  // Lower = floatier, Higher = heavier
```

### Modify Level Layout
In `TileMap.cs`, edit the `GetLevel1()`, `GetLevel2()`, or `GetLevel3()` methods.
Each number represents a tile type:
- 0 = Empty
- 1 = Ground
- 2 = Platform
- 3 = Spike
- 4 = Coin
- 5 = Enemy Spawn
- 7 = Level End

## Next Steps

1. **Play through all 3 levels** to experience the full game
2. **Experiment with constants** to see how they affect gameplay
3. **Create your own level** by modifying the tile arrays
4. **Add new features** like power-ups or new enemy types
5. **Share your high score** with friends!

## Support

For issues or questions:
- Check the README.md for detailed documentation
- Review GAME_DESIGN.md for technical details
- Examine the code comments for implementation details

## Have Fun!

Enjoy playing Mini Mario! Try to beat all three levels and achieve the highest score possible. Good luck! 🎮🍄
