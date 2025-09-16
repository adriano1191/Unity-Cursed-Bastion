using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{
    // PASYWNE – nak³adane przy dodaniu itemu, zdejmowane przy usuniêciu
    public virtual void Apply(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude) { }
    public virtual void Remove(PlayerStats playerStats, PlayerHealth playerHealth, int stacks, float magnitude) { }

    // ZDARZENIA BOJOWE
    public virtual void OnHit(PlayerStats playerStats, PlayerHealth playerHealth, GameObject target, ref int damage, int stacks, float magnitude) { }
    public virtual void OnDamageTaken(PlayerStats playerStats, PlayerHealth playerHealth, GameObject source, ref int damage, int stacks, float magnitude) { }
    public virtual void OnKill(PlayerStats playerStats, PlayerHealth playerHealth, GameObject target, int stacks, float magnitude) { }
}
