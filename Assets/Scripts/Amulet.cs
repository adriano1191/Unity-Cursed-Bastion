// 04.09.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using Unity.VisualScripting;
using UnityEngine;

public class CreateAmulet : MonoBehaviour
{
    //public PlayerStats playerStats;
    private Item amulet;
    public Sprite icon; // Ikona amuletu

    private void Start()
    {
        // Tworzenie nowego przedmiotu
        amulet = new Item
        {
            itemName = "Amulet Zdrowia",
            itemIcon = icon, // Przypisanie ikony
            itemDescription = "Amulet, który zwiększa maksymalne zdrowie o 50.",
            healthBonus = 50
        };

        // Zastosowanie efektu amuletu
        //amulet.ApplyEffect(playerStats);

        // Wyświetlenie informacji o przedmiocie
        //Debug.Log($"Stworzono przedmiot: {amulet.itemName} - {amulet.itemDescription}");

    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PickUp(other.gameObject);
        }
    }
    public void PickUp(GameObject player)
    {

        if (!player.CompareTag("Player")) return;

        var inv = player.GetComponent<Inventory>();
        if (inv == null)
        {
            Debug.LogWarning("Player nie ma komponentu Inventory.");
            return;
        }

      //  inv.AddItem(amulet);   // ← dodajemy do listy i nakładamy efekt


        // Tu dodaj logikę podnoszenia przedmiotu, np. dodanie do ekwipunku gracza
        //player.GetComponent<Inventory>().AddItem();
        Debug.Log($"Picked up {gameObject.name}");
        Destroy(gameObject); // Usuwa przedmiot ze sceny po podniesieniu

    }
}