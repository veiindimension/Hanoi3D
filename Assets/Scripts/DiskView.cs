// 07/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using Hanoi.Model;
using Hanoi.Controller;
using System.Collections;
using System.Linq;

namespace Hanoi.View
{
    /// <summary>
    /// DiskView handles the physics, drag interaction, and visual feedback
    /// for each disk. 
    /// Includes elastic spring movement and outline feedback (green/red/yellow).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class DiskView : MonoBehaviour
    {
        private DiskModel model;
        private GameController controller;

        private Rigidbody rb;
        private Renderer rend;
        private Material baseMaterial;
        [SerializeField] private Material outlineMaterial; // assign M_Outline in prefab

        // Combined materials (base + outline)
        private Material[] combinedMaterials;
        private Color outlineColor = Color.green;

        // Drag physics state
        private bool isHeld = false;
        private Camera mainCamera;

        // Elastic spring parameters
        [Header("Elastic Drag Settings")]
        [SerializeField] private float springStrength = 60f;  // pull strength toward mouse
        [SerializeField] private float springDamping = 10f;   // damping to reduce oscillation
        [SerializeField] private float dragPlaneZOffset = 0f; // optional plane offset

        // Disk geometry parameters
        [Header("Disk Shape Settings")]
        [SerializeField] private float baseThickness = 1.0f;
        [SerializeField] private float minRadius = 1.0f;
        [SerializeField] private float maxRadius = 1.8f;

        // Respawn position
        private Vector3 initialPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rend = GetComponentInChildren<Renderer>();

            if (rend == null)
                Debug.LogError("[DiskView] Renderer not found in children.");

            baseMaterial = (rend != null) ? rend.material : null;
            mainCamera = Camera.main;

            if (mainCamera == null)
                Debug.LogError("[DiskView] Camera.main is null. Tag your camera 'MainCamera'.");

            // Rigidbody configuration
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.linearDamping = 1f;
                rb.angularDamping = 0.1f;
            }
        }

        /// <summary>
        /// Initializes disk logic, appearance, and scaling based on its model.
        /// </summary>
        public void Initialize(DiskModel diskModel, GameController gameController)
        {
            model = diskModel;
            controller = gameController;

            int total = 1;
            if (controller != null && controller.GetGameModel() != null)
                total = controller.GetGameModel().DiskCount;

            // Scale radius proportionally to logical size
            float t = (total > 1) ? (model.Size - 1) / (float)(total - 1) : 0f;
            float radius = Mathf.Lerp(minRadius, maxRadius, t);
            transform.localScale = new Vector3(radius, baseThickness, radius);

            // Set up materials
            if (rend != null)
            {
                baseMaterial = rend.material;
                if (outlineMaterial != null)
                {
                    combinedMaterials = new Material[] { baseMaterial, outlineMaterial };
                }
            }

            // Randomize base color (visual variety)
            if (baseMaterial != null)
            {
                Color randomColor = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
                baseMaterial.color = randomColor;
            }

            initialPosition = transform.position;
        }

        // ==========================================================
        //  HOVER VISUAL FEEDBACK
        // ==========================================================

        public void OnHoverEnter(bool canPick)
        {
            if (rend == null || outlineMaterial == null) return;

            // Choose outline color based on availability to pick
            outlineColor = canPick ? Color.green : Color.red;
            outlineMaterial.SetColor("_outline_color", outlineColor);

            // Combine materials: base + outline
            rend.materials = combinedMaterials;
        }

        public void OnHoverExit()
        {
            if (rend == null || baseMaterial == null) return;

            // Restore base material only
            rend.materials = new Material[] { baseMaterial };
        }

        // ==========================================================
        //  PICK UP / RELEASE INTERACTION
        // ==========================================================

        public void OnPick()
        {
            if (rb == null) return;

            if (controller != null && !controller.CanSelectDisk(this))
            {
                Debug.Log("[DiskView] OnPick called but disk not selectable.");
                return;
            }

            isHeld = true;

            // Adjust drag for smoother control
            rb.linearDamping = 8f;
            rb.angularDamping = 8f;

            // Remove outline while held
            if (outlineMaterial != null)
            {
                rend.materials = new Material[] { baseMaterial };
            }
        }

        public void OnRelease()
        {
            if (rb == null) return;

            isHeld = false;

            // Reset damping
            rb.linearDamping = 1f;
            rb.angularDamping = 0.1f;

            // Remove outline → restore base
            if (baseMaterial != null)
                rend.materials = new Material[] { baseMaterial };

            // Evaluate landing position
            StartCoroutine(CheckLandingAfterDelay());





        }

        // ==========================================================
        //  PHYSICS-BASED MOUSE FOLLOW (ELASTIC)
        // ==========================================================

        private void FixedUpdate()
        {
            if (isHeld)
                ApplyElasticFollow();
        }

        /// <summary>
        /// Applies a spring-like force that pulls the disk toward the mouse cursor.
        /// Keeps movement restricted to X and Y axes (no depth movement).
        /// </summary>
        private void ApplyElasticFollow()
        {
            if (mainCamera == null || rb == null) return;

            // Plane at the current Z depth of the disk
            Plane movePlane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z + dragPlaneZOffset));

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (movePlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector3 targetPosition = new Vector3(worldPoint.x, worldPoint.y, transform.position.z);

                // Calculate spring force and apply it
                Vector3 direction = targetPosition - transform.position;
                Vector3 force = (direction * springStrength) - (rb.linearVelocity * springDamping);
                rb.AddForce(force, ForceMode.Acceleration);
            }
        }

        // ==========================================================
        //  POSITION RESET AND VALIDATION
        // ==========================================================

        /// <summary>
        /// Moves the disk back to its initial position if dropped outside valid towers.
        /// </summary>
        

        /// <summary>
        /// Moves the disk back to its original position on Tower 0.
        /// This method is static because with dynamic switch from tower to tower it would create bugs onto the counter of moves.
        /// </summary>
        public void ResetPosition()
        {

            // Check if the initial position was Tower 0
            if (model.TowerIndex == 0)
            {
                // Reset the disk to its original position on Tower 0
                transform.position = controller.GetTowerTransforms()[0].position;

                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log("[DiskView] Disk reset to original position on Tower 0.");
            }

            // Check if the initial position was Tower 1
            if (model.TowerIndex == 1)
            {
                // Reset the disk to its original position on Tower 1
                transform.position = controller.GetTowerTransforms()[1].position;

                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log("[DiskView] Disk reset to original position on Tower 1.");
            }

            // Check if the initial position was Tower 2
            if (model.TowerIndex == 2)
            {
                // Reset the disk to its original position on Tower 2
                transform.position = controller.GetTowerTransforms()[2].position;

                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log("[DiskView] Disk reset to original position on Tower 2.");
            }
        }


        /// <summary>
        /// Waits briefly for physics to settle, then checks which tower the disk landed on.
        /// Updates the logical model accordingly or resets position if invalid.
        /// </summary>
        private IEnumerator CheckLandingAfterDelay()
        {
            yield return new WaitForSeconds(0.3f);

            var towers = controller.GetTowerTransforms();
            if (towers == null || towers.Count == 0)
            {
                Debug.LogWarning("[DiskView] No tower references found.");
                ResetPosition();
                yield break;
            }

            Transform closestTower = towers
                .OrderBy(t => Mathf.Abs(t.position.x - transform.position.x))
                .First();

            int targetIndex = towers.IndexOf(closestTower);

            Debug.Log($"[DiskView] Closest tower = {closestTower.name} (index {targetIndex})");

            float dist = Mathf.Abs(closestTower.position.x - transform.position.x);
            if (dist > 1.5f)
            {
                Debug.Log("[DiskView] Disk landed too far from any tower → resetting.");
                ResetPosition();
                yield break;
            }

            TowerModel targetTower = controller.GetGameModel().Towers[targetIndex];
            DiskModel targetTopDisk = targetTower.Peek();
            if (targetTopDisk != null && model.Size > targetTopDisk.Size)
            {
                Debug.LogWarning($"[DiskView] Cannot place Disk {model.Size} on top of Disk {targetTopDisk.Size} in Tower {targetIndex}. Returning to initial position.");
                // Check if the initial position was Tower 0
                ResetPosition();
                yield break;
            }

            // Update the logical model before updating the position
            controller.MoveDiskToTower(this, targetIndex);
            controller.GetGameModel().IncrementMoveCount();
            controller.UpdateMoveCountUI();

            float stackHeight = towers[targetIndex].position.y
                                + 0.3f * (controller.GetGameModel().Towers[targetIndex].Count);

            Vector3 finalPos = new Vector3(
                closestTower.position.x,
                stackHeight,
                closestTower.position.z
            );

            transform.position = finalPos;

            // Update initial position only after successful placement
            initialPosition = transform.position;

            // Update the disk's TowerIndex to reflect its new position
            model.TowerIndex = targetIndex;
        }

        // ==========================================================
        //  ACCESSORS
        // ==========================================================
        public DiskModel GetModel() => model;
    }
}