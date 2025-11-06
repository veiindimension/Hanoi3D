using System.Collections.Generic;
using UnityEngine;
using Hanoi.Model;
using Hanoi.View;

namespace Hanoi.Controller
{
    /// <summary>
    /// Central manager that controls the logic and interactions of the Tower of Hanoi game.
    /// This version uses towers that already exist in the Unity scene (not generated via prefab).
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform towerA;
        [SerializeField] private Transform towerB;
        [SerializeField] private Transform towerC;

        [Tooltip("The prefab used to spawn disks.")]
        [SerializeField] private GameObject diskPrefab;

        [Header("Game Settings")]
        [SerializeField, Range(3, 10)] private int diskCount = 4;
        [SerializeField] private float diskVerticalGap = 0.02f; // tiny gap to avoid z-fighting/collision

        private GameModel gameModel;
        private List<Transform> towerTransforms = new List<Transform>();
        private DiskView selectedDisk = null;

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            gameModel = new GameModel();
            gameModel.Initialize(diskCount);

            // register towers
            towerTransforms.Clear();
            towerTransforms.Add(towerA);
            towerTransforms.Add(towerB);
            towerTransforms.Add(towerC);

            SpawnDisks();
        }

        /// <summary>
        /// Spawn disks on the first tower.
        /// This version stacks disks using each disk's actual world height (from renderer.bounds),
        /// so scaling randomization won't produce overlaps.
        /// </summary>
        private void SpawnDisks()
        {
            TowerModel firstTower = gameModel.Towers[0];

            // cumulative vertical position above the tower's base
            float cumulativeHeight = 0f;

            // iterate through disks from bottom->top (remember stack enumeration yields top first,
            // so we must ensure the model.Disks enumerable provides correct order; if it yields top->bottom,
            // we can collect to list and reverse. Here we assume DiskModel were pushed so enumeration is LIFO,
            // so safer to collect and reverse to spawn bottom->top)
            List<DiskModel> disks = new List<DiskModel>(firstTower.Disks);
            disks.Reverse(); // now bottom -> top

            foreach (DiskModel diskModel in disks)
            {
                GameObject newDisk = Instantiate(diskPrefab);
                newDisk.name = "Disk_" + diskModel.Size;

                DiskView view = newDisk.GetComponent<DiskView>();
                view.Initialize(diskModel, this);

                // compute vertical position: base tower position + cumulativeHeight + half of current disk height
                float diskHeight = newDisk.GetComponentInChildren<Renderer>().bounds.size.y;
                Vector3 towerBase = towerTransforms[0].position;

                // place disk so its bottom sits at towerBase.y + cumulativeHeight
                float yPos = towerBase.y + cumulativeHeight + diskHeight * 0.5f;

                Vector3 pos = new Vector3(towerBase.x, yPos, towerBase.z);
                newDisk.transform.position = pos;

                // update cumulative height for next disk: add full height + tiny gap
                cumulativeHeight += diskHeight + diskVerticalGap;
            }
        }

        // (interaction methods unchanged)
        public void OnDiskHovered(DiskView hoveredDisk)
        {
            DiskModel model = hoveredDisk.GetModel();
            int towerIndex = model.TowerIndex;

            DiskModel topDisk = gameModel.Towers[towerIndex].Peek();

            if (topDisk == model)
                hoveredDisk.ShowOutline(Color.white);
            else
                hoveredDisk.ShowOutline(Color.red);
        }

        public void OnDiskSelected(DiskView clickedDisk)
        {
            DiskModel model = clickedDisk.GetModel();
            int towerIndex = model.TowerIndex;
            DiskModel topDisk = gameModel.Towers[towerIndex].Peek();

            if (topDisk != model)
            {
                Debug.Log("❌ Disk not selectable: not at top of tower");
                return;
            }

            if (selectedDisk == clickedDisk)
            {
                selectedDisk.HideOutline();
                selectedDisk = null;
                return;
            }

            selectedDisk = clickedDisk;
            clickedDisk.ShowOutline(Color.yellow);
            Debug.Log("✅ Disk selected: " + model.Size);
        }

        // Helpers (optional)
        public List<Transform> GetTowerTransforms()
        {
            return towerTransforms;
        }

        public GameModel GetGameModel()
        {
            return gameModel;
        }

        public bool CanSelectDisk(DiskView disk)
        {
            DiskModel model = disk.GetModel();
            int towerIndex = model.TowerIndex;
            DiskModel topDisk = gameModel.Towers[towerIndex].Peek();

            // Solo il disco in cima alla torre è selezionabile
            return topDisk == model;
        }


    }
}
