using System.Collections.Generic;
using UnityEngine;
using Hanoi.Model;
using Hanoi.View;
using System.Linq;

namespace Hanoi.Controller
{
    /// <summary>
    /// Central manager that controls logic and handles mouse input (raycast-based).
    /// This version manages hover, click and release centrally to avoid OnMouse* issues.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene References (assign in Inspector)")]
        [Tooltip("Drag TowerA Transform here")]
        [SerializeField] private Transform towerA;
        [Tooltip("Drag TowerB Transform here")]
        [SerializeField] private Transform towerB;
        [Tooltip("Drag TowerC Transform here")]
        [SerializeField] private Transform towerC;

        [Tooltip("Disk prefab (assign prefab with DiskView component)")]
        [SerializeField] private GameObject diskPrefab;

        [Header("Settings")]
        [SerializeField, Range(3, 10)] private int diskCount = 4;
        [SerializeField] private float diskVerticalGap = 0.02f;

        // Logical model
        private GameModel gameModel;
        private List<Transform> towerTransforms = new List<Transform>();

        // Input / selection state
        private DiskView hoveredDisk = null;
        private DiskView selectedDisk = null;

        // Cached camera
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogError("[GameController] Camera.main is null. Make sure your camera has tag 'MainCamera'.");

            if (towerA == null || towerB == null || towerC == null)
                Debug.LogWarning("[GameController] One or more tower references are not assigned in the Inspector.");
            if (diskPrefab == null)
                Debug.LogWarning("[GameController] Disk prefab not assigned in Inspector.");
        }

        private void Start()
        {
            InitializeGame();
        }

        /// <summary>
        /// Initializes the logical and visual game elements.
        /// </summary>
        private void InitializeGame()
        {
            gameModel = new GameModel();
            gameModel.Initialize(diskCount);

            towerTransforms.Clear();
            towerTransforms.Add(towerA);
            towerTransforms.Add(towerB);
            towerTransforms.Add(towerC);

            SpawnDisks();
        }

        /// <summary>
        /// Spawns disks visually on Tower A according to model data.
        /// </summary>
        private void SpawnDisks()
        {
            TowerModel firstTower = gameModel.Towers[0];
            List<DiskModel> disks = firstTower.Disks.ToList();
            disks.Reverse(); // bottom → top

            float cumulativeHeight = 0f;
            foreach (DiskModel diskModel in disks)
            {
                GameObject newDisk = Instantiate(diskPrefab);
                newDisk.name = "Disk_" + diskModel.Size;

                DiskView view = newDisk.GetComponent<DiskView>();
                if (view == null)
                {
                    Debug.LogError("[GameController] Disk prefab missing DiskView component.");
                    continue;
                }

                view.Initialize(diskModel, this);

                float diskHeight = newDisk.GetComponentInChildren<Renderer>().bounds.size.y;
                Vector3 towerBase = towerTransforms[0].position;
                float yPos = towerBase.y + cumulativeHeight + diskHeight * 0.5f;
                Vector3 pos = new Vector3(towerBase.x, yPos, towerBase.z);

                newDisk.transform.position = pos;
                cumulativeHeight += diskHeight + diskVerticalGap;
            }

            Debug.Log("[GameController] Spawned " + disks.Count + " disks on Tower 0.");
        }

        private void Update()
        {
            HandleMouseRaycast();
            HandleMouseClickRelease();
        }

        // =======================================================
        // HOVER DETECTION
        // =======================================================
        private void HandleMouseRaycast()
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                DiskView hitDisk = hit.collider.GetComponentInParent<DiskView>();
                if (hitDisk != hoveredDisk)
                {
                    if (hoveredDisk != null)
                        hoveredDisk.OnHoverExit();

                    hoveredDisk = hitDisk;

                    if (hoveredDisk != null)
                    {
                        bool canPick = CanSelectDisk(hoveredDisk);
                        hoveredDisk.OnHoverEnter(canPick);
                    }
                }
            }
            else
            {
                if (hoveredDisk != null)
                {
                    hoveredDisk.OnHoverExit();
                    hoveredDisk = null;
                }
            }
        }

        // =======================================================
        // CLICK / RELEASE HANDLING
        // =======================================================
        private void HandleMouseClickRelease()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hoveredDisk != null)
                {
                    if (CanSelectDisk(hoveredDisk))
                    {
                        selectedDisk = hoveredDisk;
                        selectedDisk.OnPick();
                        Debug.Log("[GameController] Selected disk: " + selectedDisk.GetModel().Size);
                    }
                    else
                    {
                        Debug.Log("[GameController] Disk hovered but not selectable (not top).");
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (selectedDisk != null)
                {
                    selectedDisk.OnRelease();
                    Debug.Log("[GameController] Released disk: " + selectedDisk.GetModel().Size);
                    selectedDisk = null;
                }
            }
        }

        // =======================================================
        // UTILITY & LOGIC HELPERS
        // =======================================================
        public bool CanSelectDisk(DiskView disk)
        {
            if (disk == null || gameModel == null) return false;
            DiskModel model = disk.GetModel();
            int towerIndex = model.TowerIndex;

            if (towerIndex < 0 || towerIndex >= gameModel.Towers.Length) return false;
            DiskModel top = gameModel.Towers[towerIndex].Peek();
            return top == model;
        }

        public List<Transform> GetTowerTransforms() => towerTransforms;

        public GameModel GetGameModel() => gameModel;

        // =======================================================
        // LOGIC UPDATE WHEN A DISK IS MOVED BETWEEN TOWERS
        // =======================================================
        public void MoveDiskToTower(DiskView disk, int targetTowerIndex)
        {
            if (gameModel == null) return;

            DiskModel model = disk.GetModel();
            int fromIndex = model.TowerIndex;

            if (targetTowerIndex < 0 || targetTowerIndex >= gameModel.Towers.Length)
                return;
            if (fromIndex == targetTowerIndex)
                return;

            TowerModel fromTower = gameModel.Towers[fromIndex];
            TowerModel toTower = gameModel.Towers[targetTowerIndex];

            // --- Remove from source tower if it's the top disk ---
            DiskModel topDisk = fromTower.Peek();
            if (topDisk != model)
            {
                Debug.LogWarning($"[GameController] Tried to move non-top disk from Tower {fromIndex}");
                return;
            }

            fromTower.Pop(); // remove from source
            toTower.Push(model); // add to destination
            model.TowerIndex = targetTowerIndex;

            Debug.Log($"[GameController] Disk {model.Size} moved from Tower {fromIndex} → Tower {targetTowerIndex}");
        }
    }
}
