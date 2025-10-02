using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public GameObject prefab;   // prefab pickupa (np. AmuletPickup, CoinPickup)
    public int min = 1;
    public int max = 1;
    public int weight = 1;      // waga w losowaniu (im wi�ksza, tym cz�ciej)
}

[CreateAssetMenu(menuName = "Loot/Table")]
public class LootTable : ScriptableObject
{
    public int noDropWeight = 0;      // szansa na �nic� (np. 50)
    public List<LootEntry> entries = new(); // lista mo�liwych �up�w

    public LootEntry Pick()
    {
        int total = noDropWeight; // suma wag
        foreach (var e in entries) total += Mathf.Max(0, e.weight); // ignoruj ujemne wagi
        if (total <= 0) return null; // nic do wylosowania

        int r = Random.Range(0, total); // losuj liczb� od 0 do total-1
        if (r < noDropWeight) return null; // �nic�
        r -= noDropWeight; // zmniejsz o wag� �nic�

        foreach (var e in entries)
        {
            int w = Mathf.Max(0, e.weight);// ignoruj ujemne wagi
            if (r < w) return e; // wylosowano ten �up
            r -= w; // zmniejsz o wag� tego �upu i sprawd� nast�pny
        }
        return null;
    }
}
