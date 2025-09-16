using UnityEngine;

public class HealthBar : MonoBehaviour
{
    Vector3 localScale;
    public GameObject target;
    public float maxHealth = 100;
    public float currentHealth =100;
    private float barSize;
    void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        if(!target) { 
            target = transform.root.gameObject;
            //Debug.LogError("HealthBar: Target is not assigned.");
            //return;
        }
        
        if (target)
        {
            if (target.CompareTag("Player"))
            {
                maxHealth = target.GetComponent<PlayerHealth>().MaxHealth;
            }
            else if (target.CompareTag("Monster"))
            {
                maxHealth = target.GetComponent<MonsterHealth>().maxHealth;
            }
        }
        




        localScale = transform.localScale;
    }
    void Update()
    {
        if (target)
        {
            if (target.CompareTag("Player"))
            {
                maxHealth = target.GetComponent<PlayerHealth>().MaxHealth;
                HealthUpdate(target.GetComponent<PlayerHealth>().CurrentHealth);
            }
            else if (target.CompareTag("Monster"))
            {
                HealthUpdate(target.GetComponent<MonsterHealth>().currentHealth);

            }
        }


    }

    public void HealthUpdate(float health)
    {
        if (health <= 0)
        {
            health = 0;
        }
        if (target && health >=0)
        {
            currentHealth = health;
            localScale.x = currentHealth / maxHealth;
            transform.localScale = localScale;
        }


    }


}
