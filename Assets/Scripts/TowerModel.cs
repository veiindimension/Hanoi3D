using System.Collections.Generic;

namespace Hanoi.Model
{
    /// <summary>
    /// Represents a single tower in the Tower of Hanoi game.
    /// Each tower acts as a stack that can hold multiple DiskModel objects.
    /// </summary>
    public class TowerModel
    {
        // Internal stack of disks (top of the stack = top of the tower)
        private Stack<DiskModel> disks = new Stack<DiskModel>();

        /// <summary>
        /// Returns how many disks are currently on this tower.
        /// </summary>
        public int Count => disks.Count;

        /// <summary>
        /// Returns an enumerable collection of disks (used for spawning or iteration).
        /// The bottom-most disk is the first pushed; the top-most disk is the last one.
        /// </summary>
        public IEnumerable<DiskModel> Disks => disks;

        /// <summary>
        /// Pushes a disk onto the top of the tower (logical operation).
        /// </summary>
        /// <param name="disk">The disk to place on this tower.</param>
        public void Push(DiskModel disk)
        {
            // Adds a disk to the top of the stack (like putting it on top of the tower)
            disks.Push(disk);
        }

        /// <summary>
        /// Removes and returns the disk from the top of the tower.
        /// </summary>
        /// <returns>The disk that was removed, or null if tower is empty.</returns>
        public DiskModel Pop()
        {
            // If there are disks, remove the top one; otherwise return null
            return disks.Count > 0 ? disks.Pop() : null;
        }

        /// <summary>
        /// Returns (but does not remove) the disk currently on top of the tower.
        /// </summary>
        /// <returns>The top disk, or null if tower is empty.</returns>
        public DiskModel Peek()
        {
            // If the tower has disks, return the top-most; else null
            return disks.Count > 0 ? disks.Peek() : null;
        }

        /// <summary>
        /// Removes all disks from this tower.
        /// </summary>
        public void Clear()
        {
            // Clears the entire stack (used when restarting the game)
            disks.Clear();
        }
    }
}
