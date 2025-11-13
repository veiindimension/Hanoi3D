using Hanoi.Controller;
using UnityEngine;

/// <summary>
/// Controls the draggable cylinder used to select the number of disks for the game.
/// The cylinder can be dragged along the X-axis and snaps to the nearest selector marker.
/// Each selector corresponds to a different disk count (3-10).
/// </summary>
public class CylinderController : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("Reference to the main GameController")]
    [SerializeField] private GameController gameController;
    
    [Tooltip("Parent GameObject containing all selector markers")]
    [SerializeField] private Transform selectorsParent;
    
    [Tooltip("AudioSource component for playing sounds")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [Tooltip("Sound played when cylinder is grabbed")]
    [SerializeField] private AudioClip lever1SFX;
    
    [Tooltip("Sound played when cylinder is released")]
    [SerializeField] private AudioClip lever2SFX;

    #endregion

    #region Private Fields

    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Transform[] selectors;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Cache main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[CylinderController] Main Camera not found. Ensure camera is tagged 'MainCamera'.");
        }

        // Cache all selector children
        int childCount = selectorsParent.childCount;
        selectors = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            selectors[i] = selectorsParent.GetChild(i);
        }

        Debug.Log($"[CylinderController] Initialized with {selectors.Length} selectors.");
    }

    private void Update()
    {
        if (isDragging)
        {
            DragCylinder();
        }
    }

    #endregion

    #region Mouse Interaction

    /// <summary>
    /// Called when mouse button is pressed on the cylinder.
    /// Starts dragging and plays grab sound.
    /// </summary>
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Play grab sound
            if (audioSource != null && lever1SFX != null)
            {
                audioSource.clip = lever1SFX;
                audioSource.Play();
            }

            // Start dragging
            isDragging = true;
            dragOffset = transform.position - GetMouseWorldPosition();
            
            Debug.Log("[CylinderController] Started dragging cylinder.");
        }
    }

    /// <summary>
    /// Called when mouse button is released.
    /// Stops dragging, snaps to nearest selector, and updates disk count.
    /// </summary>
    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // Play release sound
            if (audioSource != null && lever2SFX != null)
            {
                audioSource.clip = lever2SFX;
                audioSource.Play();
            }

            // Stop dragging and snap
            isDragging = false;
            SnapToClosestSelector();
            
            Debug.Log($"[CylinderController] Released cylinder at position: {transform.position.x:F2}");
        }
    }

    #endregion

    #region Dragging Logic

    /// <summary>
    /// Updates cylinder position while dragging.
    /// Movement is constrained to X-axis only.
    /// </summary>
    private void DragCylinder()
    {
        Vector3 mousePosition = GetMouseWorldPosition() + dragOffset;

        // Constrain movement to X-axis only
        transform.position = new Vector3(mousePosition.x, transform.position.y, transform.position.z);
    }

    /// <summary>
    /// Converts mouse screen position to world position on a horizontal plane.
    /// </summary>
    /// <returns>World position at the cylinder's Y level</returns>
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

    #endregion

    #region Snapping Logic

    /// <summary>
    /// Snaps the cylinder to the closest selector and updates the game's disk count.
    /// The selector's name is parsed as the disk count (e.g., selector named "5" = 5 disks).
    /// </summary>
    private void SnapToClosestSelector()
    {
        if (selectors.Length == 0)
        {
            Debug.LogWarning("[CylinderController] No selectors available.");
            return;
        }

        // Find closest selector by X position
        float closestDistance = float.MaxValue;
        Transform closestSelector = null;

        foreach (Transform selector in selectors)
        {
            float distance = Mathf.Abs(transform.position.x - selector.position.x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSelector = selector;
            }
        }

        // Snap to closest selector
        if (closestSelector != null)
        {
            transform.position = new Vector3(
                closestSelector.position.x,
                transform.position.y,
                transform.position.z
            );

            // Parse disk count from selector name
            if (int.TryParse(closestSelector.name, out int diskCount))
            {
                gameController.diskCount = diskCount;
                Debug.Log($"[CylinderController] Snapped to selector '{closestSelector.name}'. Disk count set to {diskCount}.");
            }
            else
            {
                Debug.LogError($"[CylinderController] Selector name '{closestSelector.name}' is not a valid number.");
            }
        }
    }

    #endregion
}
