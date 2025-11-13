# Tower of Hanoi - Technical Architecture

This document provides an in-depth explanation of the code architecture, design patterns, and technical decisions behind the Tower of Hanoi 3D game.

---

## 📐 Architecture Overview

### MVC Design Pattern

The project follows a strict **Model-View-Controller** pattern to separate concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                        USER INPUT                            │
│                    (Mouse, Keyboard)                         │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                   CONTROLLER LAYER                           │
│  ┌────────────────────────────────────────────────────┐     │
│  │         GameController (Main Orchestrator)          │     │
│  │  • Handles input (raycasting)                       │     │
│  │  • Validates moves                                  │     │
│  │  • Coordinates Model ↔ View                         │     │
│  │  • Manages game loop                                │     │
│  └────────────────────────────────────────────────────┘     │
│                                                               │
│  ┌────────────────────────────────────────────────────┐     │
│  │        CylinderController (Disk Selector)           │     │
│  │  • Manages UI interaction                           │     │
│  │  • Updates game settings                            │     │
│  └────────────────────────────────────────────────────┘     │
└──────────────────┬────────────────────┬─────────────────────┘
                   │                    │
         ┌─────────▼────────┐  ┌────────▼─────────┐
         │   MODEL LAYER    │  │   VIEW LAYER     │
         │  (Pure Logic)    │  │  (Visual/Physics)│
         └─────────┬────────┘  └────────┬─────────┘
                   │                    │
    ┌──────────────┼────────────────────┼──────────────┐
    │              │                    │              │
    ▼              ▼                    ▼              ▼
┌────────┐  ┌────────────┐      ┌────────────┐  ┌──────────┐
│ Disk   │  │   Tower    │      │  DiskView  │  │ Unity    │
│ Model  │  │   Model    │      │ (Renderer, │  │ GameObj. │
│        │  │            │      │  Physics)  │  │          │
└────────┘  └────────────┘      └────────────┘  └──────────┘
     │              │                    │              │
     └──────────────┼────────────────────┼──────────────┘
                    │                    │
                    ▼                    ▼
            ┌────────────────────────────────┐
            │        GameModel               │
            │  • 3 TowerModels               │
            │  • Game state                  │
            │  • Win condition logic         │
            └────────────────────────────────┘
```

---

## 🧩 Layer Breakdown

### 1. Model Layer (Data & Logic)

**Purpose**: Pure game logic and data structures with NO Unity dependencies

#### `DiskModel.cs`
```
Properties:
  • Size (int)          - Disk size (1 = smallest)
  • TowerIndex (int)    - Current tower (0, 1, 2)
```

**Design Decision**: Immutable `Size` (set only in constructor) prevents accidental modification during gameplay.

#### `TowerModel.cs`
```
Data Structure: Stack<DiskModel>

Methods:
  • Push(disk)    - Add disk to top
  • Pop()         - Remove and return top disk
  • Peek()        - View top disk without removing
  • Clear()       - Remove all disks
  
Properties:
  • Count         - Number of disks
  • Disks         - Enumerable for iteration
```

**Design Decision**: Stack perfectly mirrors the physical tower structure (LIFO - Last In, First Out).

#### `GameModel.cs`
```
Properties:
  • DiskCount        - Total disks in game
  • MoveCount        - Player moves made
  • Towers[3]        - Array of TowerModels

Methods:
  • Initialize(count)      - Setup game with N disks
  • MoveDisk(from, to)     - Logical move between towers
  • IncrementMoveCount()   - Increase counter
  • ResetMoveCount()       - Reset to zero
  • IsGameComplete()       - Check if all disks on Tower C
```

**Design Decision**: This is the "source of truth" for game state. All game logic happens here first, then Views update to reflect changes.

---

### 2. View Layer (Visual Representation)

**Purpose**: Bridge between GameObjects and logical models

#### `DiskView.cs`

The most complex script, handling:

##### A. Initialization
```csharp
Initialize(DiskModel model, GameController controller)
  ├─ Set model reference
  ├─ Calculate visual scale based on size
  ├─ Setup materials (base + outline)
  ├─ Assign random color
  └─ Store initial position
