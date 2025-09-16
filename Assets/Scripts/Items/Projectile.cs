using Unity.AppUI.UI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float lifeTime = 3f;
    public int baseDamage = 25;
    public float knockbackForce = 4f;

    private Rigidbody2D rb;
    private Inventory playerInventory;
    public AudioSource sfx;
    public AudioClip hitClip;
    [SerializeField, Range(0f, 1f)] float volume = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //sfx = GetComponent<AudioSource>();
    }

    public void Init(Inventory ownerInv) => playerInventory = ownerInv;
    public void Fire(Vector2 dir, float speed)
    {
        rb.linearVelocity = dir.normalized * speed;

        // obr�� grafik� be�tu w kierunku lotu (1 linia)
        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        // tu p�niej dodasz zadawanie obra�e�
        if (other.CompareTag("Monster"))
        {
            int damage = baseDamage;
            playerInventory?.NotifyOnHit(other.gameObject, ref damage);
            var monster = other.GetComponent<MonsterHealth>();
            if (monster!= null)
            {
                
                monster.TakeDamage(damage);
                OnHitSFX();


                //Debug.Log($"Projectile hit {other.name} for {damage} damage.");
            }

            var monsterRB = other.GetComponent<Rigidbody2D>();
            if (monsterRB != null)
            {   
                monster.GetComponent<MonsterStandardMove>().Stun(0.5f);
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                monsterRB.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        
        Destroy(gameObject);
    }

    private void OnHitSFX()
    {
        //if(!sfx && !hitClip) return;
        sfx.transform.SetParent(null);           // odczep, żeby żył po zniszczeniu pocisku
        sfx.pitch = Random.Range(0.8f, 1.2f);
        sfx.PlayOneShot(hitClip, volume);
        Destroy(sfx.gameObject, hitClip.length);

    }
}
