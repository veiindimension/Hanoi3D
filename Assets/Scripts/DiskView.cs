using UnityEngine;
using Hanoi.Model;
using Hanoi.Controller;
using System.Collections;
using System.Linq;

namespace Hanoi.View
{
    /// <summary>
    /// DiskView: visual + physics behaviour.
    /// Methods OnHoverEnter/Exit/OnPick/OnRelease are called by GameController (raycast input).
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

        // drag physics
        private bool isHeld = false;
        private Camera mainCamera;
        [SerializeField] private float liftHeight = 2.0f;
        [SerializeField] private float followSpeed = 12f;

        // visual params (set in inspector or keep defaults)
        [SerializeField] private float baseThickness = 1.0f;
        [SerializeField] private float minRadius = 1.0f;
        [SerializeField] private float maxRadius = 1.8f;

        // respawn
        private Vector3 initialPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rend = GetComponentInChildren<Renderer>();
            if (rend == null) Debug.LogError("[DiskView] Renderer not found in children.");
            baseMaterial = (rend != null) ? rend.material : null;
            mainCamera = Camera.main;
            if (mainCamera == null) Debug.LogError("[DiskView] Camera.main is null. Tag your camera 'MainCamera'.");

            // ensure RB settings are reasonable
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }

        public void Initialize(DiskModel diskModel, GameController gameController)
        {
            model = diskModel;
            controller = gameController;

            int total = 1;
            if (controller != null && controller.GetGameModel() != null)
                total = controller.GetGameModel().DiskCount;

            float t = (total > 1) ? (model.Size - 1) / (float)(total - 1) : 0f;
            float radius = Mathf.Lerp(minRadius, maxRadius, t);
            transform.localScale = new Vector3(radius, baseThickness, radius);

            // random color for base material
            if (baseMaterial != null)
            {
                Color randomColor = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
                baseMaterial.color = randomColor;
            }

            // store initial pos (spawned position)
            initialPosition = transform.position;
        }

        // ---------- Called by controller when raycast hits this disk ----------
        public void OnHoverEnter(bool canPick)
        {
            if (rend == null) return;
            if (outlineMaterial == null)
            {
                // fallback: tint base material slightly
                if (baseMaterial != null)
                    baseMaterial.color = canPick ? Color.green : Color.red;
                return;
            }

            // set outline material color and apply
            outlineMaterial.color = canPick ? Color.green : Color.red;
            rend.material = outlineMaterial;
        }

        public void OnHoverExit()
        {
            if (rend == null) return;
            if (baseMaterial != null)
                rend.material = baseMaterial;
        }

        // ---------- Pick up / release called by controller ----------
        public void OnPick()
        {
            if (rb == null) return;

            // only allow pick if controller says so (extra safety)
            if (controller != null && !controller.CanSelectDisk(this))
            {
                Debug.Log("[DiskView] OnPick called but disk not selectable.");
                return;
            }

            isHeld = true;
            rb.useGravity = false;
            rb.isKinematic = true;

            // visual feedback: yellow outline if possible
            if (outlineMaterial != null)
            {
                outlineMaterial.color = Color.yellow;
                rend.material = outlineMaterial;
            }
        }

        public void OnRelease()
        {
            if (rb == null) return;

            isHeld = false;
            rb.isKinematic = false;
            rb.useGravity = true;

            // restore material immediately (or wait)
            if (baseMaterial != null)
                rend.material = baseMaterial;

            // after a short delay, check landing
            StartCoroutine(CheckLandingAfterDelay());
        }

        private void Update()
        {
            if (isHeld)
                FollowMouse();
        }

        private void FollowMouse()
        {
            if (mainCamera == null) return;

            // Read current mouse position (in screen coordinates)
            Vector3 mousePos = Input.mousePosition;

            // Add a fixed depth value so we can convert it to a world position.
            // This depth should roughly match the Z distance of the disk from the camera.
            // The larger this value, the further away the "drag plane" is.
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);

            // Convert screen position → world position
            Vector3 target = mainCamera.ScreenToWorldPoint(mousePos);

            // ✅ Lock movement to X and Y (let them move)
            //    but keep the same Z value as the original disk (no depth movement)
            target.z = transform.position.z;

            // Smooth movement for a nice drag feeling
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * followSpeed);
        }

        /// <summary>
        /// Moves the disk back to its original starting position
        /// (used when the player drops it outside of any valid tower area).
        /// </summary>
        public void ResetToInitialPosition()
        {
            // Instantly move the disk back to where it started
            transform.position = initialPosition;

            // Stop any residual motion if using physics
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log("[DiskView] Disk reset to initial position.");
        }



        /// <summary>
        /// After a short delay (to let physics settle), this method checks 
        /// where the disk has landed and updates its logical tower accordingly.
        /// If the disk is too far from any tower, it resets to its original position.
        /// </summary>
        private System.Collections.IEnumerator CheckLandingAfterDelay()
        {
            // Wait a short time to ensure the disk has stopped moving physically
            yield return new WaitForSeconds(0.3f);

            // Get all tower transforms from the GameController
            var towers = controller.GetTowerTransforms();
            if (towers == null || towers.Count == 0)
            {
                Debug.LogWarning("[DiskView] No tower references found.");
                ResetToInitialPosition();
                yield break;
            }

            // Find the closest tower based on horizontal (X) distance
            Transform closestTower = towers
                .OrderBy(t => Mathf.Abs(t.position.x - transform.position.x))
                .First();

            // Get the index (0, 1, or 2) of that closest tower
            int targetIndex = towers.IndexOf(closestTower);

            // Debug log to verify which tower is detected
            Debug.Log($"[DiskView] Closest tower = {closestTower.name} (index {targetIndex})");

            // If the disk is too far away from any tower, reset its position
            float dist = Mathf.Abs(closestTower.position.x - transform.position.x);
            if (dist > 1.5f)
            {
                Debug.Log("[DiskView] Disk landed too far from any tower → resetting.");
                ResetToInitialPosition();
                yield break;
            }

            // Inform the controller that this disk has moved to a new tower
            controller.MoveDiskToTower(this, targetIndex);

            // Compute new Y position based on how many disks are already on that tower
            float stackHeight = towers[targetIndex].position.y
                                + 0.3f * (controller.GetGameModel().Towers[targetIndex].Count);

            // Build the final position (same X and Z as the tower)
            Vector3 finalPos = new Vector3(
                closestTower.position.x,
                stackHeight,
                closestTower.position.z
            );

            // Instantly move the disk to its final resting position
            transform.position = finalPos;

            // Save this position as the new "base" position
            initialPosition = transform.position;
        }

        private IEnumerator RespawnSmooth(Vector3 targetPos, float duration)
        {
            // temporarily disable physics while moving back
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Vector3 start = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, targetPos, t);
                yield return null;
            }

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        // accessor
        public DiskModel GetModel() => model;
    }
}
