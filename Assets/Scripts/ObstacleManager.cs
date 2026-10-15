using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager Instance { get; private set; }

    [SerializeField] private bool useRandomLayout = false;
    [SerializeField] private float minDistanceFromFence = 1.5f;
    [SerializeField] private float minDistanceFromCenter = 3f;
    [SerializeField] private float minDistanceBetween = 3f;
    [SerializeField] private int seed = 12345;

    public GameObject[] obstaclePrefabs;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadPrefabs();

        if (useRandomLayout)
        {
            GenerateRandomLayout();
        }
        else
        {
            // Default fixed layout - just load fixed ones or generate a known good seed
            GenerateRandomLayout(42); 
        }
    }

    private void LoadPrefabs()
    {
        var allPrefabs = Resources.LoadAll<GameObject>("Obstacles");
        List<GameObject> filtered = new List<GameObject>();
        foreach(var p in allPrefabs) {
            // Don't add Sword, WoodLog, or TreeStump
            if (!p.name.Contains("Sword") && !p.name.Contains("WoodLog") && !p.name.Contains("TreeStump")) 
            {
                filtered.Add(p);
            }
        }
        obstaclePrefabs = filtered.ToArray();
    }

    public void GenerateRandomLayout(int? specificSeed = null)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        Random.InitState(specificSeed.HasValue ? specificSeed.Value : seed);
        
        // Clear existing obstacles except Sword
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Obstacle_") && !child.name.Contains("Sword")) {
                Destroy(child.gameObject);
            }
        }
        
        float spawnAreaX = 8.5f - minDistanceFromFence;
        float spawnAreaZ = 8.5f - minDistanceFromFence;

        List<Vector3> placedPositions = new List<Vector3>();
        int bigCount = 0;

        // Collect existing sword
        foreach (Transform child in transform) {
            placedPositions.Add(child.position);
        }

        int targetObstacles = Random.Range(6, 9);
        int[] quadrants = new int[] { 0, 1, 2, 3 };
        int quadIndex = 0;

        for (int i = 0; i < targetObstacles; i++)
        {
            Vector3 pos = Vector3.zero;
            bool valid = false;
            int attempts = 0;

            int currentQuad = quadrants[quadIndex % 4];
            quadIndex++;

            while (!valid && attempts < 100)
            {
                attempts++;
                float rx = Random.Range(0.1f, spawnAreaX);
                float rz = Random.Range(0.1f, spawnAreaZ);
                
                if (currentQuad == 0) { rx = -rx; rz = -rz; }
                else if (currentQuad == 1) { rx = -rx; }
                else if (currentQuad == 2) { rz = -rz; }

                pos = new Vector3(rx, 0f, rz);

                if (Vector3.Distance(Vector3.zero, pos) < minDistanceFromCenter) continue;
                if (Mathf.Abs(pos.x) < 1.5f && Mathf.Abs(pos.z) < 1.5f) continue;

                bool farEnough = true;
                foreach (var p in placedPositions)
                {
                    if (Vector3.Distance(p, pos) < minDistanceBetween)
                    {
                        farEnough = false;
                        break;
                    }
                }

                if (farEnough) valid = true;
            }

            if (!valid)
            {
                Debug.LogWarning("Failed to find valid pos, using fallback");
                pos = new Vector3(Random.Range(-spawnAreaX, spawnAreaX), 0, Random.Range(-spawnAreaZ, spawnAreaZ));
            }

            placedPositions.Add(pos);
            int pIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject prefabToSpawn = obstaclePrefabs[pIndex];
            
            if (prefabToSpawn.name.Contains("Big") || prefabToSpawn.name.Contains("Sword"))
            {
                if (bigCount >= 2) prefabToSpawn = obstaclePrefabs[0]; 
                else bigCount++;
            }

            float scaleRandom = Random.Range(2.2f, 3.5f); // Increased scale to make rocks bigger
            
            GameObject inst = Instantiate(prefabToSpawn, pos, Quaternion.Euler(0, Random.Range(0, 360f), 0), transform);
            inst.transform.localScale *= scaleRandom;
            
            // Embed 5% into ground based on height (less embedding so they stick out more)
            if (!prefabToSpawn.name.Contains("Sword")) 
            {
                Renderer r = inst.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float height = r.bounds.size.y;
                    inst.transform.position = new Vector3(pos.x, -height * 0.05f, pos.z);
                    
                    // Add a BoxCollider to ensure it acts as a vertical wall and prevents climbing
                    BoxCollider bc = r.gameObject.GetComponent<BoxCollider>();
                    if (bc == null) bc = r.gameObject.AddComponent<BoxCollider>();
                    // MeshCollider might exist, BoxCollider will overpower its slopes
                }
            }
        }
    }
}
