# Mini Mario - Advanced 2D Platformer

A complete, professional 2D platformer game built with MonoGame Framework featuring advanced physics, multiple enemy types, and three challenging levels.

## Features

### Player Mechanics
- Smooth movement with acceleration and deceleration
- Realistic jump physics with gravity
- Double jump capability
- Coyote time (grace period after leaving edge)
- Jump buffering (early jump input registration)
- Variable jump height (hold/release for different heights)
- 3 lives with respawn system
- Invincibility frames after taking damage

### Physics Engine
- Gravity system pulling entities down
- Terminal velocity (max fall speed)
- Ground friction and air resistance
- Framerate-independent physics using delta time
- Precise tile-based collision detection

### Level Design
- 3 progressively challenging levels
- Tile-based system with multiple tile types:
  - Ground (solid blocks)
  - Platforms (one-way, jump through from below)
  - Spikes (instant damage)
  - Coins (collectibles)
  - Enemy spawn points
  - Level end markers

### Enemy System
- Walker Enemy: Patrols back and forth, reverses at edges
- Jumper Enemy: Jumps toward player periodically
- Stomp mechanic: Jump on enemies to defeat them
- Enemy collision with player causes damage
- Enemies respect gravity and tile collision

### Camera System
- Smooth camera following with lerp
- Camera clamped to level boundaries
- Parallax background layers at different scroll speeds

### Game States
- Menu: Title screen with blinking "Press ENTER" text
- Playing: Full gameplay with pause functionality (ESC)
- Game Over: Shows final score and high score

### Visual Design
- All graphics created programmatically (no external files needed)
- Player: Blue rectangle with animated eyes
- Enemies: Red walkers with angry eyebrows, orange jumpers
- Coins: Gold circles with shine effect
- Spikes: Red triangular hazards
- Parallax starfield background
- HUD with score, lives (hearts), and coin counter

## Controls

| Key | Action |
|-----|--------|
| A / Left Arrow | Move Left |
| D / Right Arrow | Move Right |
| Space / W / Up Arrow | Jump (press again in air for double jump) |
| ESC | Pause / Back to Menu |
| R | Restart (on Game Over screen) |
| M | Back to Menu (on Game Over / Pause) |
| Enter | Start Game (on Menu) |

## How to Run

### Prerequisites
- .NET 8 SDK or later
- Windows, macOS, or Linux

### Installation & Running

1. Navigate to the project directory:
```bash
cd MiniMario
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Run the game:
```bash
dotnet run
```

The game window will open at 1280x720 resolution.

## Project Structure

```
MiniMario/
├── Constants.cs          # Game constants and configuration
├── GameManager.cs        # Main game class, state management
├── Program.cs            # Entry point
├── InputHelper.cs        # Keyboard input handling
├── TextureHelper.cs      # Programmatic texture creation
├── IGameState.cs         # Game state interface
├── MenuState.cs          # Main menu state
├── PlayingState.cs       # Gameplay state
├── GameOverState.cs      # Game over state
├── Player.cs             # Player character with physics
├── Enemy.cs              # Enemy base class and implementations
├── TileMap.cs            # Level data and tile collision
├── Camera.cs             # Camera system with parallax
├── HUD.cs                # Heads-up display
└── README.md             # This file
```

## Game Architecture

### State Machine
The game uses a state pattern with three main states:
- MenuState: Title screen and high score display
- PlayingState: Active gameplay with player, enemies, and level
- GameOverState: End screen with score and restart options

### Physics System
All physics calculations use delta time for framerate independence:
- Gravity: 1800 pixels/sec²
- Jump Force: -620 pixels/sec
- Move Speed: 220 pixels/sec
- Terminal Velocity: 900 pixels/sec

### Collision Detection
- Tile-based collision using grid coordinates
- Separate horizontal and vertical collision resolution
- Platform collision only from above
- Enemy collision with stomp detection

## Level Progression

### Level 1 - Tutorial
- Flat terrain with basic platforming
- Few enemies to learn mechanics
- Coins scattered for collection practice

### Level 2 - Intermediate
- Platforms over gaps
- Spike hazards
- Multiple enemies
- Requires precise jumping

### Level 3 - Advanced
- Vertical climbing sections
- Tight platforming challenges
- Many enemies
- Tests all player skills

## Technical Details

- Built with MonoGame 3.8
- Target Framework: .NET 8.0
- No external content files required
- All textures generated at runtime
- Optimized rendering with view culling

## Credits

Created as a complete, beginner-friendly example of a professional 2D platformer using MonoGame Framework.

## License

This project is provided as-is for educational purposes.
