using UnityEngine;

public class DiedMenu : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] SpawnerManager SpawnManager;
    [SerializeField] GameObject timer;
    [SerializeField] GameObject DiedMenuPanel;

    [SerializeField] TMPro.TextMeshProUGUI killCountText;
    [SerializeField] TMPro.TextMeshProUGUI timeText;

    void Awake()
    {
        if(!playerHealth)playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        if(!SpawnManager) SpawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnerManager>();
        if(!timer) timer = GameObject.Find("Timer");


    }
    void Start()
    {
        //DiedMenuPanel = transform.GetChild(0).gameObject;
        DiedMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (playerHealth && playerHealth.CurrentHealth <= 0)
        {
            DiedMenuPanel.SetActive(true);
            OnActive();
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;

        }
    }

    void OnActive()
    {
        killCountText.text = $"Kills: {SpawnManager.GetKillCount()}";
        timeText.text = $"Time: {timer.GetComponent<Timer>().GetTime()}";
        Time.timeScale = 0f;
    }
}
