using UnityEngine;

public class QuitButton : MonoBehaviour
{
    [Header("Animazione")]
    [SerializeField] private float animationDuration = 0.1f; // Durata dell'animazione
    [SerializeField] private float animationOffset = -0.1f; // Offset dell'animazione

    private Vector3 initialPosition;
    private bool isAnimating = false;

    private void Start()
    {
        initialPosition = transform.localPosition;
    }

    private void OnMouseDown()
    {
        Debug.Log("[QuitButton] Button clicked!"); // Debug log per verificare il click del pulsante

        if (!isAnimating)
        {
            isAnimating = true;

            // Esegui animazione al click
            Vector3 targetPosition = initialPosition + new Vector3(0, animationOffset, 0);
            StartCoroutine(AnimateButton(targetPosition, () =>
            {
                Debug.Log("[QuitButton] Exiting the game..."); // Debug log per confermare l'azione di uscita

                // Esci dal gioco
                QuitGame();

                // Termina animazione
                StartCoroutine(AnimateButton(initialPosition, () => isAnimating = false));
            }));
        }
    }

    private void QuitGame()
    {
        // Esci dal gioco
        Application.Quit();

        // Debug log per editor (non funziona in build)
        Debug.Log("[QuitButton] Application.Quit() chiamato.");
    }

    private System.Collections.IEnumerator AnimateButton(Vector3 targetPosition, System.Action onComplete)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.localPosition;

        while (elapsedTime < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;

        onComplete?.Invoke();
    }
}