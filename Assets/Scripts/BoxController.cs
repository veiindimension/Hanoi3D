using UnityEngine;

public class BoxController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int minDiskCount = 3;
    [SerializeField] private int maxDiskCount = 10;

    private float boxMinX;
    private float boxMaxX;
    private float[] divisions;

    private void Start()
    {
        // Calcola i limiti del box
        Vector3 boxSize = transform.localScale;
        boxMinX = transform.position.x - boxSize.x / 2;
        boxMaxX = transform.position.x + boxSize.x / 2;

        // Suddividi il box in 8 parti proporzionali
        divisions = new float[maxDiskCount - minDiskCount + 1];
        float divisionWidth = (boxMaxX - boxMinX) / divisions.Length;

        for (int i = 0; i < divisions.Length; i++)
        {
            divisions[i] = boxMinX + divisionWidth * (i + 0.5f);
        }
    }

    /// <summary>
    /// Restituisce il numero corrispondente alla posizione del cilindro.
    /// </summary>
    public int GetDiskCountFromPosition(float cylinderX)
    {
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < divisions.Length; i++)
        {
            float distance = Mathf.Abs(cylinderX - divisions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return minDiskCount + closestIndex;
    }
}