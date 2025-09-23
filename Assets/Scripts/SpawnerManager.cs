using UnityEngine;

public class SpawnerManager : MonoBehaviour
{

    public int maxEnemies = 2;
    public int currentEnemies = 0;
    public int killCount = 0;
    public float timer = 0f;
    public float interval = 10f;
    public float nextTick;

    private void Start()
    {
        nextTick = interval;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        MaxEnemiesChange();
    }

    public void AddEnemy()
    {
        currentEnemies++;
    }

    public void RemoveEnemy()
    {
        if (currentEnemies > 0)
        {
            currentEnemies--;
            killCount++;
        }
    }

    public void MaxEnemiesChange()
    {
        while (timer >= nextTick)    // while, ¿eby nie zgubiæ ticków przy lagach
        {
            maxEnemies += 2;
            nextTick += interval;
        }
    }

}
