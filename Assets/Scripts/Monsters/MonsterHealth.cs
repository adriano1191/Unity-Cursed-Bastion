using UnityEngine;

[RequireComponent(typeof(MonsterStats))]
public class MonsterHealth : MonoBehaviour
{
    public int currentHealth { get; private set; }
    public int maxHealth { get; private set; } = 0;
    AudioSource sfx;
    [Range(0f, 1f)] float volume = 1f;

    private MonsterStats monsterStats;

    private void Awake()
    {
        monsterStats = GetComponent<MonsterStats>();
        maxHealth = monsterStats.maxHealth;
        currentHealth = maxHealth;
        sfx = GetComponent<AudioSource>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        //OnHitSFX();
        if (currentHealth <= 0)
        {
            Die();
        }
    }   

    private void Die()
    {
        // Logika œmierci potwora (np. animacja, usuniêcie z gry)
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }

    private void OnHitSFX()
    {
        if(!sfx) return;
        sfx.pitch = Random.Range(0.8f, 1.2f);
        sfx.PlayOneShot(sfx.clip, volume);

    }
}
