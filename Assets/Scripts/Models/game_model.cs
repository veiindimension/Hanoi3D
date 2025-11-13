using UnityEngine;

namespace Hanoi.Model
{
    /// <summary>
    /// Core game state model for the Tower of Hanoi puzzle.
    /// Manages all game data: towers, disk count, move counter, and win condition.
    /// Part of the MVC Model layer - pure logic with no Unity dependencies except Debug.
    /// </summary>
    public class GameModel
    {
        #region Properties

        /// <summary>
        /// Total number of disks in the current game.
        /// </summary>
        public int DiskCount { get; private set; }

        /// <summary>
        /// Number of moves made by the player since game start/reset.
        /// </summary>
        public int MoveCount { get; private set; }

        /// <summary>
        /// Array of three towers (indices: 0 = Tower A, 1 = Tower B, 2 = Tower C).
        /// </summary>
        public TowerModel[] Towers { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new GameModel with three empty towers.
        /// </summary>
        public GameModel()
        {
            Towers = new TowerModel[3];
            for (int i = 0; i < 3; i++)
            {
                Towers[i] = new TowerModel();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes or resets the game with a specified number of disks.
        /// All disks start on Tower A (index 0) in descending size order.
        /// </summary>
        /// <param name="diskCount">Number of disks to use (typically 3-10)</param>
        public void Initialize(int diskCount)
        {
            DiskCount = diskCount;
            MoveCount = 0;

            // Clear all towers
            for (int i = 0; i < 3; i++)
            {
                Towers[i] = new TowerModel();
            }

            // Create disks in descending order (largest to smallest) on Tower A
            for (int i = diskCount; i >= 1; i--)
            {
                Towers[0].Push(new DiskModel(i, 0));
            }
        }

        #endregion

        #region Game Logic

        /// <summary>
        /// Moves a disk from one tower to another (logic only, no validation).
        /// Updates the move counter automatically.
        /// </summary>
        /// <param name="fromTower">Source tower index (0-2)</param>
        /// <param name="toTower">Destination tower index (0-2)</param>
        public void MoveDisk(int fromTower, int toTower)
        {
            // Validate tower indices
            if (fromTower < 0 || fromTower >= 3 || toTower < 0 || toTower >= 3)
            {
                return;
            }

            // Move disk between towers
            DiskModel disk = Towers[fromTower].Pop();
            if (disk != null)
            {
                disk.TowerIndex = toTower;
                Towers[toTower].Push(disk);
                MoveCount++;
            }
        }

        /// <summary>
        /// Increments the move counter by one.
        /// Used when moves are tracked separately from MoveDisk().
        /// </summary>
        public void IncrementMoveCount()
        {
            MoveCount++;
        }

        /// <summary>
        /// Resets the move counter to zero without affecting disk positions.
        /// </summary>
        public void ResetMoveCount()
        {
            MoveCount = 0;
        }

        #endregion

        #region Win Condition

        /// <summary>
        /// Checks if the game is complete.
        /// Victory condition: All disks moved to Tower C (index 2) in correct order.
        /// </summary>
        /// <returns>True if all disks are on Tower C in ascending size order</returns>
        public bool IsGameComplete()
        {
            // Check if Tower C contains all disks
            if (Towers[2].Count != DiskCount)
            {
                Debug.Log($"[GameModel] Tower C does not contain all disks. Current: {Towers[2].Count}, Expected: {DiskCount}");
                return false;
            }

            // Verify disks are in correct order (smallest to largest from top to bottom)
            int expectedSize = 1; // Start with smallest disk
            foreach (var disk in Towers[2].Disks)
            {
                if (disk.Size != expectedSize)
                {
                    Debug.Log($"[GameModel] Disk order incorrect in Tower C. Expected size: {expectedSize}, Found: {disk.Size}");
                    return false;
                }
                expectedSize++;
            }

            Debug.Log("[GameModel] Victory condition met: All disks are correctly stacked on Tower C.");
            return true;
        }

        #endregion
    }
}
