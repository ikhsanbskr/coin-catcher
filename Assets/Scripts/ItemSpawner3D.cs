using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner3D : MonoBehaviour
{
    [SerializeField] private float initialSpawnInterval = 1.5f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    
    [SerializeField] private float coinStartSpeed = 2.0f;
    [SerializeField] private float coinSpeedIncrease = 0.3f;
    [SerializeField] private float coinMaxSpeed = 5.0f;

    [SerializeField] private float bombStartSpeed = 3.0f;
    [SerializeField] private float bombSpeedIncrease = 0.4f;
    [SerializeField] private float bombMaxSpeed = 7.0f;

    public float spawnY = 12f; 
    [SerializeField] private float spawnAreaX = 8.5f;
    [SerializeField] private float spawnAreaZ = 8.5f;
    [SerializeField] private float nearPlayerChance = 0.2f;
    [SerializeField] private float nearRadiusMin = 3f;
    [SerializeField] private float nearRadiusMax = 6f;
    [SerializeField] private float minBombDistance = 2f;
    [SerializeField] private float minItemDistance = 1.5f;
    [SerializeField] private int maxCoins = 3;
    [SerializeField] private int maxBombs = 2;
    
    private Transform playerTrans;
    private float spawnTimer;
    private float gameTimer;
    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnGameStarted += ResetSpawner;
        }
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) playerTrans = pObj.transform;
        CalculateSpawnBounds();
    }

    private void OnDestroy()
    {
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnGameStarted -= ResetSpawner;
        }
    }

    private void CalculateSpawnBounds()
    {
        float fMinX = float.MaxValue, fMaxX = float.MinValue;
        float fMinZ = float.MaxValue, fMaxZ = float.MinValue;
        GameObject env = GameObject.Find("Environment");
        if (env != null)
        {
            foreach (Transform child in env.transform)
            {
                if (child.name.Contains("Fence"))
                {
                    Vector3 pos = child.position;
                    if (pos.x < fMinX) fMinX = pos.x;
                    if (pos.x > fMaxX) fMaxX = pos.x;
                    if (pos.z < fMinZ) fMinZ = pos.z;
                    if (pos.z > fMaxZ) fMaxZ = pos.z;
                }
            }
            float margin = 1.5f;
            if (fMinX != float.MaxValue)
            {
                spawnAreaX = (fMaxX - fMinX) * 0.5f - margin;
                spawnAreaZ = (fMaxZ - fMinZ) * 0.5f - margin;
            }
        }
    }

    private void ResetSpawner()
    {
        spawnTimer = 0f;
        gameTimer = 0f;
    }

    private void Update()
    {
        if (GameManager3D.Instance == null || GameManager3D.Instance.CurrentState != GameManager3D.GameState.Playing) return;

        spawnTimer += Time.deltaTime;
        gameTimer += Time.deltaTime;
        
        float currentSpawnRate = Mathf.Max(minSpawnInterval, initialSpawnInterval - (gameTimer / 15f) * 0.2f);

        if (spawnTimer >= currentSpawnRate)
        {
            SpawnItem();
            spawnTimer = 0f;
        }
    }

    private void SpawnItem()
    {
        int currentMaxCoins = maxCoins + Mathf.FloorToInt(gameTimer / 30f);
        int currentMaxBombs = maxBombs + Mathf.FloorToInt(gameTimer / 45f);

        var allItems = FindObjectsByType<FallingItem3D>(FindObjectsInactive.Include);
        List<Vector3> activePositions = new List<Vector3>();
        int activeCoins = 0; int activeBombs = 0;
        foreach (var i in allItems) {
            if (!i.gameObject.activeInHierarchy) continue;
            activePositions.Add(i.transform.position);
            if (i.type == FallingItem3D.ItemType.Coin) activeCoins++;
            else activeBombs++;
        }

        bool canSpawnCoin = activeCoins < currentMaxCoins;
        bool canSpawnBomb = activeBombs < currentMaxBombs;

        if (!canSpawnCoin && !canSpawnBomb) return;

        string poolTag = "Coin3D";
        if (canSpawnBomb && (!canSpawnCoin || Random.value > 0.75f)) poolTag = "Bomb3D";

        float randomX = 0f, randomZ = 0f;
        Vector3 spawnPos = Vector3.zero;
        bool validPos = false;
        int attempts = 0;

        while (!validPos && attempts < 15)
        {
            attempts++;
            if (playerTrans != null && Random.value < nearPlayerChance)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(nearRadiusMin, nearRadiusMax);
                randomX = playerTrans.position.x + Mathf.Cos(angle) * radius;
                randomZ = playerTrans.position.z + Mathf.Sin(angle) * radius;
            }
            else
            {
                randomX = Random.Range(-spawnAreaX, spawnAreaX);
                randomZ = Random.Range(-spawnAreaZ, spawnAreaZ);
            }

            randomX = Mathf.Clamp(randomX, -spawnAreaX, spawnAreaX);
            randomZ = Mathf.Clamp(randomZ, -spawnAreaZ, spawnAreaZ);

            spawnPos = new Vector3(randomX, spawnY, randomZ);
            
            bool isFarEnoughFromPlayer = true;
            if (poolTag == "Bomb3D" && playerTrans != null)
            {
                float dist = Vector2.Distance(new Vector2(randomX, randomZ), new Vector2(playerTrans.position.x, playerTrans.position.z));
                if (dist < minBombDistance) isFarEnoughFromPlayer = false;
            }
            
            bool isFarEnoughFromOthers = true;
            foreach (var pos in activePositions)
            {
                if (Vector2.Distance(new Vector2(randomX, randomZ), new Vector2(pos.x, pos.z)) < minItemDistance)
                {
                    isFarEnoughFromOthers = false;
                    break;
                }
            }

            bool isFarEnoughFromObstacles = !Physics.CheckSphere(new Vector3(randomX, 0.5f, randomZ), 1.5f, LayerMask.GetMask("Obstacle"));

            if (isFarEnoughFromPlayer && isFarEnoughFromOthers && isFarEnoughFromObstacles)
            {
                validPos = true;
            }
        }

        if (!validPos) return; 
        
        GameObject spawned = ObjectPooler3D.Instance.SpawnFromPool(poolTag, spawnPos, Quaternion.identity);
        if (spawned != null)
        {
            FallingItem3D item = spawned.GetComponent<FallingItem3D>();
            if (item != null)
            {
                int intervals = Mathf.FloorToInt(gameTimer / 15f);
                if (item.type == FallingItem3D.ItemType.Coin)
                {
                    item.Initialize(Mathf.Min(coinMaxSpeed, coinStartSpeed + intervals * coinSpeedIncrease));
                }
                else
                {
                    item.Initialize(Mathf.Min(bombMaxSpeed, bombStartSpeed + intervals * bombSpeedIncrease));
                }
            }
        }
    }
}
