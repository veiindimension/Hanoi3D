// 07/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

// 07/11/2025 AI-Tag
// Questo script è stato creato con l'aiuto di Assistant, un prodotto di Intelligenza Artificiale di Unity.

using System.Collections.Generic;
using UnityEngine;
using Hanoi.Model;
using Hanoi.View;
using System.Linq;

namespace Hanoi.Controller
{
    /// <summary>
    /// Gestore centrale che controlla la logica e gestisce l'input del mouse (basato su raycast).
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Riferimenti della scena (assegnare nell'Inspector)")]
        [Tooltip("Trascina qui il Transform di TowerA")]
        [SerializeField] private Transform towerA;
        [Tooltip("Trascina qui il Transform di TowerB")]
        [SerializeField] private Transform towerB;
        [Tooltip("Trascina qui il Transform di TowerC")]
        [SerializeField] private Transform towerC;

        [Tooltip("Prefab del disco (assegnare il prefab con il componente DiskView)")]
        [SerializeField] private GameObject diskPrefab;

        [Header("Impostazioni")]
        [SerializeField, Range(3, 10)] private int diskCount = 4;
        [SerializeField] private float diskVerticalGap = 0.02f;
        [Tooltip("Se vero, i dischi verranno generati in ordine sulla prima torre.")]
        [SerializeField] private bool spawnDisksOrdered = true;

        // Modello logico
        private GameModel gameModel;
        private List<Transform> towerTransforms = new List<Transform>();

        // Stato di input/selezione
        private DiskView hoveredDisk = null;
        private DiskView selectedDisk = null;

        // Cache della camera
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogError("[GameController] Camera.main è null. Assicurati che la tua camera abbia il tag 'MainCamera'.");

            if (towerA == null || towerB == null || towerC == null)
                Debug.LogWarning("[GameController] Uno o più riferimenti alle torri non sono assegnati nell'Inspector.");
            if (diskPrefab == null)
                Debug.LogWarning("[GameController] Prefab del disco non assegnato nell'Inspector.");
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            gameModel = new GameModel();
            gameModel.Initialize(diskCount);

            towerTransforms.Clear();
            towerTransforms.Add(towerA);
            towerTransforms.Add(towerB);
            towerTransforms.Add(towerC);

            SpawnDisks();
        }

        private void SpawnDisks()
        {
            TowerModel firstTower = gameModel.Towers[0];
            List<DiskModel> disks = firstTower.Disks.ToList();

            if (!spawnDisksOrdered)
            {
                System.Random random = new System.Random();
                disks = disks.OrderBy(d => random.Next()).ToList();
            }
            else
            {
                disks = disks.OrderByDescending(d => d.Size).ToList();
            }

            firstTower.Clear();

            float cumulativeHeight = 0f;
            foreach (DiskModel diskModel in disks)
            {
                GameObject newDisk = Instantiate(diskPrefab);
                newDisk.name = "Disk_" + diskModel.Size;

                DiskView view = newDisk.GetComponent<DiskView>();
                if (view == null)
                {
                    Debug.LogError("[GameController] Il prefab del disco manca del componente DiskView.");
                    continue;
                }

                view.Initialize(diskModel, this);

                float diskHeight = newDisk.GetComponentInChildren<Renderer>().bounds.size.y;
                Vector3 towerBase = towerTransforms[0].position;
                float yPos = towerBase.y + cumulativeHeight + diskHeight * 0.5f;
                Vector3 pos = new Vector3(towerBase.x, yPos, towerBase.z);

                newDisk.transform.position = pos;
                cumulativeHeight += diskHeight + diskVerticalGap;

                firstTower.Push(diskModel);
            }

            Debug.Log("[GameController] Generati " + disks.Count + " dischi sulla Torre 0.");
        }

        private void Update()
        {
            HandleMouseRaycast();
            HandleMouseClickRelease();

            if (gameModel.IsGameComplete())
            {
                Debug.Log($"[GameController] Vittoria! Tutti i dischi sono impilati in ordine sulla terza torre in {gameModel.MoveCount} mosse.");
                ShowVictoryScreen();
            }
        }

        private void HandleMouseRaycast()
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                DiskView hitDisk = hit.collider.GetComponentInParent<DiskView>();
                if (hitDisk != hoveredDisk)
                {
                    if (hoveredDisk != null)
                        hoveredDisk.OnHoverExit();

                    hoveredDisk = hitDisk;

                    if (hoveredDisk != null)
                    {
                        bool canPick = CanSelectDisk(hoveredDisk);
                        hoveredDisk.OnHoverEnter(canPick);
                    }
                }
            }
            else
            {
                if (hoveredDisk != null)
                {
                    hoveredDisk.OnHoverExit();
                    hoveredDisk = null;
                }
            }
        }

        private void HandleMouseClickRelease()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hoveredDisk != null)
                {
                    if (CanSelectDisk(hoveredDisk))
                    {
                        selectedDisk = hoveredDisk;
                        selectedDisk.OnPick();
                        Debug.Log("[GameController] Disco selezionato: " + selectedDisk.GetModel().Size);
                    }
                    else
                    {
                        Debug.Log("[GameController] Disco non selezionabile (non è in cima).");
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (selectedDisk != null)
                {
                    selectedDisk.OnRelease();
                    Debug.Log("[GameController] Disco rilasciato: " + selectedDisk.GetModel().Size);
                    selectedDisk = null;
                }
            }
        }

        public bool CanSelectDisk(DiskView disk)
        {
            if (disk == null || gameModel == null) return false;
            DiskModel model = disk.GetModel();
            int towerIndex = model.TowerIndex;

            if (towerIndex < 0 || towerIndex >= gameModel.Towers.Length) return false;
            DiskModel top = gameModel.Towers[towerIndex].Peek();
            return top == model;
        }

        public List<Transform> GetTowerTransforms() => towerTransforms;

        public GameModel GetGameModel() => gameModel;

        public void MoveDiskToTower(DiskView disk, int targetTowerIndex)
        {
            if (gameModel == null) return;

            DiskModel model = disk.GetModel();
            int fromIndex = model.TowerIndex;

            if (targetTowerIndex < 0 || targetTowerIndex >= gameModel.Towers.Length)
                return;
            if (fromIndex == targetTowerIndex)
                return;

            TowerModel fromTower = gameModel.Towers[fromIndex];
            TowerModel toTower = gameModel.Towers[targetTowerIndex];

            DiskModel topDisk = fromTower.Peek();
            if (topDisk != model)
            {
                Debug.LogWarning($"[GameController] Tentativo di spostare un disco non in cima dalla Torre {fromIndex}");
                return;
            }

            DiskModel targetTopDisk = toTower.Peek();
            if (targetTopDisk != null && model.Size > targetTopDisk.Size)
            {
                Debug.LogWarning($"[GameController] Non è possibile posizionare il disco {model.Size} sopra il disco {targetTopDisk.Size} nella Torre {targetTowerIndex}. Ritorno alla posizione iniziale.");
                disk.ResetToInitialPosition();
                return;
            }

            fromTower.Pop();
            toTower.Push(model);
            model.TowerIndex = targetTowerIndex;

            Debug.Log($"[GameController] Disco {model.Size} spostato dalla Torre {fromIndex} → Torre {targetTowerIndex}");
        }

        private void ShowVictoryScreen()
        {
            Debug.Log($"[GameController] Vittoria! Tutti i dischi sono impilati in ordine sulla terza torre in {gameModel.MoveCount} mosse.");
        }
    }
}