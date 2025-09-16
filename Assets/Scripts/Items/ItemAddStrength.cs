using UnityEngine;

[CreateAssetMenu(menuName = "Items/Effects/Add Strength (Flat)")]
public class ItemAddStrength : ItemEffect
{
    public override void Apply(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
    {
        int add = Mathf.RoundToInt(magnitude) * stacks;
        playerStats.AddStrength(add);
    }

    public override void Remove(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
    {
        int sub = Mathf.RoundToInt(magnitude) * stacks;
        playerStats.AddStrength(-sub);
    }
}
