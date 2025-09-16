using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class ItemStack { public ItemDefinition def; public int count; }

public class Inventory : MonoBehaviour
{
    [SerializeField] private PlayerStats ownerStats;
    [SerializeField] private PlayerHealth ownerHealth;
    [SerializeField] private List<ItemStack> slots = new();

    void Awake() 
    { 
        if (!ownerStats) ownerStats = GetComponent<PlayerStats>();
        if (!ownerHealth) ownerHealth = GetComponent<PlayerHealth>();

    }

    // DODAWANIE (stackowalne)
    public void Add(ItemDefinition def, int amount = 1)
    {
        // 1) Item jednorazowy zastosuj i NIE dodawaj do slots
        if (def.consumeOnPickup)
        {
            foreach (var e in def.effects)
                if (e.trigger == ItemTrigger.Passive)
                    e.effect?.Apply(ownerStats, ownerHealth, amount, e.magnitude); // w Twoim case: GrantTimedBuffEffect.Apply  BuffManager.AddBuff(...)
            return;
        }

        var s = slots.Find(x => x.def == def);
        if (s == null) { s = new ItemStack { def = def, count = 0 }; slots.Add(s); }

        int add = Mathf.Min(amount, def.maxStack - s.count);
        if (add <= 0) return;
        s.count += add;

        // pasywy: dołóż różnicę
        foreach (var e in def.effects)
            if (e.trigger == ItemTrigger.Passive)
                e.effect?.Apply(ownerStats, ownerHealth, add, e.magnitude);
    }

    // USUWANIE (zmniejsz stack)
    public void Remove(ItemDefinition def, int amount = 1)
    {
        var s = slots.Find(x => x.def == def);
        if (s == null) return;

        int rem = Mathf.Min(amount, s.count);
        s.count -= rem;

        foreach (var e in def.effects)
            if (e.trigger == ItemTrigger.Passive)
                e.effect?.Remove(ownerStats, ownerHealth, rem, e.magnitude);

        if (s.count <= 0) slots.Remove(s);
    }

    // WYWOŁANIA ZDARZEŃ – wołasz z walki
    public void NotifyOnHit(GameObject target, ref int damage)
    {
        foreach (var s in slots)
            foreach (var e in s.def.effects)
                if (e.trigger == ItemTrigger.OnHit)
                    e.effect?.OnHit(ownerStats, ownerHealth, target, ref damage, s.count, e.magnitude);
    }

    public void NotifyOnDamageTaken(GameObject source, ref int damage)
    {
        foreach (var s in slots)
            foreach (var e in s.def.effects)
                if (e.trigger == ItemTrigger.OnDamageTaken)
                    e.effect?.OnDamageTaken(ownerStats, ownerHealth, source, ref damage, s.count, e.magnitude);
    }

    public void NotifyOnKill(GameObject target)
    {
        foreach (var s in slots)
            foreach (var e in s.def.effects)
                if (e.trigger == ItemTrigger.OnKill)
                    e.effect?.OnKill(ownerStats, ownerHealth, target, s.count, e.magnitude);
    }
}
