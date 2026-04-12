# Game Design Document - Mini Mario

## Overview
Mini Mario is a complete 2D platformer featuring advanced physics, multiple enemy types, and three progressively challenging levels.

## Core Mechanics

### Player Movement
- **Horizontal Movement**: Smooth acceleration/deceleration with lerp
- **Jump**: Initial force of -620 pixels/sec
- **Double Jump**: Second jump with -520 pixels/sec force
- **Coyote Time**: 0.1 second grace period after leaving platform edge
- **Jump Buffer**: 0.15 second window to register early jump inputs
- **Variable Jump Height**: Release jump key early for shorter jumps

### Physics Constants
```csharp
Gravity: 1800 pixels/sec²
Terminal Velocity: 900 pixels/sec
Move Speed: 220 pixels/sec
Jump Force: -620 pixels/sec
Double Jump Force: -520 pixels/sec
Friction: 0.85 (ground)
Air Resistance: 0.95 (air)
```

### Player Stats
- Lives: 3
- Invincibility Duration: 1.5 seconds after hit
- Size: 28x44 pixels

## Enemy Types

### Walker Enemy
- **Behavior**: Patrols back and forth
- **Speed**: 80 pixels/sec
- **Health**: 1 hit
- **Patrol Distance**: 200 pixels from spawn
- **AI**: Reverses at edges and walls
- **Visual**: Red rectangle with angry eyebrows

### Jumper Enemy
- **Behavior**: Jumps toward player periodically
- **Speed**: 60 pixels/sec horizontal
- **Jump Force**: -500 pixels/sec
- **Jump Interval**: 2 seconds
- **Health**: 2 hits
- **Visual**: Orange rectangle with rounded appearance

### Enemy Interactions
- **Stomp**: Player jumps on top → enemy takes 1 damage, player bounces up
- **Collision**: Player touches from side → player loses 1 life, gains invincibility
- **Score**: +50 points per enemy killed

## Tile System

### Tile Types
| ID | Type | Description |
|----|------|-------------|
| 0 | Empty | Air, no collision |
| 1 | Ground | Solid block, full collision |
| 2 | Platform | One-way platform, can jump through from below |
| 3 | Spike | Instant damage on contact |
| 4 | Coin | Collectible, +10 score |
| 5 | Enemy Spawn | Spawns enemy at level start |
| 6 | Checkpoint | Save point (future feature) |
| 7 | Level End | Triggers level completion |

### Tile Size
- 32x32 pixels per tile
- Grid-based collision detection

## Level Design

### Level 1 - Tutorial
- **Dimensions**: 50x20 tiles (1600x640 pixels)
- **Difficulty**: Easy
- **Features**:
  - Flat terrain for learning movement
  - Few coins to collect
  - One walker enemy
  - Simple platform section
- **Goal**: Teach basic mechanics

### Level 2 - Intermediate
- **Dimensions**: 50x20 tiles
- **Difficulty**: Medium
- **Features**:
  - Platforms over gaps
  - Spike hazards
  - Multiple enemies (walkers and jumpers)
  - Requires precise jumping
- **Goal**: Test platforming skills

### Level 3 - Advanced
- **Dimensions**: 50x20 tiles
- **Difficulty**: Hard
- **Features**:
  - Vertical climbing sections
  - Tight platforming
  - Many enemies
  - Complex spike patterns
- **Goal**: Master-level challenge

## Camera System

### Following Behavior
- **Type**: Smooth lerp-based following
- **Lerp Speed**: 5.0 (higher = faster catch-up)
- **Target**: Player centered on screen
- **Bounds**: Clamped to level boundaries

### Parallax Layers
- **Layer 1**: 0.1x speed (far stars)
- **Layer 2**: 0.3x speed (mid stars)
- **Layer 3**: 0.5x speed (near stars)

## Scoring System

### Point Values
- Coin: +10 points
- Enemy Kill: +50 points
- High Score: Persists during game session