```

##### B. Visual Feedback System
```
OnHoverEnter(canPick)
  ├─ If can pick: Green outline
  └─ If cannot pick: Red outline

OnHoverExit()
  └─ Remove outline
```

##### C. Physics-Based Dragging

**Elastic Spring System**:
```
Force = (target - current) * springStrength - velocity * springDamping
```

Parameters:
- `springStrength` (60): Pull force toward mouse
- `springDamping` (10): Reduces oscillation
- `dragPlaneZOffset` (0): Z-depth of drag plane

**Why Springs?**:
- Smooth, organic movement
- Automatically handles acceleration/deceleration
- Feels more natural than rigid cursor following

##### D. Landing Validation Flow
```
OnRelease()
  └─ Wait 0.3 seconds (physics settle)
      └─ CheckLandingAfterDelay()
          ├─ Find closest tower (by X position)
          ├─ Check distance < 1.5 units
          ├─ Validate Hanoi rules
          │   ├─ If valid: Update model, snap to tower
          │   └─ If invalid: ResetPosition()
          └─ Increment move counter
```

**Design Decision**: Delayed validation allows physics to settle, preventing premature checks while disk is still moving.

##### E. Reset Position Logic

```csharp
ResetPosition()
  ├─ Get tower position from TowerIndex
  ├─ Move disk above that tower
  ├─ Zero out all velocities
  └─ Play error sound
```

**Why Static Reset?**: Using `TowerIndex` instead of tracking initial position prevents bugs when disks move between towers dynamically.

---

### 3. Controller Layer (Orchestration)

#### `GameController.cs`

The "brain" of the game. Responsibilities:

##### A. Initialization Flow
```
Start()
  └─ InitializeGame()
      ├─ Create GameModel
      ├─ Initialize with diskCount
      ├─ Setup tower transform references
      ├─ SpawnDisks()
      │   ├─ Order disks (or randomize)
      │   ├─ Instantiate prefabs
      │   ├─ Initialize DiskViews
      │   └─ Position on Tower A
      └─ UpdateMoveCountUI()
```

##### B. Input Handling (Raycast-Based)
```
Update()
  └─ HandleMouseRaycast()
      ├─ Raycast from mouse to scene
      ├─ Check if hit disk
      ├─ Update hoveredDisk
      │   ├─ Call OnHoverExit() on previous
      │   ├─ Call OnHoverEnter() on new
      │   └─ Pass canPick status
      └─ Clear hover if no hit
```

**Design Decision**: Raycasting allows precise 3D object selection without complex collision matrices.

##### C. Move Validation
```
MoveDiskToTower(disk, targetIndex)
  ├─ Get source tower from disk.TowerIndex
  ├─ Verify disk is on top
  ├─ Check Hanoi rule (size comparison)
  │   ├─ If valid:
  │   │   ├─ Pop from source tower
  │   │   ├─ Push to target tower
  │   │   └─ Update disk.TowerIndex
  │   └─ If invalid:
  │       └─ Call disk.ResetPosition()
  └─ Update UI
```

**Critical**: Model is updated BEFORE view position changes, maintaining data integrity.

##### D. Victory Detection
```
Update()
  └─ if GameModel.IsGameComplete() && !hasVictoryScreenShown
      └─ ShowVictoryScreen()
          ├─ Update UI text
          ├─ Play victory sound
          └─ Set flag to prevent repeat
```

**Design Decision**: Flag prevents multiple victory sounds/screens if player continues moving disks after winning.

##### E. Reset System

Two reset methods:

1. **ResetGameWithDiskCount()** - Full reset
   - Destroys all disk GameObjects
   - Respawns based on current `diskCount`
   - Used by: Reset button, Disk selector

2. **ResetAllDisks()** - Soft reset
   - Repositions existing disks
   - Faster, no instantiation overhead
   - Currently unused (kept for potential undo system)

---

#### `CylinderController.cs`

Manages the disk count selector UI:

```
Interaction Flow:
OnMouseDown()
  ├─ Play grab sound
  └─ Start dragging

