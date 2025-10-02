using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Stats")]
    private int damageProjectile = 25;
    private float knockbackForce = 2f;

    [Header("Piercing")]
    [Tooltip("How many enemies this projectile can pierce through before being destroyed. 0 = destroy on first hit.")]
    [Min(0)] public int pierceCount = 0;

    [Header("SFX (optional)")]
    public AudioSource sfx;              // optional, used for detachable one‑shot
    public AudioClip hitClip;            // hit sound
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private Rigidbody2D rb;
    private Inventory playerInventory;
    private bool hasHit;
    private int hitsSoFar;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Inventory ownerInv) => playerInventory = ownerInv;

    /// <summary>
    /// Fires the projectile in the given direction with specified speed, range, damage, and knockback.
    /// </summary>
    public void Fire(Vector2 dir, float speed, float lifeTime, int damage, float knockback, int pierce)
    {
        damageProjectile = damage;
        knockbackForce = knockback;
        pierceCount = pierce;
        // Rigidbody2D uses 'velocity' (not linearVelocity)
        rb.linearVelocity = dir.normalized * speed;

        // rotate sprite in flight direction
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > 1e-6f)
        {
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (hasHit) return;               // prevent multi‑hits on stacked colliders

        // Optional: ignore non‑monsters fast
        if (!other.CompareTag("Monster"))
        {
            Destroy(gameObject);
            return;
        }

        //hasHit = true;

        playerInventory?.NotifyOnHit(other.gameObject, ref damageProjectile);

        var hp = other.GetComponent<MonsterHealth>();
        if (hp != null)
        {
            bool killed = hp.TakeDamage(damageProjectile);
            if (killed) playerInventory?.NotifyOnKill(other.gameObject);
        }

        // Knockback (use projectile flight dir if available)
        var targetRb = other.attachedRigidbody;
        if (targetRb != null)
        {
            Vector2 dir = (rb.linearVelocity.sqrMagnitude > 1e-6f)
                ? rb.linearVelocity.normalized
                : (Vector2)(other.transform.position - transform.position).normalized;

            targetRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

            // Optional: try stun if component exists
            var mover = other.GetComponent<MonsterStandardMove>();
            if (mover != null)
            {
                mover.Stun(0.1f);
            }
        }

        PlayHitSfx();

        // --- Piercing logic ---
        if (hitsSoFar < pierceCount)
        {
            hitsSoFar++;
            // projectile keeps flying, don't destroy yet
        }
        else
        {
            // Disable physics immediately to avoid extra contacts before destroy
            var col = GetComponent<Collider2D>();
            if (col) col.enabled = false;
            if (rb) rb.simulated = false;

            Destroy(gameObject);
        }


    }

    private void PlayHitSfx()
    {
        if (hitClip == null) return;

        // Preferred: detach provided AudioSource so it can finish after projectile is destroyed
        if (sfx != null)
        {
            sfx.transform.SetParent(null);
            sfx.transform.position = transform.position;
            sfx.pitch = Random.Range(0.9f, 1.1f);
            sfx.PlayOneShot(hitClip, volume);
            Destroy(sfx.gameObject, hitClip.length + 0.05f);
            return;
        }

        // Fallback: fire‑and‑forget at point
        AudioSource.PlayClipAtPoint(hitClip, transform.position, volume);
    }
}
