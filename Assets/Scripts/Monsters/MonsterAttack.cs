using UnityEngine;

[RequireComponent(typeof(MonsterStats))]
public class MonsterAttack : MonoBehaviour
{
    MonsterStats monsterStats;
    public float attackCooldown { get; private set; }
    public int attackPower { get; private set; } = 0;
    public float attackRange { get; private set; } = 0;
    float lastAttack = 0f;
    [SerializeField] private Collider2D myCol;      // collider przeciwnika
    [SerializeField] private Collider2D targetCol;  // collider gracza
    Transform target;
    PlayerHealth playerHealth;


    private void Awake()
    {
        monsterStats = GetComponent<MonsterStats>();
        attackPower = monsterStats.attackPower;
        attackCooldown = monsterStats.attackCooldown;
        attackRange = monsterStats.attackRange;

        target = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = target.GetComponent<PlayerHealth>();

        if (!myCol) myCol = GetComponent<Collider2D>();
        if (!targetCol) targetCol = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();
    }

    public void Update()
    {

        Attack();

    }

    public void Attack()
    {
        if (lastAttack >= attackCooldown)
        {

            //float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            ColliderDistance2D d = Physics2D.Distance(myCol, targetCol);
            if (d.distance <= attackRange)
            {

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackPower);
                    lastAttack = 0;
                    //Debug.Log($"{gameObject.name} attacked {target.name} for {attackPower} damage.");
                }
            }
        }
        else
        {
            lastAttack += Time.deltaTime;
        }
    }
}
