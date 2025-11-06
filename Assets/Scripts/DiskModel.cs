using System;

namespace Hanoi.Model ///better organization of the code, this script represents the logic part of the code
{
    /// data-only model representing a single disk in the Tower of Hanoi.
    /// stores size and its current tower index.
    [Serializable]  ///imported from System, it allows to make variables updatable in real time
    public class DiskModel 
    {
        public int Size { get; private set; }  /// 1 = smallest, the size of the disk, set is private because we don't need to change it during the game
        public int TowerIndex { get; set; }    /// 0, 1, 2 which tower the disk will be starting at

        public DiskModel(int size, int startTower)  /// basic constructor
        {
            Size = size;
            TowerIndex = startTower;
        }
    }
}