Update() (while dragging)
  └─ DragCylinder()
      └─ Constrain position to X-axis only

OnMouseUp()
  ├─ Play release sound
  └─ SnapToClosestSelector()
      ├─ Find nearest selector marker
      ├─ Parse selector.name as int
      ├─ Update GameController.diskCount
      └─ Snap to exact position
```

**Design Decision**: Selector names are integers ("3", "4", ..., "10") for easy parsing without lookup tables.

---

### 4. UI Layer (User Interaction)

#### `ResetButton.cs`
```
OnMouseDown()
  ├─ Play sound
  ├─ AnimateButton() (down → up)
  └─ FindFirstObjectByType<GameController>()
      └─ ResetGameWithDiskCount()
```

#### `OrderButton.cs`
```
OnMouseDown()
  ├─ Play sound
  ├─ AnimateButton()
  ├─ Toggle GameController.spawnDisksOrdered
  └─ UpdateLedColor()
      ├─ Green if ordered
      └─ Red if random
```

#### `QuitButton.cs`
```
OnMouseDown()
  ├─ Play sound
  ├─ AnimateButton()
  └─ Application.Quit()
```

**Note**: All buttons use consistent animation pattern for visual feedback.

---

## 🔄 Data Flow Examples

### Example 1: Player Picks Up Disk

```
1. User clicks disk
   ↓
2. GameController.HandleMouseClickRelease()
   ↓
3. Check CanSelectDisk(disk)
   ├─ Query GameModel: Is disk on top?
   └─ Return true/false
   ↓
4. If true: selectedDisk.OnPick()
   ↓
5. DiskView.OnPick()
   ├─ Set isHeld = true
   ├─ Increase damping
   └─ Play pickup sound
   ↓
6. FixedUpdate() starts applying elastic force
```

### Example 2: Valid Disk Placement

```
1. User releases disk above Tower B
   ↓
2. DiskView.OnRelease()
   ↓
3. Wait 0.3 seconds
   ↓
4. CheckLandingAfterDelay()
   ├─ Find closest tower (Tower B)
   ├─ Distance check: PASS
   └─ Rule validation: PASS
   ↓
5. GameController.MoveDiskToTower(disk, 1)
   ├─ GameModel.Towers[0].Pop()
   ├─ GameModel.Towers[1].Push(disk.model)
   └─ disk.model.TowerIndex = 1
   ↓
6. GameModel.IncrementMoveCount()
   ↓
7. UpdateMoveCountUI()
   ↓
8. DiskView snaps to position on Tower B
```

### Example 3: Invalid Disk Placement

```
1. User tries to place large disk on small disk
   ↓
2. DiskView.CheckLandingAfterDelay()
   ├─ Find closest tower
   ├─ Distance check: PASS
   └─ Rule validation: FAIL (size > topDisk.size)
   ↓
3. DiskView.ResetPosition()
   ├─ Move to position above original tower
   ├─ Zero velocities
   └─ Play error sound
   ↓
4. Move counter NOT incremented
```

---

## 🎯 Key Design Principles

### 1. Single Responsibility Principle
Each class has one clear purpose:
- Models: Data storage
- Views: Visual representation
- Controllers: Coordination

### 2. Dependency Injection
```csharp
DiskView.Initialize(DiskModel model, GameController controller)
```
Views receive dependencies instead of finding them, making testing easier.

### 3. Separation of Concerns
Models have ZERO Unity dependencies (except Debug):
```csharp
// ✅ Good
public class GameModel { ... }

