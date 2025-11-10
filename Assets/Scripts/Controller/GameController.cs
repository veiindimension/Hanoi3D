// 07/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections.Generic;
using UnityEngine;
using Hanoi.Model;
using Hanoi.View;
using System.Linq;
using UnityEngine.UI;
using TMPro;


namespace Hanoi.Controller
{
    /// <summary>
    /// Central manager that controls the game logic and handles mouse input (raycast-based).
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene References (assign in Inspector)")]
        [Tooltip("Drag the Transform of TowerA here")]
        [SerializeField] private Transform towerA;
        [Tooltip("Drag the Transform of TowerB here")]
        [SerializeField] private Transform towerB;
        [Tooltip("Drag the Transform of TowerC here")]
        [SerializeField] private Transform towerC;

        [Tooltip("Disk prefab (assign prefab with DiskView component)")]
        [SerializeField] private GameObject diskPrefab;

        [Header("Settings")]
        [SerializeField, Range(3, 10)] public int diskCount = 4;
        [SerializeField] private float diskVerticalGap = 0.02f;
        [Tooltip("If true, disks will be spawned in order on the first tower.")]
        [SerializeField] public bool spawnDisksOrdered = true;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI movesCounterText; //Refers to the text of the UI



        // Logical model
        private GameModel gameModel;
        private List<Transform> towerTransforms = new List<Transform>();

        // Input/selection state
        private DiskView hoveredDisk = null;
        private DiskView selectedDisk = null;

        // Camera cache
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogError("[GameController] Camera.main is null. Make sure your camera has the 'MainCamera' tag.");

            if (towerA == null || towerB == null || towerC == null)
                Debug.LogWarning("[GameController] One or more tower references are not assigned in the Inspector.");
            if (diskPrefab == null)
                Debug.LogWarning("[GameController] Disk prefab not assigned in the Inspector.");
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            gameModel = new GameModel();
            gameModel.Initialize(diskCount);

            towerTransforms.Clear();
            towerTransforms.Add(towerA);
            towerTransforms.Add(towerB);
            towerTransforms.Add(towerC);

