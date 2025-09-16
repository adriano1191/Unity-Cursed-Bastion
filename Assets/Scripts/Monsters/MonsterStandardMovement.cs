using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MonsterStats))]
public class MonsterStandardMove : MonoBehaviour
{
    Rigidbody2D rb;
    MonsterStats stats;
    Transform player;
    private float stunDuration = 0f;
    private bool isStunned = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<MonsterStats>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void FixedUpdate()
    {
        if (player != null)
        {
            if (!isStunned)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.MovePosition(rb.position + direction * stats.moveSpeed * Time.fixedDeltaTime);
                Flip(direction.x > 0);
            }
            else 
            {
                stunDuration -= Time.deltaTime;
                if (stunDuration <= 0)
                {
                    isStunned = false;
                    stunDuration = 0f;
                }
            }

        }
    }

    public void Flip(bool facingRight)
    {
        /*
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
        */
        SpriteRenderer flip = GetComponent<SpriteRenderer>();
        flip.flipX = facingRight;
    }

    public void Stun(float stunTime)
    {
        stunDuration = stunTime;
        if (stunDuration > 0)
            {
                isStunned = true;
            }

    }
}

