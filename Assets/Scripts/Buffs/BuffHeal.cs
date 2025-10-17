using UnityEngine;
[CreateAssetMenu(menuName = "Buffs/Heal")]
public class BuffHeal : BuffEffect
{
    public override void OnStart(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, string name, string description, Sprite icon, float magnitude)
    {
        playerHealth.Heal(Mathf.RoundToInt(magnitude) * stacks);
    }
}