            SpawnDisks();
            UpdateMoveCountUI();
        }

        private void SpawnDisks()
        {
            TowerModel firstTower = gameModel.Towers[0];
            List<DiskModel> disks = firstTower.Disks.ToList();

            if (!spawnDisksOrdered)
            {
                System.Random random = new System.Random();
                disks = disks.OrderBy(d => random.Next()).ToList();
            }
            else
            {
                disks = disks.OrderByDescending(d => d.Size).ToList();
            }

            firstTower.Clear();

            float cumulativeHeight = 0f;
            foreach (DiskModel diskModel in disks)
            {
                GameObject newDisk = Instantiate(diskPrefab);
                newDisk.name = "Disk_" + diskModel.Size;

                DiskView view = newDisk.GetComponent<DiskView>();
                if (view == null)
                {
                    Debug.LogError("[GameController] Disk prefab is missing the DiskView component.");
                    continue;
                }

                view.Initialize(diskModel, this);

                float diskHeight = newDisk.GetComponentInChildren<Renderer>().bounds.size.y;
                Vector3 towerBase = towerTransforms[0].position;
                float yPos = towerBase.y + cumulativeHeight + diskHeight * 0.5f;
                Vector3 pos = new Vector3(towerBase.x, yPos, towerBase.z);

                newDisk.transform.position = pos;
                cumulativeHeight += diskHeight + diskVerticalGap;

                firstTower.Push(diskModel);
            }

            Debug.Log("[GameController] Spawned " + disks.Count + " disks on Tower 0.");
        }

        private void Update()
        {
            HandleMouseRaycast();
            HandleMouseClickRelease();

            if (gameModel.IsGameComplete())
            {
                Debug.Log($"[GameController] Victory! All disks are stacked in order on the third tower in {gameModel.MoveCount} moves.");
                ShowVictoryScreen();
            }
        }

        private void HandleMouseRaycast()
        {
            if (mainCamera == null) return;

            // Cast a ray from the mouse position on the screen
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

        private void HandleMouseClickRelease()
        {
            // When left mouse button is pressed
            if (Input.GetMouseButtonDown(0))
            {
                if (hoveredDisk != null)
                {
                    if (CanSelectDisk(hoveredDisk))
                    {
                        selectedDisk = hoveredDisk;
                        selectedDisk.OnPick();
                        Debug.Log("[GameController] Disk selected: " + selectedDisk.GetModel().Size);
                    }
                    else
                    {
                        Debug.Log("[GameController] Disk cannot be selected (it's not on top).");
                    }
                }
            }

            // When left mouse button is released
            if (Input.GetMouseButtonUp(0))
            {
                if (selectedDisk != null)
                {
                    selectedDisk.OnRelease();
                    Debug.Log("[GameController] Disk released: " + selectedDisk.GetModel().Size);
                    selectedDisk = null;
                }
            }
        }

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

            DiskModel topDisk = fromTower.Peek();
            if (topDisk != model)
            {
                Debug.LogWarning($"[GameController] Attempted to move a disk that's not on top of Tower {fromIndex}");
                return;
            }

            DiskModel targetTopDisk = toTower.Peek();
            if (targetTopDisk != null && model.Size > targetTopDisk.Size)
            {
                Debug.LogWarning($"[GameController] Cannot place disk {model.Size} on top of disk {targetTopDisk.Size} in Tower {targetTowerIndex}. Returning to initial position.");
                disk.ResetPosition();
                return;
            }

            fromTower.Pop();
            toTower.Push(model);
            model.TowerIndex = targetTowerIndex;

            //gameModel.MoveDisk(fromIndex, targetTowerIndex);

            UpdateMoveCountUI(); // Update UI for the move counter

            Debug.Log($"[GameController] Disk {model.Size} moved from Tower {fromIndex} → Tower {targetTowerIndex}");
        }

        public void UpdateMoveCountUI()
        {
            // Find the MovesCounterText object in the scene
            var movesCounterText = GameObject.Find("MovesCounterText").GetComponent<TextMeshProUGUI>();
            if (movesCounterText != null)
            {
                // Check if the game is complete
                if (gameModel.IsGameComplete())
                {
                    // Update the text to display the victory message
                    movesCounterText.text = $"You won! With {gameModel.DiskCount} disks and {gameModel.MoveCount} moves!";
                }
                else
                {
                    // Update the text with the current move count
                    movesCounterText.text = $"Moves: {gameModel.MoveCount}";
                }
            }
            else
            {
                Debug.LogError("[GameController] MovesCounterText not found in the scene or missing TextMeshProUGUI component.");
            }
        }

        public void ResetDisksToInitialPositions()
        {
            if (gameModel != null)
            {
                gameModel.Initialize(gameModel.DiskCount);

                var diskViews = FindObjectsOfType<DiskView>();
                foreach (var diskView in diskViews)
                {
                    diskView.ResetPosition();
                }

                Debug.Log("[GameController] Dischi resettati alle posizioni iniziali.");
            }
            else
            {
                Debug.LogError("[GameController] GameModel non inizializzato.");
            }
        }
        /// <summary>
        /// Metodo per resettare tutti i dischi alle loro posizioni iniziali.
        /// </summary>
        public void ResetAllDisks()
        {
            if (gameModel != null)
            {
                // Reinitialize the game model
                gameModel.Initialize(gameModel.DiskCount);

                // Reset the move count
                gameModel.ResetMoveCount();
                UpdateMoveCountUI();

                // Clear all towers in the logical model
                foreach (var tower in gameModel.Towers)
                {
                    tower.Clear();
                }

                // Find all DiskView instances in the scene and sort them by size (largest to smallest)
                var diskViews = FindObjectsOfType<DiskView>().OrderByDescending(diskView => diskView.GetModel().Size).ToList();

                float cumulativeHeight = 0f;
                foreach (var diskView in diskViews)
                {
                    // Get the disk model
                    DiskModel diskModel = diskView.GetModel();
                    if (diskModel != null)
                    {
                        // Update the disk's position in the model
                        diskModel.TowerIndex = 0;

                        // Push the disk into the logical model's Tower 0
                        gameModel.Towers[0].Push(diskModel);

                        // Calculate the initial position of the disk
                        Transform towerTransform = GetTowerTransforms()[0];
                        float diskHeight = diskView.GetComponentInChildren<Renderer>().bounds.size.y;
                        float yPos = towerTransform.position.y + cumulativeHeight + diskHeight * 0.5f;
                        Vector3 initialPosition = new Vector3(towerTransform.position.x, yPos, towerTransform.position.z);

                        // Set the disk's position
                        diskView.transform.position = initialPosition;

                        // Reset the rigidbody velocities
                        Rigidbody rb = diskView.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                        }

                        cumulativeHeight += diskHeight + 0.1f; // 0.1f is the vertical gap between disks
                    }
                }

                Debug.Log("[GameController] All disks have been reset to their initial positions, move counter reset, and logical model updated.");
            }
            else
            {
                Debug.LogError("[GameController] GameModel is not initialized.");
            }
        }


        public void ToggleSpawnDisksOrdered()
        {
            spawnDisksOrdered = !spawnDisksOrdered;
            Debug.Log($"[GameController] spawnDisksOrdered set to: {spawnDisksOrdered}");
        }


        /// <summary>
        /// Resets the game with the number of disks selected by the DiskSelector.
        /// </summary>
        public void ResetGameWithDiskCount()
        {
            if (gameModel != null)
            {
                // Reinitialize the game model with the current diskCount
                gameModel.Initialize(diskCount);

                // Reset the move count
                gameModel.ResetMoveCount();
                UpdateMoveCountUI();

                // Clear all towers in the logical model
                foreach (var tower in gameModel.Towers)
                {
                    tower.Clear();
                }

                // Find all DiskView instances in the scene and destroy them
                var existingDiskViews = FindObjectsOfType<DiskView>();
                foreach (var diskView in existingDiskViews)
                {
                    Destroy(diskView.gameObject);
                }

                // Generate disk order based on spawnDisksOrdered
                int[] diskOrder = spawnDisksOrdered
                    ? Enumerable.Range(1, diskCount).Reverse().ToArray() // Largest to smallest
                    : Enumerable.Range(1, diskCount).OrderBy(x => Random.value).ToArray(); // Random order

                // Spawn new DiskView instances based on diskOrder
                float cumulativeHeight = 0f;
                foreach (int size in diskOrder)
                {
                    // Create a new DiskModel with the correct size and starting tower index
                    DiskModel newDiskModel = new DiskModel(size, 0);

                    // Push the disk into the logical model's Tower 0
                    gameModel.Towers[0].Push(newDiskModel);

                    // Instantiate a new DiskView
                    GameObject newDisk = Instantiate(diskPrefab);
                    DiskView newDiskView = newDisk.GetComponent<DiskView>();
                    if (newDiskView != null)
                    {
                        // Initialize the DiskView with the DiskModel and GameController
                        newDiskView.Initialize(newDiskModel, this);

                        // Calculate the initial position of the disk
                        Transform towerTransform = GetTowerTransforms()[0];
                        float diskHeight = newDiskView.GetComponentInChildren<Renderer>().bounds.size.y;
                        float yPos = towerTransform.position.y + cumulativeHeight + diskHeight * 0.5f;
                        Vector3 initialPosition = new Vector3(towerTransform.position.x, yPos, towerTransform.position.z);

                        // Set the disk's position
                        newDisk.transform.position = initialPosition;

                        // Reset the rigidbody velocities
                        Rigidbody rb = newDisk.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                        }

                        cumulativeHeight += diskHeight + diskVerticalGap; // Add vertical gap between disks
                    }
                }

                Debug.Log($"[GameController] All disks have been reset to their initial positions, move counter reset, and logical model updated with {diskCount} disks.");
            }
            else
            {
                Debug.LogError("[GameController] GameModel is not initialized.");
            }
        }

        private void ShowVictoryScreen()
        {
            if (movesCounterText != null)
            {
                movesCounterText.text = $"You won!\nWith {gameModel.DiskCount} disks and {gameModel.MoveCount} moves!";
            }
            else
            {
                Debug.LogError("[GameController] MovesCounterText not found in the scene or missing TextMeshProUGUI component.");
            }

            Debug.Log($"[GameController] Victory! All disks are stacked in order on TowerC in {gameModel.MoveCount} moves.");
        }
    }
}
