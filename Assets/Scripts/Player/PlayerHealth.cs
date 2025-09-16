using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    PlayerStats playerStats;
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => maxHealth;
    [field: SerializeField] public int CurrentHealth { get; private set; }

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        CurrentHealth = maxHealth;
    }

    void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.StrengthChanged += OnStrengthChanged; // subskrybuj zdarzenie zmiany si³y
            OnStrengthChanged(0, playerStats.Strength); // inicjalne ustawienie maxHealth na podstawie aktualnej si³y
            Debug.Log($"PlayerHealth: Initial maxHealth set to {maxHealth} based on Strength {playerStats.Strength}");
        }
    }


    void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.StrengthChanged -= OnStrengthChanged; // anuluj subskrypcjê zdarzenia przy wy³¹czeniu
        }
    }



    /// <summary>
    /// Zmienia maksymalne zdrowie gracza o okreœlon¹ wartoœæ.
    /// </summary>
    /// <param name="changeHealth"></param>
    public void ChangeMaxHealth(int changeHealth)
    {
        maxHealth += changeHealth;
    }
    /// <summary>
    /// Zadaje obra¿enia graczowi i sprawdza, czy gracz umar³.    
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
    }

    public void Die()
    {
        Debug.Log("Player has died.");
        CurrentHealth = 0;
        // Add death logic here (e.g., respawn, game over screen)
    }

    /// <summary>
    /// subskrybowana metoda do zdarzenia zmiany si³y
    /// </summary>
    /// <param name="oldStr"></param>
    /// <param name="newStr"></param>
    void OnStrengthChanged(int oldStr, int newStr)
    {
        int deltaStr = newStr - oldStr;          // np. +3
        int deltaHp = deltaStr * 10;            // 1 STR = +10 HP
        ChangeMaxHealth(deltaHp);
        Heal(deltaHp); // Heal to reflect the increase in max health
    }
}
