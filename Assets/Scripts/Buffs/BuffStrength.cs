using UnityEngine;
[CreateAssetMenu(menuName = "Buffs/Strength Bonus (Timed)")]
public class BuffStrength : BuffEffect
{

    public override void OnStart(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, string name, string description, Sprite icon, float magnitude)
    => playerStats.AddStats(PlayerStats.StatType.Strength, Mathf.RoundToInt(magnitude) * stacks);

    public override void OnEnd(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, string name, string description, Sprite icon, float magnitude)
        => playerStats.AddStats(PlayerStats.StatType.Strength, - Mathf.RoundToInt(magnitude) * stacks);

    /*
    public override void OnStart(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
        => playerStats.AddStrength(Mathf.RoundToInt(magnitude) * stacks);

    public override void OnEnd(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude)
        => playerStats.AddStrength(-Mathf.RoundToInt(magnitude) * stacks);
    */

}
