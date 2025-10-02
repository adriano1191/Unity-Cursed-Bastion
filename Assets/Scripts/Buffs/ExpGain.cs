using UnityEngine;
[CreateAssetMenu(menuName = "Buffs/ExpGain")]
public class ExpGain : BuffEffect
{
    public override void OnStart(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
    {
        playerStats.AddStats(PlayerStats.StatType.Experience, Mathf.RoundToInt(magnitude) * stacks);
    }
}
