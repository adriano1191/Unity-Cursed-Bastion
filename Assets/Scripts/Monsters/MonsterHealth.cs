using UnityEngine;

[RequireComponent(typeof(MonsterStats))]
public class MonsterHealth : MonoBehaviour
{
    public int currentHealth { get; private set; }
    public int maxHealth { get; private set; } = 0;
    AudioSource sfx;
    [Range(0f, 1f)] float volume = 1f;
    SpawnerManager spawnerManager;
    public GameObject xpPrefab;
    public GameObject bloodPrefab;
    public GameObject bloodHit;
    public bool IsDead => currentHealth <= 0;

    private MonsterStats monsterStats;

    private void Awake()
    {
        monsterStats = GetComponent<MonsterStats>();
        maxHealth = monsterStats.maxHealth;
        currentHealth = maxHealth;
        sfx = GetComponent<AudioSource>();
    }

    public bool TakeDamage(int damage)
    {
        if (IsDead) return false;
        currentHealth -= damage;
        if (bloodHit)
        {
            Instantiate(bloodHit, transform.position, Quaternion.identity);
        }
        //OnHitSFX();
        if (currentHealth <= 0)
        {
            if (xpPrefab)
            {
                DropXp(xpPrefab, transform.position);
            }
            if (bloodPrefab)
            {
                Instantiate(bloodPrefab, transform.position, Quaternion.identity);
            }
            GetComponent<LootDropper>()?.Drop();
            Die();
            return true;
        }
        return false;
    }   

    private void Die()
    {
        // Logika œmierci potwora (np. animacja, usuniêcie z gry)
        if (!spawnerManager) spawnerManager = FindFirstObjectByType<SpawnerManager>();
        spawnerManager?.RemoveEnemy();
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }

    private void OnHitSFX()
    {
        if(!sfx) return;
        sfx.pitch = Random.Range(0.8f, 1.2f);
        sfx.PlayOneShot(sfx.clip, volume);

    }

    public void DropXp(GameObject itemPrefab, Vector3 position)
    {
        for (int i = 0; i < Random.Range(1, 3); i++) // losowa liczba przedmiotów do upuszczenia (1-3)
        {
            position = Random.insideUnitSphere * 0.8f + position; // losowa pozycja w promieniu 0.5 jednostki
            Instantiate(itemPrefab, position, Quaternion.identity);
        }
    }
}
