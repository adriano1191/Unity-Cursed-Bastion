using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class SpawnerMonster : MonoBehaviour
{
    public GameObject monsterPrefab;
    public float spawnInterval = 3f;
    public float spawnIntervalRandom = 3f;
    private float spawnRandomTimer = 0f;
    private float timer;
    private Transform playerTransform;
    public float spawnRadius = 2f;
    public bool showRayCast = false;
    public SpawnerManager spawnerManager;


    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        spawnRandomTimer = Random.Range(0f, spawnIntervalRandom); //wyliczanie losowego czasu na start
        if(!spawnerManager) spawnerManager = FindFirstObjectByType<SpawnerManager>();

    }

    private void Update()
    {
        SpawnMonster();
        if (showRayCast)
        {
            Debug.DrawRay(transform.position, Vector2.left * spawnRadius);
            Debug.DrawRay(transform.position, Vector2.right * spawnRadius);
            Debug.DrawRay(transform.position, Vector2.up * spawnRadius);
            Debug.DrawRay(transform.position, Vector2.down * spawnRadius);

        }


    }


    private void SpawnMonster()
    {

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        //if (currentMonsterCount >= maxMonsters) return;
        //Vector2 spawnPosition = (Vector2)transform.position;
        timer += Time.deltaTime;
        float spawnTimer = spawnInterval + spawnRandomTimer;
        if (timer >= spawnTimer && spawnerManager.currentEnemies < spawnerManager.maxEnemies && distanceToPlayer >= spawnRadius*2)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector2 spawnPosition = randomDirection * spawnRadius;
            Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
            //currentMonsterCount++;
            spawnerManager.AddEnemy();
            timer = 0f;
            spawnRandomTimer = Random.Range(0f, spawnIntervalRandom);
           // Debug.Log($"Spawned monster at {spawnPosition}. Current count: {currentMonsterCount}");

        }

    }
}
