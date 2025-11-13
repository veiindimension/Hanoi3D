using System.Collections.Generic;
using UnityEngine;
using Hanoi.Model;
using Hanoi.View;
using System.Linq;
using TMPro;

namespace Hanoi.Controller
{
    /// <summary>
    /// Central game controller managing the Tower of Hanoi game loop.
    /// Handles initialization, user input (mouse raycasting), disk movement validation,
    /// win condition checking, and UI updates.
    /// Part of the MVC Controller layer - orchestrates Model and View interactions.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        #region Serialized Fields - Scene References

        [Header("Tower References")]
        [Tooltip("Transform of Tower A (left)")]
        [SerializeField] private Transform towerA;
        
        [Tooltip("Transform of Tower B (center)")]
        [SerializeField] private Transform towerB;
        
        [Tooltip("Transform of Tower C (right)")]
        [SerializeField] private Transform towerC;

        [Header("Prefab")]
        [Tooltip("Disk prefab with DiskView component")]
        [SerializeField] private GameObject diskPrefab;

        [Header("Game Settings")]
        [Tooltip("Number of disks to use in the game (3-10)")]
        [SerializeField, Range(3, 10)] public int diskCount = 4;
        
        [Tooltip("Vertical spacing between stacked disks")]
        [SerializeField] private float diskVerticalGap = 0.02f;
        
        [Tooltip("If true, disks spawn in order (largest to smallest). If false, random order.")]
        [SerializeField] public bool spawnDisksOrdered = true;

        [Header("UI")]
        [Tooltip("TextMeshPro text displaying move counter")]
        [SerializeField] private TextMeshProUGUI movesCounterText;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip yaySoundEffect;     // Victory sound
        [SerializeField] private AudioClip diskHoldSFX;        // Pickup sound
        [SerializeField] private AudioClip diskReleaseSFX;     // Release sound
        [SerializeField] private AudioClip wrongClickSFX;      // Invalid action sound

        #endregion

        #region Private Fields

        // Game state
        private GameModel gameModel;
        private List<Transform> towerTransforms = new List<Transform>();

        // Input state
        private DiskView hoveredDisk = null;
        private DiskView selectedDisk = null;

        // UI state
        private bool hasVictoryScreenShown = false;

