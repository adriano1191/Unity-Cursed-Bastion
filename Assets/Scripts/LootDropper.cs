using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [SerializeField] LootTable table;
    [SerializeField] int rolls = 1;          // ile �losowa� zrobi� (np. 2)
    [SerializeField] float scatterRadius = 0.5f;
    [SerializeField] float impulse = 2f;

    public void Drop()
    {
        if (!table) return;
        for (int i = 0; i < rolls; i++) // wykonaj okre�lon� liczb� losowa�
        {
            var entry = table.Pick(); // wylosuj jeden wpis z tabeli
            if (entry == null || !entry.prefab) continue; // nic nie wylosowano lub brak prefabryki

            int qty = Random.Range(entry.min, entry.max + 1); // losuj ilo�� w zakresie [min, max]
            for (int q = 0; q < qty; q++) 
            {
                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * scatterRadius); // losuj pozycj� w promieniu
                var go = Object.Instantiate(entry.prefab, pos, Quaternion.identity); // utw�rz pickup

                var rb = go.GetComponent<Rigidbody2D>(); // dodaj losowy impuls
                if (rb) rb.AddForce(Random.insideUnitCircle.normalized * impulse, ForceMode2D.Impulse); // losowy kierunek
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scatterRadius);
    }
}
