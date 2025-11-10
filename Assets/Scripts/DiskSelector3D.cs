using UnityEngine;
using Hanoi.Controller;

public class DiskSelector3D : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform cylinder; // Riferimento al cilindro
    [SerializeField] private Transform selectorsParent; // Riferimento al GameObject "Selectors"
    [SerializeField] private GameController gameController; // Riferimento al GameController

    [Header("Impostazioni")]
    [SerializeField] private int minDiskCount = 3; // Numero minimo di dischi
    [SerializeField] private int maxDiskCount = 10; // Numero massimo di dischi

    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Transform[] selectors;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[DiskSelector3D] Main Camera non trovata. Assicurati che la tua camera sia taggata come 'MainCamera'.");
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
            isDragging = true;
            dragOffset = cylinder.position - GetMouseWorldPosition();
            Debug.Log("[DiskSelector3D] Inizio trascinamento.");
        }
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            UpdateDiskCount(cylinder.position.x);
            Debug.Log("[DiskSelector3D] Fine trascinamento. Posizione cilindro: " + cylinder.position.x);
        }
    }

    private void DragCylinder()
    {
        Vector3 mousePosition = GetMouseWorldPosition() + dragOffset;

        // Blocca il movimento solo sull'asse X
        cylinder.position = new Vector3(mousePosition.x, cylinder.position.y, cylinder.position.z);

        Debug.Log($"[DiskSelector3D] Cilindro trascinato alla posizione: {cylinder.position}");
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, cylinder.position);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Aggiorna il numero di dischi in base alla posizione del cilindro rispetto ai selector.
    /// </summary>
    public void UpdateDiskCount(float cylinderX)
    {
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        // Trova il selector più vicino alla posizione del cilindro
        for (int i = 0; i < selectors.Length; i++)
        {
            float distance = Mathf.Abs(cylinderX - selectors[i].position.x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        // Mappa l'indice del selector al range di dischi (3-10)
        int newDiskCount = minDiskCount + closestIndex;

        // Aggiorna il numero di dischi nel GameController
        gameController.diskCount = newDiskCount;

        Debug.Log($"[DiskSelector3D] Numero di dischi aggiornato a {newDiskCount} basato sul selector {closestIndex + 1}.");
    }
}