        // Cached components
        private Camera mainCamera;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache main camera
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[GameController] Main Camera not found. Ensure camera is tagged 'MainCamera'.");
            }

            // Validate inspector references
            if (towerA == null || towerB == null || towerC == null)
            {
                Debug.LogError("[GameController] One or more tower references missing in Inspector.");
            }

            if (diskPrefab == null)
            {
                Debug.LogError("[GameController] Disk prefab not assigned in Inspector.");
            }
        }

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            HandleMouseRaycast();
            HandleMouseClickRelease();

            // Check for victory condition
            if (gameModel.IsGameComplete() && !hasVictoryScreenShown)
            {
                hasVictoryScreenShown = true;
                ShowVictoryScreen();
            }
        }

        #endregion

        #region Game Initialization

        /// <summary>
        /// Initializes the game model, tower references, and spawns disks.
        /// </summary>
        private void InitializeGame()
        {
            // Create and initialize logical model
            gameModel = new GameModel();
            gameModel.Initialize(diskCount);

            // Setup tower transform list
            towerTransforms.Clear();
            towerTransforms.Add(towerA);
            towerTransforms.Add(towerB);
            towerTransforms.Add(towerC);

            // Spawn visual disk GameObjects
            SpawnDisks();
            
            // Update UI
            UpdateMoveCountUI();
        }

        /// <summary>
        /// Instantiates disk GameObjects and positions them on Tower A.
        /// Respects the spawnDisksOrdered flag for ordered or random placement.
        /// </summary>
        private void SpawnDisks()
        {
            TowerModel firstTower = gameModel.Towers[0];
            List<DiskModel> disks = firstTower.Disks.ToList();

            // Determine spawn order
            if (!spawnDisksOrdered)
            {
                // Random order
                System.Random random = new System.Random();
                disks = disks.OrderBy(d => random.Next()).ToList();
            }
            else
            {
                // Ordered: largest to smallest (bottom to top)
                disks = disks.OrderByDescending(d => d.Size).ToList();
            }

            // Clear tower and respawn
            firstTower.Clear();

            float cumulativeHeight = 0f;
            foreach (DiskModel diskModel in disks)
            {
                // Instantiate disk prefab
                GameObject newDisk = Instantiate(diskPrefab);
                newDisk.name = "Disk_" + diskModel.Size;

                // Get DiskView component
                DiskView view = newDisk.GetComponent<DiskView>();
                if (view == null)
                {
                    Debug.LogError("[GameController] Disk prefab missing DiskView component.");
                    continue;
                }

                // Initialize view with model
                view.Initialize(diskModel, this);

                // Calculate position on Tower A
                float diskHeight = newDisk.GetComponentInChildren<Renderer>().bounds.size.y;
                Vector3 towerBase = towerTransforms[0].position;
                float yPos = towerBase.y + cumulativeHeight + diskHeight * 0.5f;
                Vector3 position = new Vector3(towerBase.x, yPos, towerBase.z);

                newDisk.transform.position = position;
                cumulativeHeight += diskHeight + diskVerticalGap;

                // Add to logical model
                firstTower.Push(diskModel);
            }

            Debug.Log($"[GameController] Spawned {disks.Count} disks on Tower A.");
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// Performs mouse raycasting to detect disk hover.
        /// Updates hover state and visual feedback accordingly.
        /// </summary>
        private void HandleMouseRaycast()
        {
            if (mainCamera == null) return;

            // Cast ray from mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                // Check if hit object is a disk
                DiskView hitDisk = hit.collider.GetComponentInParent<DiskView>();
                
                // Update hover state
                if (hitDisk != hoveredDisk)
                {
                    // Exit previous hover
                    if (hoveredDisk != null)
                    {
                        hoveredDisk.OnHoverExit();
                    }

                    // Enter new hover
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
                // No disk hit - clear hover
                if (hoveredDisk != null)
                {
                    hoveredDisk.OnHoverExit();
                    hoveredDisk = null;
                }
            }
        }

        /// <summary>
        /// Handles mouse button press and release for disk selection.
        /// </summary>
        private void HandleMouseClickRelease()
        {
            // Mouse button pressed - attempt to select disk
            if (Input.GetMouseButtonDown(0))
            {
                if (hoveredDisk != null)
                {
                    if (CanSelectDisk(hoveredDisk))
                    {
                        selectedDisk = hoveredDisk;
                        selectedDisk.OnPick();
                        Debug.Log($"[GameController] Disk {selectedDisk.GetModel().Size} selected.");
                    }
                    else
                    {
                        Debug.Log("[GameController] Cannot select disk - not on top of tower.");
                    }
                }
            }

            // Mouse button released - release selected disk
            if (Input.GetMouseButtonUp(0))
            {
                if (selectedDisk != null)
                {
                    Debug.Log($"[GameController] Disk {selectedDisk.GetModel().Size} released.");
                    selectedDisk.OnRelease();
                    selectedDisk = null;
                }
            }
        }

        #endregion

        #region Game Logic

        /// <summary>
        /// Checks if a disk can be selected (must be on top of its tower).
        /// </summary>
        /// <param name="disk">The disk to check</param>
        /// <returns>True if disk is on top and can be selected</returns>
        public bool CanSelectDisk(DiskView disk)
        {
            if (disk == null || gameModel == null) return false;

            DiskModel model = disk.GetModel();
            int towerIndex = model.TowerIndex;

            // Validate tower index
            if (towerIndex < 0 || towerIndex >= gameModel.Towers.Length) return false;

            // Check if this disk is on top
            DiskModel topDisk = gameModel.Towers[towerIndex].Peek();
            return topDisk == model;
        }

        /// <summary>
        /// Moves a disk to a target tower in the logical model.
        /// Validates the move according to Hanoi rules.
        /// </summary>
        /// <param name="disk">The disk view to move</param>
        /// <param name="targetTowerIndex">Target tower index (0-2)</param>
        public void MoveDiskToTower(DiskView disk, int targetTowerIndex)
        {
            if (gameModel == null) return;

            DiskModel model = disk.GetModel();
            int fromIndex = model.TowerIndex;

            // Validate indices
            if (targetTowerIndex < 0 || targetTowerIndex >= gameModel.Towers.Length) return;
            if (fromIndex == targetTowerIndex) return;

            TowerModel fromTower = gameModel.Towers[fromIndex];
            TowerModel toTower = gameModel.Towers[targetTowerIndex];

            // Verify disk is on top of source tower
            DiskModel topDisk = fromTower.Peek();
            if (topDisk != model)
            {
                Debug.LogWarning($"[GameController] Cannot move Disk {model.Size} - not on top of Tower {fromIndex}.");
                return;
            }

            // Validate Hanoi rule: can't place larger disk on smaller disk
            DiskModel targetTopDisk = toTower.Peek();
            if (targetTopDisk != null && model.Size > targetTopDisk.Size)
            {
                Debug.LogWarning($"[GameController] Invalid move: Disk {model.Size} > Disk {targetTopDisk.Size}.");
                disk.ResetPosition();
                return;
            }

            // Execute move in logical model
            fromTower.Pop();
            toTower.Push(model);
            model.TowerIndex = targetTowerIndex;

            Debug.Log($"[GameController] Disk {model.Size} moved: Tower {fromIndex} → Tower {targetTowerIndex}");
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Updates the move counter UI text.
        /// Shows victory message when game is complete.
        /// </summary>
        public void UpdateMoveCountUI()
        {
            // Find UI text in scene (cached reference preferred, but this works)
            var moveText = GameObject.Find("MovesCounterText")?.GetComponent<TextMeshProUGUI>();
            
            if (moveText != null)
            {
                if (gameModel.IsGameComplete())
                {
                    moveText.text = $"You won!\nWith {gameModel.DiskCount} disks and {gameModel.MoveCount} moves!";
                }
                else
                {
                    moveText.text = $"Moves: {gameModel.MoveCount}";
                }
            }
            else
            {
                Debug.LogError("[GameController] MovesCounterText not found in scene.");
            }
        }

        /// <summary>
        /// Displays victory screen with congratulations message and plays victory sound.
        /// </summary>
        private void ShowVictoryScreen()
        {
            // Update UI text
            if (movesCounterText != null)
            {
                movesCounterText.text = $"You won!\nWith {gameModel.DiskCount} disks and {gameModel.MoveCount} moves!";
            }

            // Play victory sound
            if (audioSource != null && yaySoundEffect != null)
            {
                audioSource.PlayOneShot(yaySoundEffect);
            }

            Debug.Log($"[GameController] Victory! Game completed in {gameModel.MoveCount} moves.");
        }

        #endregion

        #region Game Reset

        /// <summary>
        /// Resets the entire game with current disk count.
        /// Destroys existing disks and spawns new ones.
        /// Called by external buttons (Reset, DiskSelector).
        /// </summary>
        public void ResetGameWithDiskCount()
        {
            if (gameModel == null)
            {
                Debug.LogError("[GameController] GameModel not initialized.");
                return;
            }

            // Reinitialize model
            gameModel.Initialize(diskCount);
            gameModel.ResetMoveCount();
            UpdateMoveCountUI();

            // Clear all towers
            foreach (var tower in gameModel.Towers)
            {
                tower.Clear();
            }

            // Destroy existing disk GameObjects
            var existingDisks = FindObjectsByType<DiskView>(FindObjectsSortMode.None);
            foreach (var disk in existingDisks)
            {
                Destroy(disk.gameObject);
            }

            // Generate spawn order
            int[] diskOrder = spawnDisksOrdered
                ? Enumerable.Range(1, diskCount).Reverse().ToArray()  // Ordered
                : Enumerable.Range(1, diskCount).OrderBy(x => Random.value).ToArray();  // Random

            // Spawn new disks
            float cumulativeHeight = 0f;
            foreach (int size in diskOrder)
            {
                // Create logical model
                DiskModel newDiskModel = new DiskModel(size, 0);
                gameModel.Towers[0].Push(newDiskModel);

                // Instantiate GameObject
                GameObject newDisk = Instantiate(diskPrefab);
                DiskView newDiskView = newDisk.GetComponent<DiskView>();
                
                if (newDiskView != null)
                {
                    newDiskView.Initialize(newDiskModel, this);

                    // Position on Tower A
                    Transform towerTransform = towerTransforms[0];
                    float diskHeight = newDiskView.GetComponentInChildren<Renderer>().bounds.size.y;
                    float yPos = towerTransform.position.y + cumulativeHeight + diskHeight * 0.5f;
                    Vector3 position = new Vector3(towerTransform.position.x, yPos, towerTransform.position.z);

                    newDisk.transform.position = position;

                    // Reset physics
                    Rigidbody rb = newDisk.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    cumulativeHeight += diskHeight + diskVerticalGap;
                }
            }

            hasVictoryScreenShown = false;
            Debug.Log($"[GameController] Game reset with {diskCount} disks.");
        }

        /// <summary>
        /// Resets all disks to their initial positions on Tower A.
        /// Maintains existing disk GameObjects and move counter.
        /// </summary>
        public void ResetAllDisks()
        {
            if (gameModel == null)
            {
                Debug.LogError("[GameController] GameModel not initialized.");
                return;
            }

            // Reset model
            gameModel.Initialize(diskCount);
            gameModel.ResetMoveCount();
            UpdateMoveCountUI();

            // Clear towers
            foreach (var tower in gameModel.Towers)
            {
                tower.Clear();
            }

            // Find and reposition all disk views
            var diskViews = FindObjectsByType<DiskView>(FindObjectsSortMode.None)
                .OrderByDescending(d => d.GetModel().Size)
                .ToList();

            float cumulativeHeight = 0f;
            foreach (var diskView in diskViews)
            {
                DiskModel diskModel = diskView.GetModel();
                if (diskModel != null)
                {
                    // Update logical model
                    diskModel.TowerIndex = 0;
                    gameModel.Towers[0].Push(diskModel);

                    // Reposition disk
                    Transform towerTransform = towerTransforms[0];
                    float diskHeight = diskView.GetComponentInChildren<Renderer>().bounds.size.y;
                    float yPos = towerTransform.position.y + cumulativeHeight + diskHeight * 0.5f;
                    Vector3 position = new Vector3(towerTransform.position.x, yPos, towerTransform.position.z);

                    diskView.transform.position = position;

                    // Reset physics
                    Rigidbody rb = diskView.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    cumulativeHeight += diskHeight + diskVerticalGap;
                }
            }

            Debug.Log("[GameController] All disks reset to Tower A.");
        }

        #endregion

        #region Audio

        /// <summary>
        /// Plays sound effect when disk is picked up.
        /// </summary>
        public void PlayDiskHoldSound()
        {
            if (audioSource != null && diskHoldSFX != null)
            {
                audioSource.PlayOneShot(diskHoldSFX);
            }
        }

        /// <summary>
        /// Plays sound effect when disk is released.
        /// </summary>
        public void PlayDiskReleaseSound()
        {
            if (audioSource != null && diskReleaseSFX != null)
            {
                audioSource.PlayOneShot(diskReleaseSFX);
            }
        }

        /// <summary>
        /// Plays sound effect for invalid action (e.g., invalid placement).
        /// </summary>
        public void PlayWrongClickSound()
        {
            if (audioSource != null && wrongClickSFX != null)
            {
                audioSource.PlayOneShot(wrongClickSFX);
            }
        }

        #endregion

        #region Public Accessors

        /// <summary>
        /// Returns the list of tower transforms (for DiskView positioning).
        /// </summary>
        public List<Transform> GetTowerTransforms() => towerTransforms;

        /// <summary>
        /// Returns the game model (for logic queries).
        /// </summary>
        public GameModel GetGameModel() => gameModel;

        #endregion
    }
}