// ❌ Bad
public class GameModel : MonoBehaviour { ... }
```

### 4. Encapsulation
Private setters protect data integrity:
```csharp
public int Size { get; private set; }  // Read-only externally
```

### 5. Clear Communication Paths
```
User Input → Controller → Model (logic) → Controller → View (visual update)
```

Never: View → Model directly (bypasses validation)

---

## 🧪 Testing Strategy

### Unit Testable Components
Models can be tested without Unity:

```csharp
[Test]
public void TestMoveDisk()
{
    GameModel game = new GameModel();
    game.Initialize(3);
    
    game.MoveDisk(0, 1);  // Move from A to B
    
    Assert.AreEqual(2, game.Towers[0].Count);
    Assert.AreEqual(1, game.Towers[1].Count);
}
```

### Integration Tests
Controllers require Unity Test Framework:
- Test disk selection validation
- Test move validation logic
- Test victory condition detection

---

## 🔧 Configuration & Extensibility

### Adding New Features

#### Example: Undo System
1. **Model**: Add `Stack<Move> moveHistory` to GameModel
2. **View**: No changes needed
3. **Controller**: 
   - Store moves before executing
   - Add `UndoLastMove()` method
4. **UI**: Create undo button

#### Example: Timer Mode
1. **Model**: Add `float timeElapsed` to GameModel
2. **View**: Create TimerView for display
3. **Controller**: Update timer in `Update()`
4. **UI**: Show timer, add pause button

---

## 📊 Performance Considerations

### Optimization Techniques Used

1. **Object Caching**
   ```csharp
   private Camera mainCamera;  // Cached in Awake()
   ```

2. **Component Caching**
   ```csharp
   private Rigidbody rb;
   private Renderer rend;
   ```

3. **Efficient Queries**
   ```csharp
   FindObjectsByType<T>(FindObjectsSortMode.None)  // Unity 6 optimized API
   ```

4. **Physics Optimization**
   - Rigidbody constraints (freeze rotations)
   - Appropriate damping values
   - Fixed collider shapes

### Potential Bottlenecks

- Instantiating/Destroying disks frequently (use object pooling if needed)
- Too many raycasts per frame (currently only in Update, acceptable)

---

## 🛡️ Error Handling

### Defensive Programming
All public methods validate inputs:

```csharp
public void MoveDiskToTower(DiskView disk, int targetIndex)
{
    if (gameModel == null) return;
    if (targetIndex < 0 || targetIndex >= 3) return;
    // ... rest of method
}
```

### Logging Strategy
- Errors: Missing references, invalid states
- Warnings: Rule violations, unexpected behavior
- Info: Major game events (victory, reset)

---

## 📈 Scalability

### Easy to Add:
- More towers (change TowerModel[] size)
- Different rules (modify validation in GameController)
- AI solver (create new controller)
- Network multiplayer (sync GameModel state)

### Harder to Add:
- Completely different puzzle mechanics (would need refactoring)
- 2D mode (Views tightly coupled to 3D physics)

---

## 🔍 Code Quality Metrics

- **Cyclomatic Complexity**: Low (most methods < 10 branches)
- **Coupling**: Low (MVC separation maintained)
- **Cohesion**: High (related functionality grouped)
- **Testability**: High for Models, Medium for Controllers, Low for Views

---

## 📝 Maintenance Notes

### When Updating Unity Version
- Check `FindObjectsByType` API compatibility
- Verify Physics system changes
- Test Rigidbody.linearVelocity (renamed from velocity in Unity 6)

### When Adding New Scripts
- Follow MVC pattern strictly
- Add XML documentation comments
- Use regions for organization
- Keep debug logs in English

---

## 🎓 Learning Resources

To understand this architecture better:
- [Unity MVC Pattern](https://www.raywenderlich.com/2311-introduction-to-mvcs-in-unity)
- [C# Design Patterns](https://refactoring.guru/design-patterns/csharp)
- [Unity Best Practices](https://unity.com/how-to/programming-unity)

---

**Document Version**: 1.0  
**Last Updated**: November 2025
