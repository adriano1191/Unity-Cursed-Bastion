using UnityEngine;

[CreateAssetMenu(menuName = "Items/Effects/Health Bonus (Flat)")]
public class HealthBonusEffect : ItemEffect
{
    public override void Apply(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
    {
        int add = Mathf.RoundToInt(magnitude) * stacks;
        //playerStats.maxHealth += add;
        //playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth + add, playerStats.maxHealth);
        playerHealth.ChangeMaxHealth(add);
        playerHealth.Heal(add);
    }

    public override void Remove(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
    {
        int sub = Mathf.RoundToInt(magnitude) * stacks;
        //playerHealth.maxHealth -= sub;
        //playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth, playerHealth.maxHealth);
        playerHealth.ChangeMaxHealth(-sub);
        playerHealth.Heal(0); // just to clamp current health if needed
    }
}
