# Tower of Hanoi 3D - Unity Game

A 3D interactive implementation of the classic Tower of Hanoi puzzle built in Unity 6000.2.2f1 using C# and the MVC (Model-View-Controller) design pattern.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [System Requirements](#system-requirements)
- [Project Structure](#project-structure)
- [How to Play](#how-to-play)
- [Technical Architecture](#technical-architecture)
- [Setup Instructions](#setup-instructions)
- [Scripts Documentation](#scripts-documentation)
- [Design Decisions](#design-decisions)
- [Known Limitations](#known-limitations)
- [Future Improvements](#future-improvements)

---

## 🎯 Overview

This project is a 3D physics-based Tower of Hanoi puzzle game where players can:
- Drag and drop disks between three towers
- Choose between 3-10 disks
- Track their move count
- Experience smooth elastic physics and visual feedback
- Toggle between ordered and random disk spawning

The game validates moves according to traditional Hanoi rules:
1. Only one disk can be moved at a time
2. A larger disk cannot be placed on top of a smaller disk
3. Goal: Move all disks from Tower A to Tower C

---

## ✨ Features

### Core Gameplay
- **Physics-Based Interaction**: Disks use elastic spring physics for smooth mouse following
- **Visual Feedback**: Outline effects (green = valid, red = invalid) on hover
- **Audio Feedback**: Sounds for pickup, release, invalid moves, and victory
- **Move Counter**: Tracks number of moves made
- **Victory Detection**: Automatic win condition checking

### Customization
- **Configurable Disk Count**: Select 3-10 disks via interactive cylinder selector
- **Spawn Order Toggle**: Choose ordered (largest to smallest) or random disk spawning
- **Reset Functionality**: Reset button to restart the game

### User Interface
- Reset Button: Restart the game
- Order Button: Toggle spawn order (LED indicator: green = ordered, red = random)
- Quit Button: Exit the application
- Move Counter: Real-time display of moves made

---

## 🖥️ System Requirements

- **Unity Version**: 6000.2.2f1 or newer
- **Platform**: Windows, macOS, Linux (desktop builds)
- **Recommended Hardware**: Mid-range PC with dedicated GPU

---

## 📁 Project Structure

```
Hanoi3D/
├── Assets/
│   ├── Scripts/
│   │   ├── Models/           # Data models (MVC Model layer)
│   │   │   ├── DiskModel.cs
│   │   │   ├── TowerModel.cs
│   │   │   └── GameModel.cs
│   │   ├── Views/            # Visual representations (MVC View layer)
│   │   │   └── DiskView.cs
│   │   ├── Controllers/      # Game logic controllers (MVC Controller layer)
│   │   │   ├── GameController.cs
│   │   │   └── CylinderController.cs
│   │   └── UI/               # UI interaction scripts
│   │       ├── ResetButton.cs
│   │       ├── OrderButton.cs
│   │       └── QuitButton.cs
│   ├── Prefabs/
│   │   └── DiskPrefab        # Disk GameObject with DiskView component
│   ├── Materials/
│   │   ├── M_Disk            # Base disk material
│   │   ├── M_Outline         # Outline shader material
│   │   └── M_Led             # LED indicator material
│   └── Audio/
│       ├── DiskHold.wav
│       ├── DiskRelease.wav
│       ├── WrongClick.wav
│       ├── Victory.wav
│       ├── Lever1.wav
│       └── Lever2.wav
└── README.md
```

---

## 🎮 How to Play

### Controls
1. **Hover**: Move mouse over a disk to see if it can be selected
   - Green outline = disk can be picked up (on top of tower)
   - Red outline = disk cannot be picked up (not on top)

2. **Pick Up**: Click and hold left mouse button on a valid disk

3. **Drag**: Move mouse while holding button - disk follows with elastic physics

4. **Release**: Release mouse button to drop disk
   - Disk automatically snaps to nearest tower
   - Invalid placements are rejected with error sound

### Game Settings
- **Disk Count Selector**: Drag the cylinder left/right to choose 3-10 disks
- **Order Button**: Click to toggle spawn order (watch LED color)
- **Reset Button**: Click to restart with current settings
- **Quit Button**: Click to exit the application

### Objective
Move all disks from Tower A (leftmost) to Tower C (rightmost) following the rules:
- Only move one disk at a time
- Never place a larger disk on a smaller one

---

## 🏗️ Technical Architecture

### MVC Design Pattern

This project strictly follows the **Model-View-Controller** pattern:

#### **Model Layer** (Pure Data & Logic)
- `DiskModel`: Represents a single disk (size, tower index)
- `TowerModel`: Stack data structure for disk storage
- `GameModel`: Central game state (towers, disk count, move counter, win condition)

**Key Principle**: Models contain zero Unity dependencies (except Debug for logging)

#### **View Layer** (Visual Representation)
- `DiskView`: Handles rendering, physics, user interaction, and visual feedback for disks
  - Manages Rigidbody physics
  - Implements elastic spring dragging
  - Handles outline materials for hover feedback
  - Validates landing positions

**Key Principle**: Views bridge GameObjects with their logical models

#### **Controller Layer** (Orchestration)
- `GameController`: Central manager coordinating all game systems
  - Initializes game state
  - Handles mouse input (raycasting)
  - Validates moves according to Hanoi rules
  - Manages UI updates
  - Checks victory conditions
  
- `CylinderController`: Manages disk count selector interaction

**Key Principle**: Controllers handle communication between Models and Views

#### **UI Scripts** (User Interaction)
- `ResetButton`: Game reset functionality
- `OrderButton`: Spawn order toggle with LED feedback
- `QuitButton`: Application exit

---

## 🛠️ Setup Instructions

### Opening the Project
1. Install Unity Hub and Unity 6000.2.2f1
2. Clone or download the project
3. Open project in Unity Hub
4. Wait for scripts to compile

### Running the Game
1. Open the main scene (should be the only scene in the project)
2. Press Play button in Unity Editor
3. Use mouse to interact with disks and UI buttons

### Building the Game
1. File → Build Settings
2. Select your target platform
3. Click "Build" and choose output folder
4. Run the executable

**Note**: Quit button only works in builds, not in the Editor

---

## 📚 Scripts Documentation

### Model Scripts

#### `DiskModel.cs`
Pure data class representing a disk.
- **Properties**: `Size` (int), `TowerIndex` (int)
- **Purpose**: Store disk state without game logic

#### `TowerModel.cs`
Stack data structure for disks.
- **Methods**: `Push()`, `Pop()`, `Peek()`, `Clear()`
- **Properties**: `Count`, `Disks` (enumerable)

#### `GameModel.cs`
Central game state manager.
- **Properties**: `DiskCount`, `MoveCount`, `Towers[]`
- **Methods**: 
  - `Initialize(diskCount)`: Setup game
  - `MoveDisk(from, to)`: Logical move
  - `IsGameComplete()`: Check win condition

### View Scripts

#### `DiskView.cs`
Disk visual representation and interaction.
- **Physics**: Elastic spring dragging with configurable parameters
- **Visual Feedback**: Outline materials (green/red) on hover
- **Validation**: Checks landing position and Hanoi rules
- **Key Methods**:
  - `Initialize()`: Setup disk appearance and scaling
  - `OnPick()` / `OnRelease()`: Handle user interaction
  - `ApplyElasticFollow()`: Physics-based mouse following
  - `CheckLandingAfterDelay()`: Validate disk placement

### Controller Scripts

#### `GameController.cs`
Main game orchestrator.
- **Initialization**: Spawns disks, sets up towers
- **Input Handling**: Mouse raycasting for disk selection
- **Move Validation**: Enforces Hanoi rules
- **UI Management**: Updates move counter, victory screen
- **Key Methods**:
  - `InitializeGame()`: Setup game state
  - `HandleMouseRaycast()`: Detect hover
  - `CanSelectDisk()`: Validate selection
  - `MoveDiskToTower()`: Execute move with validation
  - `ResetGameWithDiskCount()`: Full game reset

#### `CylinderController.cs`
Disk count selector controller.
- **Interaction**: Drag-and-drop along X-axis
- **Snapping**: Automatically snaps to nearest selector
- **Integration**: Updates GameController disk count

### UI Scripts

#### `ResetButton.cs`
Reset functionality with press animation.
- Calls `GameController.ResetGameWithDiskCount()`
- Plays button sound and animation

#### `OrderButton.cs`
Spawn order toggle with LED feedback.
- Toggles `GameController.spawnDisksOrdered` boolean
- Updates LED material color (green/red)

#### `QuitButton.cs`
Application exit button.
- Calls `Application.Quit()` (works in builds only)

---

## 🎨 Design Decisions

### Why MVC Pattern?
- **Separation of Concerns**: Clear boundaries between data, display, and logic
- **Testability**: Models can be unit tested without Unity
- **Maintainability**: Easy to modify one layer without affecting others
- **Scalability**: Simple to add features (e.g., undo system, AI solver)

### Why Physics-Based Dragging?
- Smooth, organic movement feels better than rigid cursor following
- Elastic spring physics provides satisfying tactile feedback
- Allows for realistic interactions with 3D environment

### Why Validate Placement After Release?
- Players can naturally drag and drop
- Physics settles before validation (more reliable)
- Clear audio/visual feedback on invalid moves

### Code Organization Choices
- **Regions**: Scripts organized into logical sections for readability
- **XML Comments**: All public methods documented
- **Naming Conventions**: Clear, descriptive names following C# standards
- **No Magic Numbers**: Settings exposed as serialized fields

---

## ⚠️ Known Limitations

1. **Single Scene Only**: Game uses one scene; no menu/level system
2. **No Save System**: Game state not persisted between sessions
3. **No Undo Feature**: Cannot reverse moves
4. **Desktop Only**: Not optimized for mobile/touch input
5. **Fixed Camera**: Camera position cannot be changed by player

---

## 🚀 Future Improvements

### Potential Features
- **Hint System**: Suggest next valid move
- **Auto-Solve**: AI solver demonstration
- **Timer Mode**: Challenge mode with time limits
- **Move History**: List of all moves with undo capability
- **Leaderboard**: Track best scores (fewest moves)
- **Different Puzzle Variants**: 
  - 4-tower Hanoi
  - Colored disk restrictions
- **VR Support**: Adaptation for Meta Quest
- **Mobile Version**: Touch controls and UI optimization

### Code Improvements
- Unit tests for Model layer logic
- Object pooling for disk instantiation
- Custom editor tools for level design
- Configurable difficulty presets
- Achievement system

---

## 📝 Credits

- **Developer**: [Your Name]
- **Unity Version**: 6000.2.2f1
- **Design Pattern**: MVC (Model-View-Controller)
- **Development Time**: [Estimated hours]

---

## 📄 License

This project was created as a technical test demonstration. All rights reserved.

---

## 🤝 Contact

For questions or feedback, please contact: [Your Email]

---

**Last Updated**: November 2025
