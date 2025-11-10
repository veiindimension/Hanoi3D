// 07/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections.Generic;

namespace Hanoi.Model
{
    /// <summary>
    /// The logical model of the Tower of Hanoi game.
    /// Stores all game data: towers, disks, move count.
    /// </summary>
    public class GameModel
    {
        public int DiskCount { get; private set; }
        public int MoveCount { get; private set; }

        public TowerModel[] Towers { get; private set; }

        public GameModel()
        {
            Towers = new TowerModel[3];
            for (int i = 0; i < 3; i++)
                Towers[i] = new TowerModel();
        }

        /// <summary>
        /// Initializes the model with a given number of disks.
        /// Disks start on tower 0 in order from largest to smallest.
        /// </summary>
        public void Initialize(int diskCount)
        {
            DiskCount = diskCount;
            MoveCount = 0;

            // Reset all towers
            for (int i = 0; i < 3; i++)
                Towers[i] = new TowerModel();

            // Create unique disk sizes in descending order
            for (int i = diskCount; i >= 1; i--)
            {
                Towers[0].Push(new DiskModel(i, 0));
            }
        }

        /// <summary>
        /// Moves a disk from one tower to another (logic only).
        /// </summary>
        public void MoveDisk(int fromTower, int toTower)
        {
            if (fromTower < 0 || fromTower >= 3 || toTower < 0 || toTower >= 3)
                return;

            DiskModel disk = Towers[fromTower].Pop();
            disk.TowerIndex = toTower;
            Towers[toTower].Push(disk);

            MoveCount++;
        }

        /// <summary>
        /// Resets the move count to zero.
        /// </summary>
        public void ResetMoveCount()
        {
            MoveCount = 0;
        }

        public void IncrementMoveCount()
        {
            MoveCount++;
        }

        /// <summary>
        /// Checks if the game is finished (all disks moved to the last tower in order).
        /// </summary>
        public bool IsGameComplete()
        {
            if (Towers[2].Count != DiskCount) return false;

            // Check if disks are in order from largest to smallest
            int expectedSize = DiskCount;
            foreach (var disk in Towers[2].Disks)
            {
                if (disk.Size != expectedSize)
                    return false;
                expectedSize--;
            }

            return true;
        }
    }
}