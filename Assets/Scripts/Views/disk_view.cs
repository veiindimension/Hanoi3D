using UnityEngine;
using Hanoi.Model;
using Hanoi.Controller;
using System.Collections;
using System.Linq;

namespace Hanoi.View
{
    /// <summary>
    /// Visual representation and interaction handler for a single disk.
    /// Handles physics-based dragging, hover feedback, collision detection, and visual effects.
    /// Part of the MVC View layer - bridges between visual GameObject and logical DiskModel.
    /// Uses elastic spring physics for smooth mouse-following behavior.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class DiskView : MonoBehaviour
    {
        #region Fields

        // Model and Controller references
        private DiskModel model;
        private GameController controller;

        // Components
        private Rigidbody rb;
        private Renderer rend;
        private Material baseMaterial;
        private Material[] combinedMaterials;
        private Color outlineColor = Color.green;

        // Drag state
        private bool isHeld = false;
        private Camera mainCamera;

        // Initial position for reset functionality
        private Vector3 initialPosition;

        #endregion

        #region Serialized Fields - Inspector Settings

        [Header("Visual Feedback")]
        [SerializeField] private Material outlineMaterial; // Assign M_Outline material in prefab

        [Header("Elastic Drag Physics")]
        [Tooltip("How strongly the disk is pulled toward the mouse cursor")]
        [SerializeField] private float springStrength = 60f;
        
        [Tooltip("Damping factor to reduce oscillation and smooth movement")]
        [SerializeField] private float springDamping = 10f;
        
        [Tooltip("Z-axis offset for the drag plane (usually 0)")]
        [SerializeField] private float dragPlaneZOffset = 0f;

        [Header("Disk Geometry")]
        [Tooltip("Thickness of all disks (Y scale)")]
        [SerializeField] private float baseThickness = 1.0f;
        
        [Tooltip("Radius of the smallest disk")]
        [SerializeField] private float minRadius = 1.0f;
        
        [Tooltip("Radius of the largest disk")]
        [SerializeField] private float maxRadius = 1.8f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache components
            rb = GetComponent<Rigidbody>();
            rend = GetComponentInChildren<Renderer>();
            mainCamera = Camera.main;

            // Validate components
            if (rend == null)
            {
                Debug.LogError("[DiskView] Renderer not found in children. Check prefab structure.");
            }

            if (mainCamera == null)
            {
                Debug.LogError("[DiskView] Main Camera not found. Ensure your camera is tagged 'MainCamera'.");
            }

            // Cache base material
            baseMaterial = (rend != null) ? rend.material : null;

            // Configure rigidbody for physics-based interaction
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.linearDamping = 1f;
                rb.angularDamping = 0.1f;
            }
        }

        private void FixedUpdate()
        {
            // Apply elastic spring force while disk is being dragged
            if (isHeld)
            {
                ApplyElasticFollow();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the disk with its data model and sets up visual appearance.
        /// Called by GameController after instantiation.
        /// </summary>
        /// <param name="diskModel">The logical model for this disk</param>
        /// <param name="gameController">Reference to the main game controller</param>
        public void Initialize(DiskModel diskModel, GameController gameController)
        {
            model = diskModel;
            controller = gameController;

            // Get total disk count for proportional scaling
            int totalDisks = 1;
            if (controller != null && controller.GetGameModel() != null)
            {
                totalDisks = controller.GetGameModel().DiskCount;
            }

            // Calculate radius based on disk size (proportional to logical size)
            float normalizedSize = (totalDisks > 1) ? (model.Size - 1) / (float)(totalDisks - 1) : 0f;
            float radius = Mathf.Lerp(minRadius, maxRadius, normalizedSize);
            transform.localScale = new Vector3(radius, baseThickness, radius);

            // Setup material array for outline effect
            if (rend != null)
            {
                baseMaterial = rend.material;
                if (outlineMaterial != null)
                {
                    combinedMaterials = new Material[] { baseMaterial, outlineMaterial };
                }
            }

            // Assign random color for visual variety
            if (baseMaterial != null)
            {
                Color randomColor = new Color(
                    Random.Range(0.3f, 1f),
                    Random.Range(0.3f, 1f),
                    Random.Range(0.3f, 1f)
                );
                baseMaterial.color = randomColor;
            }

            // Store initial position for reset functionality
            initialPosition = transform.position;
        }

        #endregion

        #region Hover Visual Feedback

        /// <summary>
        /// Called when mouse cursor enters this disk's collider.
        /// Shows visual feedback (green outline = can pick, red = cannot pick).
        /// </summary>
        /// <param name="canPick">Whether this disk can be selected</param>
        public void OnHoverEnter(bool canPick)
        {
            if (rend == null || outlineMaterial == null) return;

            // Set outline color based on selection validity
            outlineColor = canPick ? Color.green : Color.red;
            outlineMaterial.SetColor("_outline_color", outlineColor);

            // Apply combined materials (base + outline)
            rend.materials = combinedMaterials;
        }

        /// <summary>
        /// Called when mouse cursor exits this disk's collider.
        /// Removes the outline effect.
        /// </summary>
        public void OnHoverExit()
        {
            if (rend == null || baseMaterial == null) return;

            // Restore base material only (remove outline)
            rend.materials = new Material[] { baseMaterial };
        }

        #endregion

        #region Pick Up / Release Interaction

        /// <summary>
        /// Called when player clicks on this disk to pick it up.
        /// Validates selection through GameController before allowing pickup.
        /// </summary>
        public void OnPick()
        {
            if (rb == null) return;

            // Verify disk can be selected (must be on top of its tower)
            if (controller != null && !controller.CanSelectDisk(this))
            {
                Debug.Log("[DiskView] Cannot pick disk - not on top of tower.");
                return;
            }

            isHeld = true;

            // Increase damping for smoother control during drag
            rb.linearDamping = 8f;
            rb.angularDamping = 8f;

            // Remove outline while being held
            if (baseMaterial != null)
            {
                rend.materials = new Material[] { baseMaterial };
            }

            // Play pickup sound
            controller.PlayDiskHoldSound();
        }

        /// <summary>
        /// Called when player releases the disk.
        /// Triggers landing validation after a brief delay for physics to settle.
        /// </summary>
        public void OnRelease()
        {
            if (rb == null) return;

            isHeld = false;

            // Reset damping to default values
            rb.linearDamping = 1f;
            rb.angularDamping = 0.1f;

            // Ensure outline is removed
            if (baseMaterial != null)
            {
                rend.materials = new Material[] { baseMaterial };
            }

            // Play release sound
            controller.PlayDiskReleaseSound();

            // Check where disk landed after physics settles
            StartCoroutine(CheckLandingAfterDelay());
        }

        #endregion

        #region Physics-Based Dragging

        /// <summary>
        /// Applies elastic spring force to make the disk follow the mouse cursor smoothly.
        /// Movement is restricted to X and Y axes only (no depth change).
        /// </summary>
        private void ApplyElasticFollow()
        {
            if (mainCamera == null || rb == null) return;

            // Create a plane at the disk's current Z depth
            Plane movePlane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z + dragPlaneZOffset));

            // Raycast from mouse to plane
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (movePlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector3 targetPosition = new Vector3(worldPoint.x, worldPoint.y, transform.position.z);

                // Calculate spring force: F = k(target - current) - damping * velocity
                Vector3 direction = targetPosition - transform.position;
                Vector3 springForce = (direction * springStrength) - (rb.linearVelocity * springDamping);
                
                rb.AddForce(springForce, ForceMode.Acceleration);
            }
        }

        #endregion

        #region Position Reset and Landing Validation

        /// <summary>
        /// Resets the disk to its original position on its starting tower.
        /// Called when disk is placed invalidly (wrong tower or doesn't follow rules).
        /// Position is static based on current TowerIndex to avoid bugs with dynamic movement.
        /// </summary>
        public void ResetPosition()
        {
            // Get tower transforms from controller
            var towers = controller.GetTowerTransforms();
            if (towers == null || model.TowerIndex < 0 || model.TowerIndex >= towers.Count)
            {
                Debug.LogError("[DiskView] Invalid tower index during reset.");
                return;
            }

            // Reset to position above current tower
            Vector3 towerPosition = towers[model.TowerIndex].position;
            transform.position = new Vector3(towerPosition.x, towerPosition.y + 1f, towerPosition.z);

            // Stop all physics movement
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"[DiskView] Disk {model.Size} reset to Tower {model.TowerIndex}.");
            
            // Play error sound
            controller.PlayWrongClickSound();
        }

        /// <summary>
        /// Waits for physics to settle, then validates where the disk landed.
        /// Updates logical model if placement is valid, otherwise resets position.
        /// </summary>
        private IEnumerator CheckLandingAfterDelay()
        {
            // Wait for physics to settle
            yield return new WaitForSeconds(0.3f);

            var towers = controller.GetTowerTransforms();
            if (towers == null || towers.Count == 0)
            {
                Debug.LogWarning("[DiskView] No tower references found.");
                ResetPosition();
                yield break;
            }

            // Find closest tower by X position
            Transform closestTower = towers
                .OrderBy(t => Mathf.Abs(t.position.x - transform.position.x))
                .First();

            int targetIndex = towers.IndexOf(closestTower);
            float distanceToTower = Mathf.Abs(closestTower.position.x - transform.position.x);

            Debug.Log($"[DiskView] Closest tower: {closestTower.name} (index {targetIndex}), distance: {distanceToTower:F2}");

            // Check if disk is close enough to a tower
            if (distanceToTower > 1.5f)
            {
                Debug.Log("[DiskView] Disk landed too far from any tower - resetting.");
                ResetPosition();
                yield break;
            }

            // Validate placement according to Hanoi rules
            TowerModel targetTower = controller.GetGameModel().Towers[targetIndex];
            DiskModel targetTopDisk = targetTower.Peek();
            
            if (targetTopDisk != null && model.Size > targetTopDisk.Size)
            {
                Debug.LogWarning($"[DiskView] Cannot place Disk {model.Size} on Disk {targetTopDisk.Size} - violates Hanoi rules.");
                ResetPosition();
                yield break;
            }

            // Valid placement - update logical model
            controller.MoveDiskToTower(this, targetIndex);
            controller.GetGameModel().IncrementMoveCount();
            controller.UpdateMoveCountUI();

            // Calculate final position on tower stack
            float stackHeight = towers[targetIndex].position.y + 
                               0.3f * controller.GetGameModel().Towers[targetIndex].Count;

            Vector3 finalPosition = new Vector3(
                closestTower.position.x,
                stackHeight,
                closestTower.position.z
            );

            transform.position = finalPosition;

            // Update stored initial position and tower index
            initialPosition = transform.position;
            model.TowerIndex = targetIndex;

            Debug.Log($"[DiskView] Disk {model.Size} successfully placed on Tower {targetIndex}.");
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Returns the logical model associated with this view.
        /// </summary>
        public DiskModel GetModel() => model;

        #endregion
    }
}
