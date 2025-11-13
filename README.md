# Hanoi3D
Traditional puzzle game - Tower of Hanoi, made with Unity and Blender, using C# and the MVC (Model-View-Controller) design pattern., fully interactive and 3D.


# OVERVIEW
This project is a 3D physics-based Tower of Hanoi puzzle game where players can:
- Drag and drop disks between three towers
- Choose between 3-10 disks
- Track their move count
- Toggle between ordered and random disk spawning
## Objective
Move all disks from Tower A (leftmost) to Tower C (rightmost) following the rules:
- Only move one disk at a time
- Never place a larger disk on a smaller one


# 🎮 HOW TO PLAY
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

# DOCUMENTATION
You can review the full documentation inside the "Documentation" folder.
