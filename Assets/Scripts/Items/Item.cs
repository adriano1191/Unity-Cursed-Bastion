using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public Sprite itemIcon;
    public string itemDescription;

    public int healthBonus;
    /*
    public void ApplyEffect(PlayerStats playerStats)
    {
        playerStats.maxHealth += healthBonus;
        playerStats.currentHealth += healthBonus; // opcjonalnie zwiêksz aktualne zdrowie
        Debug.Log($"Applied {itemName} effect: +{healthBonus} max health" );
    }

    public void RemoveEffect(PlayerStats playerStats)
    {
        playerStats.maxHealth -= healthBonus;
        playerStats.currentHealth -= healthBonus; // opcjonalnie zmniejsz aktualne zdrowie
        Debug.Log($"Removed {itemName} effect: -{healthBonus} max health");
    }
    */
}
