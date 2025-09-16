using UnityEngine;

[CreateAssetMenu(menuName = "Items/Effects/Grant Timed Buff")]
public class GrantTimedBuffEffect : ItemEffect
{
    public BuffEffect buff;
    public float duration = 5f;
    public bool stackable = false;
    public override void Apply(PlayerStats splayerStats, PlayerHealth playerHealth, int stacks, float magnitude)
    => splayerStats.GetComponent<BuffManager>()?.AddBuff(buff, duration, magnitude, stacks, true, stackable);

}
