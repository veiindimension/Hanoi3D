using Hanoi.Controller;
using UnityEngine;

public class SelectorsController : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private GameController gameController; // Riferimento al GameController

    private void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto che ha attivato il trigger è il cilindro
        if (other.CompareTag("Player"))
        {
            // Ottieni il nome del selector
            string selectorName = other.gameObject.name; // Nome del selector
            int diskCount;

            // Prova a convertire il nome del selector in un numero
            if (int.TryParse(selectorName, out diskCount))
            {
                // Aggiorna il diskCount nel GameController
                gameController.diskCount = diskCount;

                // Chiama il metodo per reimpostare il gioco con il nuovo numero di dischi
                gameController.ResetGameWithDiskCount();

                Debug.Log($"[SelectorsController] Cilindro agganciato a {selectorName}. DiskCount aggiornato a {gameController.diskCount}.");
            }
            else
            {
                Debug.LogError($"[SelectorsController] Il nome del selector '{selectorName}' non è un numero valido.");
            }
        }
    }
}