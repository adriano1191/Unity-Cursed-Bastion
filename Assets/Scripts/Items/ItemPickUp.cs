using UnityEngine;
using UnityEngine.Rendering;
[RequireComponent(typeof(Collider2D))]

public class ItemPickUp : MonoBehaviour
{
    public ItemDefinition def;
    public int amount = 1;
    public AudioClip audioClip;
    public AudioSource sfx;
    [SerializeField, Range(0f, 1f)] float volume = 1f;


    public void OnTriggerEnter2D(Collider2D other)
    {


        if (!other.CompareTag("Player")) return;
        var inv = other.GetComponentInParent<Inventory>();
        if (!inv) return;

        inv.Add(def, amount);
        OnPickUpSFX();
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

    private void OnPickUpSFX()
    {
        if(!sfx && !audioClip) return;
        sfx.transform.SetParent(null);           // odczep, ¿eby ¿y³ po zniszczeniu pocisku
        sfx.pitch = Random.Range(0.8f, 1.2f);
        sfx.PlayOneShot(audioClip, volume);
        Destroy(sfx.gameObject, audioClip.length);

    }
}
