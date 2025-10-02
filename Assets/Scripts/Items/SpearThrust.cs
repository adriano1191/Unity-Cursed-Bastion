using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpearThrust : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;                    // root gracza
    public Transform tip;                       // czubek w³óczni (child)
    [SerializeField] FindCloseTarget closeTarget;
    public Inventory playerInventory;
    public PlayerStats playerStats;

    [Header("Movement (jak w kuszy)")]
    public float orbitRadius = 1.1f;
    public float followSpeed = 14f;

    [Header("Aim & Fire")]
    public bool autoAim = true;                 // true = w najbli¿szego wroga, false = w mysz
    public bool autoAttack = true;              // auto-pchniêcia co cooldown
    public float baseFireRate = 0.5f;           // sekundy miêdzy pchniêciami
    public float fireRate = 0.5f;

    [Header("Attack params")]
    public int damage = 30;
    public float thrustDistance = 1.4f;         // jak daleko wysuwa siê w³ócznia
    public float thrustTime = 0.08f;            // czas wysuwu
    public float retractTime = 0.12f;           // czas powrotu
    public float hitRadius = 0.25f;             // promieñ trafienia na czubku
    public LayerMask targetMask;                // warstwa przeciwników do trafiania
    public float knockback = 4f;

    [Header("SFX (opcjonalnie)")]
    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip[] swingClips;
    [SerializeField] AudioClip hitClip;
    [SerializeField, Range(0, 1)] float volume = 1f;

    public enum HandSide { Right, Left }

    [Header("Hand Arc")]
    [SerializeField] HandSide hand = HandSide.Right;      // prawa/lewa „d³oñ”
    [SerializeField, Range(10f, 180f)] float handArcDeg = 100f;

    Camera cam;
    float nextAttack;
    bool attacking;

    void Awake()
    {
        cam = Camera.main;
        if (!player) player = transform.parent; // zak³adamy, ¿e dziecko gracza
        if (!playerInventory) playerInventory = GetComponentInParent<Inventory>();
        if (!playerStats) playerStats = GetComponentInParent<PlayerStats>();
        if (!closeTarget) closeTarget = GetComponent<FindCloseTarget>();
        if (!sfx) sfx = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!player || !tip) return;

        fireRate = baseFireRate * (playerStats ? playerStats.AttackSpeed : 1f);

        // --- 1) wyznacz punkt celowania (wróg lub mysz) ---
        Vector3 aimPos;
        if (autoAim && closeTarget && closeTarget.CurrentTarget)
            aimPos = closeTarget.CurrentTarget.position;
        else
        {
            if (!cam) cam = Camera.main;
            if (!cam) return;
            aimPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        aimPos.z = 0f;

        // --- 2) pozycja w³óczni wokó³ gracza ---
        Vector2 toAim = (aimPos - player.position);
        Vector2 dirFromPlayer = toAim.sqrMagnitude > 1e-6f ? toAim.normalized : Vector2.right;
        dirFromPlayer = ClampToHandArc(dirFromPlayer);
        Vector3 targetPos = player.position + (Vector3)(dirFromPlayer * orbitRadius);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);

        // --- 3) obrót w³óczni: przód (+X) w kierunku celu ---
        Vector2 aimDir = (aimPos - transform.position).normalized;
        if (aimDir.sqrMagnitude < 1e-6f) aimDir = Vector2.right;
        transform.right = aimDir;

        // --- 4) atak: auto lub na klik ---
        bool wantAttack = autoAttack
            ? (Time.time >= nextAttack)
            : (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttack);

        if (autoAim && !closeTarget.CurrentTarget)
        {
            return;
        }
        if (wantAttack && !attacking)
            StartCoroutine(ThrustRoutine(aimDir));
    }

    IEnumerator ThrustRoutine(Vector2 aimDir)
    {
        attacking = true;
        nextAttack = Time.time + fireRate;

        // SFX swing
        if (swingClips != null && swingClips.Length > 0 && sfx)
        {
            sfx.pitch = Random.Range(0.96f, 1.04f);
            sfx.PlayOneShot(swingClips[Random.Range(0, swingClips.Length)], volume);
        }

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(aimDir.normalized * thrustDistance);

        var alreadyHit = new HashSet<Collider2D>();

        // faza wysuwu
        float t = 0f;
        while (t < thrustTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / thrustTime);
            transform.position = Vector3.Lerp(start, end, u);
            DoTipHits(alreadyHit, aimDir);
            yield return null;
        }

        // krótka „aktywna” ramka na koñcu (opcjonalnie)
        DoTipHits(alreadyHit, aimDir);

        // faza powrotu
        t = 0f;
        while (t < retractTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / retractTime);
            transform.position = Vector3.Lerp(end, start, u);
            DoTipHits(alreadyHit, aimDir);
            yield return null;
        }

        attacking = false;
    }

    void DoTipHits(HashSet<Collider2D> alreadyHit, Vector2 aimDir)
    {
        var hits = Physics2D.OverlapCircleAll(tip.position, hitRadius, targetMask);
        foreach (var h in hits)
        {
            if (!h || alreadyHit.Contains(h)) continue;
            alreadyHit.Add(h);

            int finalDmg = damage;
            playerInventory?.NotifyOnHit(h.gameObject, ref finalDmg);

            var hp = h.GetComponent<MonsterHealth>();
            if (hp != null)
            {
                bool killed = hp.TakeDamage(finalDmg);
                if (killed) playerInventory?.NotifyOnKill(h.gameObject);
            }

            var rb = h.attachedRigidbody;
            if (rb) rb.AddForce(aimDir.normalized * knockback, ForceMode2D.Impulse);

            // hit SFX
            if (hitClip) AudioSource.PlayClipAtPoint(hitClip, tip.position, volume);
        }
    }
    Vector2 ClampToHandArc(Vector2 desiredDir)
    {
        if (desiredDir.sqrMagnitude < 1e-6f) return (hand == HandSide.Right) ? Vector2.right : Vector2.left;

        // kierunek centralny ³uku: +X lokalny gracza (prawa) lub -X (lewa)
        Vector2 center = (hand == HandSide.Right) ? (Vector2)player.right : (Vector2)(-player.right);

        // k¹t miêdzy „œrodkiem” a po¿¹danym kierunkiem
        float signed = Vector2.SignedAngle(center, desiredDir);
        float half = handArcDeg * 0.5f;

        // przytnij do [-half, +half]
        signed = Mathf.Clamp(signed, -half, +half);

        // obróæ wektor œrodkowy o przyciêty k¹t
        Vector2 clamped = (Vector2)(Quaternion.Euler(0f, 0f, signed) * (Vector3)center);
        return clamped.normalized;
    }
    void OnDrawGizmosSelected()
    {
        if (tip)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(tip.position, hitRadius);
        }
    }
}
