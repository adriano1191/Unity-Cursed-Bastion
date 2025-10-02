using UnityEngine;

public class MonsterDrop : MonoBehaviour
{
    [Header("Item Drop Settings")]
    [SerializeField] GameObject[] itemPrefabs; // Array of item prefabs to drop
    [SerializeField] [Range(0f, 1f)] float dropChance = 0.5f; // Chance to drop an item
    [SerializeField] int minItemsToDrop = 1; // Minimum number of items to drop
    [SerializeField] int maxItemsToDrop = 3; // Maximum number of items to drop
    [SerializeField] Vector3 dropAreaSize = new Vector3(1, 0, 1); // Area size for random drop positions


    public void DropItem(GameObject itemPrefab, Vector3 position)
    {
        Instantiate(itemPrefab, position, Quaternion.identity);
    }
}
