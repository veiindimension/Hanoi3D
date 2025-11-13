using UnityEngine;

/// <summary>
/// Interactive button that quits the application.
/// Features press animation and audio feedback.
/// Note: Application.Quit() only works in builds, not in the Unity Editor.
/// </summary>
public class QuitButton : MonoBehaviour
{
    #region Serialized Fields

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
        // Store initial position for animation reset
        initialPosition = transform.localPosition;
    }

    #endregion

    #region Mouse Interaction

    /// <summary>
    /// Called when mouse button is pressed on the button.
    /// Triggers press animation and quits the application.
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log("[QuitButton] Button clicked.");

        // Play button sound
        if (audioSource != null && buttonSFX != null)
        {
            audioSource.PlayOneShot(buttonSFX);
        }

        // Animate button press and quit
        if (!isAnimating)
        {
            isAnimating = true;

            // Press down
            Vector3 targetPosition = initialPosition + new Vector3(0, animationOffset, 0);
            StartCoroutine(AnimateButton(targetPosition, () =>
            {
                Debug.Log("[QuitButton] Quitting application...");
                
                // Quit the game
                QuitGame();

                // Release back up (won't complete if application quits)
                StartCoroutine(AnimateButton(initialPosition, () => isAnimating = false));
            }));
        }
    }

    #endregion

    #region Quit Logic

    /// <summary>
    /// Quits the application.
    /// Works in builds only - has no effect in Unity Editor.
    /// </summary>
    private void QuitGame()
    {
        Application.Quit();
        Debug.Log("[QuitButton] Application.Quit() called. (Works in build only, not in Editor)");
        
        // For testing in Editor, you can uncomment this:
        // #if UNITY_EDITOR
        // UnityEditor.EditorApplication.isPlaying = false;
        // #endif
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
