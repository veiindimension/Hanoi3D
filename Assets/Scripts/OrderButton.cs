using Hanoi.Controller;
using UnityEngine;

public class OrderButton : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private GameController gameController; // Riferimento al GameController
    [SerializeField] private Material ledMaterial; // Materiale M_Led
    [SerializeField] private Color colorTrue = Color.green; // Colore per Spawn Disks Ordered = true
    [SerializeField] private Color colorFalse = Color.red; // Colore per Spawn Disks Ordered = false

    [Header("Animazione")]
    [SerializeField] private float animationDuration = 0.1f; // Durata dell'animazione
    [SerializeField] private float animationOffset = -0.1f; // Offset dell'animazione

    private Vector3 initialPosition;
    private bool isAnimating = false;

    private void Start()
    {
        initialPosition = transform.localPosition;

        // Imposta il colore iniziale del materiale in base al valore di spawnDisksOrdered
        UpdateLedColor();
    }

    private void OnMouseDown()
    {
        Debug.Log("[OrderButton] Button clicked!"); // Debug log to check if the button is clicked

        if (!isAnimating)
        {
            isAnimating = true;

            // Esegui animazione al click
            Vector3 targetPosition = initialPosition + new Vector3(0, animationOffset, 0);
            StartCoroutine(AnimateButton(targetPosition, () =>
            {
                // Inverti il valore di spawnDisksOrdered
                gameController.spawnDisksOrdered = !gameController.spawnDisksOrdered;

                Debug.Log($"[OrderButton] spawnDisksOrdered is now: {gameController.spawnDisksOrdered}"); // Debug log to check the new value of spawnDisksOrdered

                // Aggiorna il colore del materiale
                UpdateLedColor();

                // Termina animazione
                StartCoroutine(AnimateButton(initialPosition, () => isAnimating = false));
            }));
        }
    }

    private void UpdateLedColor()
    {
        if (gameController.spawnDisksOrdered)
        {
            ledMaterial.color = colorTrue; // Verde
        }
        else
        {
            ledMaterial.color = colorFalse; // Rosso
        }

        Debug.Log($"[OrderButton] LED color updated to: {(gameController.spawnDisksOrdered ? "Green" : "Red")}"); // Debug log to check the LED color update
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