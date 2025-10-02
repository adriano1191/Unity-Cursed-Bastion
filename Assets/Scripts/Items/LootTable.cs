using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public GameObject prefab;   // prefab pickupa (np. AmuletPickup, CoinPickup)
    public int min = 1;
    public int max = 1;
    public int weight = 1;      // waga w losowaniu (im wiêksza, tym czêœciej)
}

[CreateAssetMenu(menuName = "Loot/Table")]
public class LootTable : ScriptableObject
{
    public int noDropWeight = 0;      // szansa na „nic” (np. 50)
    public List<LootEntry> entries = new(); // lista mo¿liwych ³upów

    public LootEntry Pick()
    {
        int total = noDropWeight; // suma wag
        foreach (var e in entries) total += Mathf.Max(0, e.weight); // ignoruj ujemne wagi
        if (total <= 0) return null; // nic do wylosowania

        int r = Random.Range(0, total); // losuj liczbê od 0 do total-1
        if (r < noDropWeight) return null; // „nic”
        r -= noDropWeight; // zmniejsz o wagê „nic”

        foreach (var e in entries)
        {
            int w = Mathf.Max(0, e.weight);// ignoruj ujemne wagi
            if (r < w) return e; // wylosowano ten ³up
            r -= w; // zmniejsz o wagê tego ³upu i sprawdŸ nastêpny
        }
        return null;
    }
}
