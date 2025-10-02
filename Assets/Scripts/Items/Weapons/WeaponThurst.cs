using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // for Mouse.current

public class WeaponThurst : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FindCloseTarget closeTarget;
    [SerializeField] private Transform muzzle; // tip of the weapon
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private WeaponStats weaponStats;

    [Header("Behavior")]
    [Tooltip("true = keeps attacking on cooldown; false = requires input")]
    [SerializeField] private bool autoAttack = true;
    [Tooltip("true = require an auto-aim target to attack; false = allow free thrust towards aim dir/mouse")]
    [SerializeField] private bool requireAutoTargetWhenAutoAim = true;
    [SerializeField] private bool autoAim = true;

    [Header("Attack params")]
    [SerializeField] private int damage = 30;
    [SerializeField] private float thrustDistance = 1.4f;   // forward travel
    [SerializeField] private float thrustTime = 0.08f;      // time forward
    [SerializeField] private float retractTime = 0.12f;     // time back
    [SerializeField] private float hitRadius = 0.25f;       // circle at tip
    [SerializeField] private LayerMask targetMask;          // enemies
    [SerializeField] private float knockback = 4f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool debugLogs = false;

    private float nextAttackTime;
    private bool attacking;
    public bool IsAttacking => attacking;
    private Coroutine thrustCo;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        if (!playerInventory) playerInventory = GetComponentInParent<Inventory>();
        if (!weaponStats) weaponStats = GetComponent<WeaponStats>();
        if (!closeTarget) closeTarget = GetComponent<FindCloseTarget>();
        if (!muzzle)
        {
            var t = transform.Find("Muzzle");
            if (t) muzzle = t; else Debug.LogWarning("WeaponThurst: 'muzzle' not assigned.");
        }
    }

    private void OnDisable()
    {
        if (thrustCo != null)
        {
            StopCoroutine(thrustCo);
            thrustCo = null;
        }
        attacking = false;
    }

    private void Update()
    {
        if (!weaponStats || !muzzle) return;

        // mirror flags locally if you want (optional)
        // autoAim = weaponStats.AutoAimFlag; // if you later expose this on WeaponStats

        // If auto-aim is enabled and we require a target, bail if none
        if (autoAim && requireAutoTargetWhenAutoAim && (closeTarget == null || closeTarget.CurrentTarget == null) && !IsFirePressed())
            return;

        float cooldown = Mathf.Max(1f / weaponStats.AttackSpeed, thrustTime + retractTime + 0.02f);
        bool wantsAttack = autoAttack || IsFirePressed();

        if (wantsAttack && !attacking && Time.time >= nextAttackTime)
        {
            Vector2 aimDir = GetAimDirection();
            if (aimDir.sqrMagnitude < 1e-6f) aimDir = Vector2.right;
            aimDir.Normalize();

            thrustCo = StartCoroutine(ThrustRoutine(aimDir, cooldown));
        }
    }

    private bool IsFirePressed()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private Vector2 GetAimDirection()
    {
        // 1) Auto-aim to closest target (if desired & available)
        if (autoAim && closeTarget && closeTarget.CurrentTarget)
        {
            Vector3 delta = closeTarget.CurrentTarget.position - muzzle.position; delta.z = 0f;
            if (delta.sqrMagnitude > 1e-6f) return ((Vector2)delta).normalized;
        }

        // 2) Use WeaponStats-provided direction (from WeaponHover)
        if (weaponStats != null)
        {
            Vector2 dir = weaponStats.AimDir;
            if (dir.sqrMagnitude > 1e-6f) return dir.normalized;
        }

        // 3) Fallback to mouse (if camera exists)
        if (cam && Mouse.current != null)
        {
            Vector2 m = Mouse.current.position.ReadValue();
            float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, z));
            Vector3 delta = world - muzzle.position; delta.z = 0f;
            if (delta.sqrMagnitude > 1e-6f) return ((Vector2)delta).normalized;
        }

        return Vector2.right;
    }

    private IEnumerator ThrustRoutine(Vector2 aimDir, float cooldown)
    {
        attacking = true;
        nextAttackTime = Time.time + cooldown; // lock immediately to avoid double-fires

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(aimDir * thrustDistance);

        var alreadyHit = new HashSet<Collider2D>();

        // FORWARD
        float t = 0f;
        while (t < thrustTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / thrustTime);
            transform.position = Vector3.Lerp(start, end, u);

            if (DoTipHits(alreadyHit, aimDir))
            {
                if (debugLogs) Debug.Log("WeaponThurst: hit during forward phase -> retract");
                break; // proceed to retract immediately after first contact
            }
            yield return null;
        }

        // Optional final hit check at tip
        DoTipHits(alreadyHit, aimDir);

        // BACK
        t = 0f;
        while (t < retractTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / retractTime);
            transform.position = Vector3.Lerp(end, start, u);
            yield return null;
        }

        transform.position = start; // ensure exact reset
        attacking = false;
        thrustCo = null;
    }

    private bool DoTipHits(HashSet<Collider2D> alreadyHit, Vector2 aimDir)
    {
        if (!muzzle) return false;
        bool hitSomeone = false;

        var hits = Physics2D.OverlapCircleAll(muzzle.position, hitRadius, targetMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h || alreadyHit.Contains(h)) continue;
            alreadyHit.Add(h);
            hitSomeone = true;

            int finalDmg = damage;
            playerInventory?.NotifyOnHit(h.gameObject, ref finalDmg);

            var hp = h.GetComponent<MonsterHealth>();
            if (hp != null)
            {
                bool killed = hp.TakeDamage(finalDmg);
                if (killed) playerInventory?.NotifyOnKill(h.gameObject);
            }

            var rb = h.attachedRigidbody;
            if (rb) rb.AddForce(aimDir * knockback, ForceMode2D.Impulse);

            // SFX could be triggered via WeaponStats.PlayHitSfx() if desired
        }
        return hitSomeone;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        if (!muzzle) return;

        // Current aim preview
        Vector2 dir = Vector2.right;
#if UNITY_EDITOR
        if (Application.isPlaying && weaponStats != null)
            dir = (weaponStats.AimDir.sqrMagnitude > 1e-6f) ? weaponStats.AimDir.normalized : Vector2.right;
#endif
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(dir * thrustDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.05f);

        // Hit circle at muzzle
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(muzzle.position, hitRadius);
    }
}
