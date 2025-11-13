using Hanoi.Controller;
using System.Collections;
using UnityEngine;

/// <summary>
/// Interactive button that resets the game to initial state.
/// Features press animation and audio feedback.
/// </summary>
public class ResetButton : MonoBehaviour
{
    #region Serialized Fields

    [Header("Animation Settings")]
    [Tooltip("Duration of the button press animation")]
    [SerializeField] private float animationDuration = 0.2f;
    
    [Tooltip("How far the button moves down when pressed (negative = downward)")]
    [SerializeField] private float animationOffset = -0.05f;

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
        initialPosition = transform.position;
    }

    #endregion

    #region Mouse Interaction

    /// <summary>
    /// Called when mouse button is pressed on the button.
    /// Triggers press animation and game reset.
    /// </summary>
    private void OnMouseDown()
    {
        // Play button sound
        if (audioSource != null && buttonSFX != null)
        {
            audioSource.PlayOneShot(buttonSFX);
        }

        // Animate button press and reset game
        if (!isAnimating)
        {
            StartCoroutine(AnimateButton());
            ResetGame();
        }
    }

    #endregion

    #region Animation

    /// <summary>
    /// Animates the button press: down then back up.
    /// </summary>
    private IEnumerator AnimateButton()
    {
        isAnimating = true;

        // Press down
        Vector3 targetPosition = initialPosition + new Vector3(0, animationOffset, 0);
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;

        // Release back up
        elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.position = Vector3.Lerp(targetPosition, initialPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = initialPosition;

        isAnimating = false;
    }

    #endregion

    #region Game Reset

    /// <summary>
    /// Finds the GameController and calls its reset method.
    /// </summary>
    private void ResetGame()
    {
        GameController gameController = FindFirstObjectByType<GameController>();
        
        if (gameController != null)
        {
            gameController.ResetGameWithDiskCount();
            Debug.Log("[ResetButton] Game reset triggered.");
        }
        else
        {
            Debug.LogError("[ResetButton] GameController not found in scene.");
        }
    }

    #endregion
}
