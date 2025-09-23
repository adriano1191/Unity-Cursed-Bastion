using UnityEngine;

[CreateAssetMenu(menuName = "Items/Effects/Add Agility (Flat)")]

public class ItemAddAgility : ItemEffect
{
    public override void Apply(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
    {
        int add = Mathf.RoundToInt(magnitude) * stacks;
        playerStats.AddStats(PlayerStats.StatType.Agility, add);
    }

    public override void Remove(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
    {
        int sub = Mathf.RoundToInt(magnitude) * stacks;
        playerStats.AddStats(PlayerStats.StatType.Agility, -sub);
    }
}
