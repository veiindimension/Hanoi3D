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
        /// Disks start on tower 0 in random order.
        /// </summary>
        public void Initialize(int diskCount)
        {
            DiskCount = diskCount;
            MoveCount = 0;

            // Reset all towers
            for (int i = 0; i < 3; i++)
                Towers[i] = new TowerModel();

            // Create unique disk sizes
            List<int> sizes = new List<int>();
            for (int i = 1; i <= diskCount; i++)
                sizes.Add(i);

            // Shuffle order randomly
            for (int i = 0; i < sizes.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, sizes.Count);
                (sizes[i], sizes[randomIndex]) = (sizes[randomIndex], sizes[i]);
            }

            // Push all disks onto the first tower
            foreach (int size in sizes)
            {
                Towers[0].Push(new DiskModel(size, 0));
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
        /// Checks if the game is finished (all disks moved to the last tower).
        /// </summary>
        public bool IsGameComplete()
        {
            return Towers[2].Count == DiskCount;
        }
    }
}
