using UnityEngine;
using Hanoi.Model;

namespace Hanoi.View
{
    /// <summary>
    /// Handles the visual and interactive behavior of a disk in the Unity scene.
    /// Each DiskView is linked to a DiskModel that stores its logical state.
    /// </summary>
    [RequireComponent(typeof(Renderer))] /// Mandatory for unity 
    public class DiskView : MonoBehaviour
    {
        // Reference to the logical data model for this disk
        private DiskModel model;

        // Reference to the main game controller
        private GameController controller;

        // Material used for color and hover outline
        private Material diskMaterial;

        // Whether this disk is currently being dragged
        private bool isDragging = false;

        // Height offset when the disk is being dragged
        [SerializeField] private float dragHeight = 1.5f;

        // Original Y position when not dragging
        private float baseHeight;

        // Initial x/y/z position of the disk
        private Vector3 initialPosition;


        // Colors for hover / default
        private Color defaultColor;
        [SerializeField] private Color hoverColor = Color.black;

        private void Awake()
        {
            // Get the Renderer from the torus mesh inside the disk prefab
            diskMaterial = GetComponentInChildren<Renderer>().material;

            // Save the default color of the material
            defaultColor = diskMaterial.color;

            // Save initial height
            baseHeight = transform.position.y;
        }

        /// <summary>
        /// Links this visual object with its logical model and the game controller.
        /// </summary>
        public void Initialize(DiskModel diskModel, GameController gameController)
        {
            model = diskModel;
            controller = gameController;

            // Scale disk based on its logical size (larger size → larger radius)
            float scaleFactor = 0.3f + 0.1f * model.Size;
            transform.localScale = new Vector3(scaleFactor, 0.3f, scaleFactor);

            // Assign a random color for visual variety
            Color randomColor = new Color(
                UnityEngine.Random.Range(0.2f, 1f),
                UnityEngine.Random.Range(0.2f, 1f),
                UnityEngine.Random.Range(0.2f, 1f)
            );
            diskMaterial.color = randomColor;
            defaultColor = randomColor;
            initialPosition = transform.position;
        }


        /// <summary>
        /// Returns the disk to its original starting position.
        /// Used when it’s dropped outside a valid tower.
        /// </summary>
        public void ResetToInitialPosition()
        {
            transform.position = initialPosition;
        }


        private void OnMouseEnter()
        {
            // When mouse hovers, highlight the disk
            diskMaterial.color = hoverColor;
        }

        private void OnMouseExit()
        {
            // Reset to the default color
            diskMaterial.color = defaultColor;
        }

        private void OnMouseDown()
        {
            // Notify the game controller that this disk has been clicked
            controller.OnDiskSelected(this);
        }

        /// <summary>
        /// Instantly set the position of the disk.
        /// </summary>
        public void SetPosition(Vector3 targetPosition)
        {
            transform.position = targetPosition;
        }

        /// <summary>
        /// Smoothly move the disk toward a new position (used for animation).
        /// </summary>
        public void MoveTo(Vector3 newPos)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothMove(newPos));
        }

        private System.Collections.IEnumerator SmoothMove(Vector3 target)
        {
            Vector3 start = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 3f; // speed
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }
        }

        // Returns the logical model associated with this visual disk
        public DiskModel GetModel()
        {
            return model;
        }
    }
}
