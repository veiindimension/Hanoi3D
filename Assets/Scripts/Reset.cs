using Hanoi.Controller;
using System.Collections;
using UnityEngine;

public class ResetButton : MonoBehaviour
{
    private Vector3 initialPosition;
    private bool isAnimating = false;

    [SerializeField] private float animationDuration = 0.2f; // Durata dell'animazione
    [SerializeField] private float animationOffset = -0.05f; // Offset sull'asse Y

    [Header("Audio")]
    [SerializeField] private AudioClip buttonSFX; // Clip audio per il suono del pulsante
    [SerializeField] private AudioSource audioSource; // Componente AudioSource per riprodurre il suono

    private void Start()
    {
        // Salva la posizione iniziale
        initialPosition = transform.position;
    }

    private void OnMouseDown()
    {

        if (audioSource != null && buttonSFX != null)
        {
            audioSource.PlayOneShot(buttonSFX);
        }

        if (!isAnimating)
        {
            StartCoroutine(AnimateButton());
            ResetDisks();
        }
    }

    private IEnumerator AnimateButton()
    {
        isAnimating = true;

        // Abbassa il pulsante
        Vector3 targetPosition = initialPosition + new Vector3(0, animationOffset, 0);
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        // Torna alla posizione iniziale
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

    private void ResetDisks()
    {
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.ResetGameWithDiskCount();
        }
        else
        {
            Debug.LogError("[ResetButton] GameController non trovato nella scena.");
        }
    }
}