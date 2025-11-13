using System;

namespace Hanoi.Model
{
    /// <summary>
    /// Data model representing a single disk in the Tower of Hanoi puzzle.
    /// Stores the disk's size and current tower position.
    /// This is a pure data class with no game logic - part of the MVC Model layer.
    /// </summary>
    [Serializable]
    public class DiskModel
    {
        #region Properties

        /// <summary>
        /// Size of the disk. Smaller numbers = smaller disks.
        /// Size 1 is the smallest disk, higher numbers are progressively larger.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Index of the tower this disk is currently on (0 = Tower A, 1 = Tower B, 2 = Tower C).
        /// </summary>
        public int TowerIndex { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new disk with specified size and starting tower.
        /// </summary>
        /// <param name="size">Size of the disk (1 = smallest)</param>
        /// <param name="startTower">Initial tower index (0-2)</param>
        public DiskModel(int size, int startTower)
        {
            Size = size;
            TowerIndex = startTower;
        }

        #endregion
    }
}
