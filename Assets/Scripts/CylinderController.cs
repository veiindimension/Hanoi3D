using Hanoi.Controller;
using UnityEngine;

public class CylinderController : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private GameController gameController; // Riferimento al GameController
    [SerializeField] private Transform selectorsParent; // Riferimento al GameObject "Selectors"
    [SerializeField] private AudioSource audioSource; // Riferimento al componente AudioSource
    [SerializeField] private AudioClip lever1SFX; // Clip audio per il click del mouse
    [SerializeField] private AudioClip lever2SFX; // Clip audio per il rilascio del mouse


    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Transform[] selectors;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[CylinderController] Main Camera non trovata. Assicurati che la tua camera sia taggata come 'MainCamera'.");
        }

        // Ottieni tutti i figli del GameObject "Selectors"
        int childCount = selectorsParent.childCount;
        selectors = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            selectors[i] = selectorsParent.GetChild(i);
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            DragCylinder();
        }
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (audioSource != null && lever1SFX != null)
            {
                audioSource.clip = lever1SFX;
                audioSource.Play();
            }
            isDragging = true;
            dragOffset = transform.position - GetMouseWorldPosition();
            Debug.Log("[CylinderController] Inizio trascinamento.");
        }
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (audioSource != null && lever2SFX != null)
            {
                audioSource.clip = lever2SFX;
                audioSource.Play();
            }
            isDragging = false;
            SnapToClosestSelector();
            Debug.Log("[CylinderController] Fine trascinamento. Posizione cilindro: " + transform.position);
        }
    }

    private void DragCylinder()
    {
        Vector3 mousePosition = GetMouseWorldPosition() + dragOffset;

        // Blocca il movimento solo sull'asse X
        transform.position = new Vector3(mousePosition.x, transform.position.y, transform.position.z);

        Debug.Log($"[CylinderController] Cilindro trascinato alla posizione: {transform.position}");
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private void SnapToClosestSelector()
    {
        float closestDistance = float.MaxValue;
        Transform closestSelector = null;

        // Trova il selector più vicino
        foreach (Transform selector in selectors)
        {
            float distance = Mathf.Abs(transform.position.x - selector.position.x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSelector = selector;
            }
        }

        // Snap alla posizione del selector più vicino
        if (closestSelector != null)
        {
            transform.position = new Vector3(closestSelector.position.x, transform.position.y, transform.position.z);

            // Aggiorna il diskCount nel GameController
            int diskCount;
            if (int.TryParse(closestSelector.name, out diskCount))
            {
                gameController.diskCount = diskCount;

                // Debug per confermare l'aggiornamento
                Debug.Log($"[CylinderController] Cilindro agganciato al Selector '{closestSelector.name}'. DiskCount aggiornato a {gameController.diskCount}.");
            }
            else
            {
                Debug.LogError($"[CylinderController] Il nome del Selector '{closestSelector.name}' non è un numero valido.");
            }
        }
    }
}