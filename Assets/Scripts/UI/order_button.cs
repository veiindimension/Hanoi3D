using Hanoi.Controller;
using UnityEngine;

/// <summary>
/// Interactive button that toggles whether disks spawn in ordered or random arrangement.
/// Visual feedback provided via LED material color (green = ordered, red = random).
/// Features press animation and audio feedback.
/// </summary>
public class OrderButton : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [Tooltip("Reference to the main GameController")]
    [SerializeField] private GameController gameController;
    
    [Tooltip("LED material that changes color based on order state")]
    [SerializeField] private Material ledMaterial;

    [Header("LED Colors")]
    [Tooltip("LED color when disks spawn in order (green)")]
    [SerializeField] private Color colorTrue = Color.green;
    
    [Tooltip("LED color when disks spawn randomly (red)")]
    [SerializeField] private Color colorFalse = Color.red;

    [Header("Animation Settings")]
    [Tooltip("Duration of button press animation")]
    [SerializeField] private float animationDuration = 0.1f;
    
    [Tooltip("How far the button moves down when pressed (negative = downward)")]
    [SerializeField] private float animationOffset = -0.1f;

    [Header("Audio")]
    [Tooltip("Sound effect played when button is pressed")]
    [SerializeField] private AudioClip buttonSFX;
    
    [Tooltip("AudioSource component for playing sounds")]
    [SerializeField] private AudioSource audioSource;

    #endregion

    #region Private Fields

    private Vector3 initialPosition;
    private bool isAnimating = false;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Store initial position for animation
        initialPosition = transform.localPosition;

        // Set initial LED color based on current setting
        UpdateLedColor();
    }

    #endregion

    #region Mouse Interaction

    /// <summary>
    /// Called when mouse button is pressed on the button.
    /// Toggles spawn order setting and updates LED color.
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log("[OrderButton] Button clicked.");

        // Play button sound
        if (audioSource != null && buttonSFX != null)
        {
            audioSource.PlayOneShot(buttonSFX);
        }

        // Animate press and toggle setting
        if (!isAnimating)
        {
            isAnimating = true;

            // Press down
            Vector3 targetPosition = initialPosition + new Vector3(0, animationOffset, 0);
            StartCoroutine(AnimateButton(targetPosition, () =>
            {
                // Toggle spawn order setting
                gameController.spawnDisksOrdered = !gameController.spawnDisksOrdered;
                Debug.Log($"[OrderButton] Spawn disks ordered: {gameController.spawnDisksOrdered}");

                // Update LED color
                UpdateLedColor();

                // Release back up
                StartCoroutine(AnimateButton(initialPosition, () => isAnimating = false));
            }));
        }
    }

    #endregion

    #region LED Update

    /// <summary>
    /// Updates the LED material color based on the spawn order setting.
    /// Green = ordered, Red = random.
    /// </summary>
    private void UpdateLedColor()
    {
        if (ledMaterial == null)
        {
            Debug.LogWarning("[OrderButton] LED material not assigned.");
            return;
        }

        ledMaterial.color = gameController.spawnDisksOrdered ? colorTrue : colorFalse;
        Debug.Log($"[OrderButton] LED color set to: {(gameController.spawnDisksOrdered ? "Green" : "Red")}");
    }

    #endregion

    #region Animation

    /// <summary>
    /// Animates the button to a target position with a completion callback.
    /// </summary>
    /// <param name="targetPosition">Target local position</param>
    /// <param name="onComplete">Action to invoke when animation completes</param>
    private System.Collections.IEnumerator AnimateButton(Vector3 targetPosition, System.Action onComplete)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.localPosition;

        // Lerp to target position
        while (elapsedTime < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure exact final position
        transform.localPosition = targetPosition;

        // Invoke completion callback
        onComplete?.Invoke();
    }

    #endregion
}
