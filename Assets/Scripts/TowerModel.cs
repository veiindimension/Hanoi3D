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
        /// Returns all disks as an enumerable collection (bottom to top).
        /// </summary>
        public IEnumerable<DiskModel> Disks => disks;

        /// <summary>
        /// Adds a disk to the top of the tower.
        /// </summary>
        public void Push(DiskModel disk)
        {
            disks.Push(disk);
        }

        /// <summary>
        /// Removes and returns the top disk from this tower.
        /// Returns null if the tower is empty.
        /// </summary>
        public DiskModel Pop()
        {
            return disks.Count > 0 ? disks.Pop() : null;
        }

        /// <summary>
        /// Returns (but does not remove) the top disk from the tower.
        /// Returns null if the tower is empty.
        /// </summary>
        public DiskModel Peek()
        {
            return disks.Count > 0 ? disks.Peek() : null;
        }

        /// <summary>
        /// Clears the tower of all disks.
        /// </summary>
        public void Clear()
        {
            disks.Clear();
        }
    }
}
