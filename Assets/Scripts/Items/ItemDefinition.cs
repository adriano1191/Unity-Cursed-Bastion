using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectEntry
{
    public ItemTrigger trigger;
    public ItemEffect effect;
    public float magnitude = 0f; // si³a efektu (np. +25 HP, 0.05 = 5% itd.)
}

[CreateAssetMenu(menuName = "Items/Definition")]
public class ItemDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int maxStack = 99;
    public bool consumeOnPickup = false;
    public List<EffectEntry> effects = new();
}
