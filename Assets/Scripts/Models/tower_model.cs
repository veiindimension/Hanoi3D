using System.Collections.Generic;

namespace Hanoi.Model
{
    /// <summary>
    /// Represents a single tower in the Tower of Hanoi puzzle.
    /// Acts as a stack data structure that holds multiple DiskModel objects.
    /// Part of the MVC Model layer - contains only data and basic stack operations.
    /// </summary>
    public class TowerModel
    {
        #region Fields

        /// <summary>
        /// Internal stack of disks. Top of stack = top disk on the tower.
        /// </summary>
        private Stack<DiskModel> disks = new Stack<DiskModel>();

        #endregion

        #region Properties

        /// <summary>
        /// Returns the current number of disks on this tower.
        /// </summary>
        public int Count => disks.Count;

        /// <summary>
        /// Returns all disks as an enumerable collection (bottom to top order).
        /// Used for iterating over disks without modifying the stack.
        /// </summary>
        public IEnumerable<DiskModel> Disks => disks;

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a disk to the top of this tower.
        /// </summary>
        /// <param name="disk">The disk to add</param>
        public void Push(DiskModel disk)
        {
            disks.Push(disk);
        }

        /// <summary>
        /// Removes and returns the top disk from this tower.
        /// </summary>
        /// <returns>The top disk, or null if tower is empty</returns>
        public DiskModel Pop()
        {
            return disks.Count > 0 ? disks.Pop() : null;
        }

        /// <summary>
        /// Returns (but does not remove) the top disk from this tower.
        /// </summary>
        /// <returns>The top disk, or null if tower is empty</returns>
        public DiskModel Peek()
        {
            return disks.Count > 0 ? disks.Peek() : null;
        }

        /// <summary>
        /// Removes all disks from this tower.
        /// </summary>
        public void Clear()
        {
            disks.Clear();
        }

        #endregion
    }
}
