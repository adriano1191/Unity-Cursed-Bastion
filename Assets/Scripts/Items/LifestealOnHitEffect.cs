using UnityEngine;

[CreateAssetMenu(menuName = "Items/Effects/Lifesteal OnHit (%)")]
public class LifestealOnHitEffect : ItemEffect
{
    // magnitude = 0.05f ⇒ 5% za 1 stack
    public override void OnHit(PlayerStats playerStats, PlayerHealth playerHealth, GameObject target, ref int damage, int stacks, float magnitude)
    {
        int heal = Mathf.Max(1, Mathf.RoundToInt(damage * (magnitude * stacks)));
        //playerHealth.currentHealth = Mathf.Min(playerHealth.maxHealth, playerHealth.currentHealth + heal);
        playerHealth.Heal(heal);
        Debug.Log($"Lifesteal: Healed {heal} HP on hit.");
    }
}
