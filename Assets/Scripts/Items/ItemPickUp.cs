using UnityEngine;
[RequireComponent(typeof(Collider2D))]

public class ItemPickUp : MonoBehaviour
{
    public ItemDefinition def;
    public int amount = 1;

    public void OnTriggerEnter2D(Collider2D other)
    {


        if (!other.CompareTag("Player")) return;
        var inv = other.GetComponentInParent<Inventory>();
        if (!inv) return;

        inv.Add(def, amount);
        Destroy(gameObject);

        /*if (other.CompareTag("Player"))
        {
            PickUp(other.gameObject);
        }*/

    }
    public void PickUp(GameObject player)
    {
        // Tu dodaj logikê podnoszenia przedmiotu, np. dodanie do ekwipunku gracza
        Debug.Log($"Picked up {gameObject.name}");
        Destroy(gameObject); // Usuwa przedmiot ze sceny po podniesieniu
        
    }
}