### HUD Display
- Score (top-left)
- Lives as hearts (top-center)
- Coins collected counter (top-center-right)
- Current level number (top-right)

## Game States

### Menu State
- Title animation with bouncing letters
- Blinking "Press ENTER to Start"
- High score display
- Animated parallax background
- Controls reference

### Playing State
- Full gameplay loop
- Pause functionality (ESC)
- Level progression
- Enemy spawning
- Collision detection
- Score tracking

### Game Over State
- Final score display
- High score comparison
- "New High Score" animation if beaten
- Restart option (R key)
- Return to menu option (M key)

## Input Mapping

### Movement
- A / Left Arrow: Move left
- D / Right Arrow: Move right
- Space / W / Up Arrow: Jump

### System
- ESC: Pause/Resume
- M: Return to menu (when paused/game over)
- R: Restart game (when game over)
- Enter: Start game (on menu)

## Visual Design

### Color Palette
- **Player**: Blue (#0000FF) with white eyes
- **Walker Enemy**: Red (#FF0000) with dark red details
- **Jumper Enemy**: Orange (#FFA500) with dark orange details
- **Ground**: Dark gray (#505050) with lighter border
- **Platform**: Brown (#8B4513) with tan border
- **Spike**: Red (#FF0000) triangular
- **Coin**: Gold (#FFD700) with yellow shine
- **Background**: Dark blue gradient (#14143C to #3C3C78)
- **Stars**: White (#FFFFFF) with transparency

### Animation States
- **Idle**: Standing still
- **Walking**: Moving horizontally
- **Jumping**: Rising (velocity < 0)
- **Falling**: Descending (velocity > 0)

### Visual Effects
- Invincibility flashing (10 Hz blink)
- Coin spin animation
- Title letter bounce
- Parallax scrolling
- Camera smoothing

## Technical Architecture

### Class Structure
```
GameManager (Game)
├── MenuState (IGameState)
├── PlayingState (IGameState)
│   ├── Player
│   ├── TileMap
│   ├── Camera
│   ├── HUD
│   └── List<Enemy>
│       ├── WalkerEnemy
│       └── JumperEnemy
└── GameOverState (IGameState)
```

### Update Loop
1. Input handling (InputHelper)
2. Player physics update
3. Enemy AI and physics
4. Collision detection (player-tile, player-enemy)
5. Camera following
6. State transitions

### Render Loop
1. Background (parallax layers)
2. World objects (with camera transform)
   - Tiles
   - Enemies
   - Player
3. HUD (no camera transform)
4. Overlays (pause, game over)

## Collision Detection

### Algorithm
1. Calculate entity bounds in world space
2. Convert to tile grid coordinates
3. Check surrounding tiles for collision
4. Resolve X-axis collisions first
5. Resolve Y-axis collisions second
6. Update entity position

### Platform Collision
- Only collide when coming from above
- Check if entity bottom was above platform top in previous frame
- Allow jumping through from below

### Enemy Stomp Detection
- Check if player velocity is downward (Y > 0)
- Check if player bottom is near enemy top (within 10 pixels)
- If true: stomp, else: damage player

## Performance Optimizations

### View Culling
- Only render tiles visible on screen
- Calculate visible tile range based on camera position
- Skip drawing off-screen entities

### Texture Management
- All textures created once at startup
- Stored in dictionary for fast lookup
- Reused across all draw calls

### Physics
- Delta time for framerate independence
- Early exit on collision detection
- Efficient grid-based tile lookup

## Future Enhancements

### Potential Features
- Checkpoint system
- Moving platforms
- Power-ups (speed boost, invincibility)
- Boss battles
- Level editor
- Sound effects and music
- Particle effects
- More enemy types
- Collectible secrets
- Time attack mode
- Leaderboard system

### Technical Improvements
- Save/load system
- Configuration file
- Gamepad support
- Fullscreen mode
- Resolution scaling
- Performance profiling
- Unit tests
