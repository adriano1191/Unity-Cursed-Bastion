using UnityEngine;

public class KillCount : MonoBehaviour
{
    TMPro.TextMeshProUGUI killCountText;
    [SerializeField] SpawnerManager spawnerManager;

    void Start()
    {
        killCountText = GetComponent<TMPro.TextMeshProUGUI>();
        if (!spawnerManager) spawnerManager = FindFirstObjectByType<SpawnerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        spawnerManager.GetKillCount();
        killCountText.text = $"Kills: {spawnerManager.GetKillCount()}";

    }
}
