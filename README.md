# Match 3 Example
A match-3 style game developed with Unity, featuring custom physics system and optimized grid-based mechanics.

Gameplay: [YouTube Video](https://youtu.be/Qv45kBFJUTM)

## Technical Features
### Custom Physics System
- Implemented a simplified physics system without Unity's built-in physics
- Optimized for grid-based movement primarily on the Y-axis
- Uses Image components on Canvas for better batching performance
- Efficient object pooling for spawning and respawning grid cells

### Grid System
- Grid cells maintain references to:
  - ScriptableObject (ABlastableContainer) for item properties
  - Current grid position
  - Image reference
  - Root reference for grouping

### Optimized Search Algorithms
- Three distinct search patterns:
  - Connected (same type) search
  - Directional search
  - Distance-based search
- Cached HashSet implementation for performance
- Custom search configurations per BlastableType

### Memory Management
- Extensive use of struct types to minimize garbage collection
- Object pooling for particle effects and grid elements
- Cached reference objects for optimal performance

### Additional Features
- Deadlock prevention system with automatic grid remix
- Player hint system for available matches
- Custom editor tools for testing
- Visual and audio feedback systems
- Fireworks, bombs, and special effects
- Main menu system

## Technical Dependencies
- Unity Version: 6000.0.30f1
- PrimeTween for optimized animations
- UniTask for asynchronous operations

## Debug Mode
To enable debug logs (Debug.Log, Debug.LogError):
1. Open Project Settings
2. Navigate to Player Settings
3. Add "GAME_DEBUG" to Scripting Define Symbols

## Performance Notes
- Android build is locked to 60 FPS (configurable in Menu.cs)
- Optimized batching through UI components
- Flyweight pattern implementation for group management

## Development Tools
- Custom GameManagerEditor for testing and debugging
- Automated group detection and management
- Configurable blast patterns and effects

Note: Due to copyright restrictions on various assets used in development, this repository contains only the scripts and source code.
