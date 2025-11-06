using UnityEngine;
using Hanoi.Model;
using Hanoi.Controller;
using System.Collections;

namespace Hanoi.View
{
    /// <summary>
    /// Handles selection, dragging and dropping of a disk.
    /// Includes outline color feedback and respawn logic if dropped outside towers.
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
        [SerializeField] private Material outlineMaterial;

        // --- Dragging ---
        private bool isHeld = false;
        private Camera mainCamera;
        private float liftHeight = 2.0f;
        private float followSpeed = 12f;

        // --- Shape ---
        [SerializeField] private float baseThickness = 1.0f;
        [SerializeField] private float minRadius = 1.0f;
        [SerializeField] private float maxRadius = 1.8f;

        // --- Respawn ---
        private Vector3 initialPosition;
        private bool landedOnTower = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rend = GetComponentInChildren<Renderer>();
            baseMaterial = rend.material;
            mainCamera = Camera.main;

            // Rigidbody setup
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public void Initialize(DiskModel diskModel, GameController gameController)
        {
            model = diskModel;
            controller = gameController;

            int total = controller.GetGameModel().DiskCount;
            float t = (total > 1) ? (model.Size - 1) / (float)(total - 1) : 0f;

            float radius = Mathf.Lerp(minRadius, maxRadius, t);
            transform.localScale = new Vector3(radius, baseThickness, radius);

            // Random color
            Color randomColor = new Color(
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f)
            );
            baseMaterial.color = randomColor;

            initialPosition = transform.position;
        }

        // --------------------------------------------------------------------
        // OUTLINE LOGIC
        // --------------------------------------------------------------------
        public void ShowOutline(Color color)
        {
            if (rend == null || outlineMaterial == null) return;
            outlineMaterial.color = color;
            rend.material = outlineMaterial;
        }

        public void HideOutline()
        {
            if (rend == null || baseMaterial == null) return;
            rend.material = baseMaterial;
        }

        private void OnMouseEnter()
        {
            if (controller == null) return;

            bool canPick = controller.CanSelectDisk(this);
            if (canPick)
                ShowOutline(Color.green); // in cima → verde
            else
                ShowOutline(Color.red);   // non in cima → rosso
        }

        private void OnMouseExit()
        {
            HideOutline();
        }

        // --------------------------------------------------------------------
        // DRAG + DROP LOGIC
        // --------------------------------------------------------------------
        private void OnMouseDown()
        {
            if (controller == null) return;
            if (!controller.CanSelectDisk(this)) return;

            controller.OnDiskSelected(this);

            isHeld = true;
            rb.useGravity = false;
            rb.isKinematic = true;
            landedOnTower = false;

            ShowOutline(Color.yellow); // selezionato
        }

        private void OnMouseUp()
        {
            if (!isHeld) return;

            isHeld = false;
            rb.isKinematic = false;
            rb.useGravity = true;

            HideOutline();

            // Avvia la verifica del posizionamento dopo la caduta
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

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 targetPos = hit.point;
                targetPos.y += liftHeight;
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            }
        }

        // --------------------------------------------------------------------
        // LANDING + RESPAWN LOGIC
        // --------------------------------------------------------------------
        private IEnumerator CheckLandingAfterDelay()
        {
            // aspetta mezzo secondo per permettere alla fisica di fermarsi
            yield return new WaitForSeconds(0.5f);

            // controlla se è sopra una torre
            landedOnTower = false;
            foreach (Transform tower in controller.GetTowerTransforms())
            {
                float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                  new Vector3(tower.position.x, 0, tower.position.z));
                if (distance < 1.5f) // distanza accettabile per considerare la torre "centrata"
                {
                    landedOnTower = true;
                    break;
                }
            }

            if (!landedOnTower)
            {
                // fuori da tutte le torri → reset posizione
                RespawnToInitialPosition();
            }
        }

        private void RespawnToInitialPosition()
        {
            rb.isKinematic = true;
            rb.useGravity = false;

            StartCoroutine(SmoothMove(initialPosition, 0.4f));
        }

        private IEnumerator SmoothMove(Vector3 targetPos, float duration)
        {
            Vector3 start = transform.position;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, targetPos, t);
                yield return null;
            }

            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // --------------------------------------------------------------------
        // ACCESSORS
        // --------------------------------------------------------------------
        public DiskModel GetModel() => model;
    }
}
