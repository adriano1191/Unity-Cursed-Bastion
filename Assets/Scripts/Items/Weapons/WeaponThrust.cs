using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Generic thrust controller for stabbing weapons (spear, rapier, dagger, etc.).
/// Integrates with WeaponStats (seconds-per-shot × factor) and WeaponHover (AimDir publish).
/// Root object stays in orbit (WeaponHover); this script animates a child (bladeRoot) forward & back.
/// Tip hits are checked via Physics2D.OverlapCircle at tipTransform.
/// </summary>
[DisallowMultipleComponent]
public class WeaponThrust : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Weapon stats provider (cooldown, AimDir, SFX)")]
    [SerializeField] private WeaponStats weaponStats;
    [Tooltip("Optional: nearest-target provider for strict auto-aim gating")]
    [SerializeField] private FindCloseTarget closeTarget;

    [Header("Geometry")]
    [Tooltip("Child transform that moves forward/back during thrust (visual + muzzle/tip parent)")]
    [SerializeField] private Transform bladeRoot;            // moves along aim
    [Tooltip("Tip point where hit-check circle is evaluated")]
    [SerializeField] private Transform tipTransform;         // used for hit tests

    [Header("Thrust Timing/Shape")]
    [Tooltip("How far the bladeRoot moves forward along AimDir (world units)")]
    [Min(0.01f)][SerializeField] private float thrustDistance = 1.25f;
    [Tooltip("Forward time (seconds)")][Min(0.01f)][SerializeField] private float thrustTime = 0.08f;
    [Tooltip("Return time (seconds)")][Min(0.01f)][SerializeField] private float retractTime = 0.12f;
    [Tooltip("Pause at max extension (seconds)")][Min(0f)][SerializeField] private float holdTime = 0.0f;

    [Header("Combat")]
    [SerializeField] private int baseDamage = 30;
    [SerializeField] private float hitRadius = 0.25f;
    [SerializeField] private LayerMask targetMask; // enemies layers
    [SerializeField] private float knockback = 4f;
    [Tooltip("If true, allow hitting during retract phase as well")][SerializeField] private bool hitOnRetract = false;

    [Header("Behaviour")]
    [Tooltip("If true, attack automatically on cooldown. If false, requires LMB input")]
    [SerializeField] private bool autoAttack = true;
    [Tooltip("If true and no auto target, auto fire is blocked unless input is pressed")]
    [SerializeField] private bool requireTargetForAuto = true;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    // runtime
    private Vector3 bladeLocalStart;
    private bool attacking;
    public bool IsAttacking => attacking;
    private float nextAttackTime;
    private Coroutine thrustCo;
    private Camera cam;

    // external (optional)
    [SerializeField] private Inventory playerInventory;

    private void Awake()
    {
        cam = Camera.main;
        if (!weaponStats) weaponStats = GetComponent<WeaponStats>();
        if (!playerInventory) playerInventory = GetComponentInParent<Inventory>();
        if (!closeTarget) closeTarget = GetComponent<FindCloseTarget>();
        if (!bladeRoot) bladeRoot = transform; // fallback: move self
        if (!tipTransform) tipTransform = bladeRoot; // fallback: tip at bladeRoot

        bladeLocalStart = bladeRoot.localPosition;
    }

    private void OnDisable()
    {
        if (thrustCo != null)
        {
            StopCoroutine(thrustCo);
            thrustCo = null;
        }
        attacking = false;
        bladeRoot.localPosition = bladeLocalStart;
    }

    private void Update()
    {
        if (!weaponStats || !bladeRoot || !tipTransform) return;

        // Optional gating: no auto fire without target
        if (autoAttack && requireTargetForAuto && closeTarget && closeTarget.CurrentTarget == null && !IsFireHeld())
            return;

        float desiredCooldown = weaponStats.AttackCooldownSeconds;
        float animTime = thrustTime + holdTime + retractTime + 0.02f; // small buffer
        float cooldown = Mathf.Max(desiredCooldown, animTime);

        bool wantsAttack = autoAttack || IsFirePressed();

        if (!attacking && wantsAttack && Time.time >= nextAttackTime)
        {
            Vector2 aimDir = ResolveAimDir();
            if (aimDir.sqrMagnitude < 1e-6f) aimDir = Vector2.right;
            aimDir.Normalize();
            thrustCo = StartCoroutine(ThrustRoutine(aimDir, cooldown));
        }
    }

    private bool IsFirePressed() => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    private bool IsFireHeld() => Mouse.current != null && Mouse.current.leftButton.isPressed;

    private Vector2 ResolveAimDir()
    {
        // Prefer WeaponStats.AimDir (published by WeaponHover)
        if (weaponStats != null)
        {
            Vector2 dir = weaponStats.AimDir;
            if (dir.sqrMagnitude > 1e-6f) return dir.normalized;
        }

        // Fallback to close target direction
        if (closeTarget && closeTarget.CurrentTarget)
        {
            Vector3 d = closeTarget.CurrentTarget.position - transform.position; d.z = 0f;
            if (d.sqrMagnitude > 1e-6f) return ((Vector2)d).normalized;
        }

        // Mouse fallback
        if (cam && Mouse.current != null)
        {
            Vector2 m = Mouse.current.position.ReadValue();
            float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, z));
            Vector3 d = w - transform.position; d.z = 0f;
            if (d.sqrMagnitude > 1e-6f) return ((Vector2)d).normalized;
        }

        return Vector2.right;
    }

    private IEnumerator ThrustRoutine(Vector2 aimDir, float cooldown)
    {
        attacking = true;
        nextAttackTime = Time.time + cooldown; // lock ASAP to avoid reentry

        // Cache start & end in LOCAL space along aim
        Vector3 localForward = WorldDirToLocal(bladeRoot, aimDir) * thrustDistance;
        Vector3 localStart = bladeLocalStart;
        Vector3 localEnd = localStart + localForward;

        var alreadyHit = new HashSet<Collider2D>();

        // Forward
        float t = 0f;
        while (t < thrustTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / thrustTime);
            bladeRoot.localPosition = Vector3.Lerp(localStart, localEnd, EaseOutCubic(u));
            DoTipHits(alreadyHit, aimDir);
            yield return null;
        }
        bladeRoot.localPosition = localEnd;

        // Hold at tip (optional)
        if (holdTime > 0f)
        {
            float ht = 0f;
            while (ht < holdTime)
            {
                ht += Time.deltaTime;
                DoTipHits(alreadyHit, aimDir);
                yield return null;
            }
        }

        // Retract
        t = 0f;
        while (t < retractTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / retractTime);
            bladeRoot.localPosition = Vector3.Lerp(localEnd, localStart, EaseInCubic(u));
            if (hitOnRetract) DoTipHits(alreadyHit, aimDir);
            yield return null;
        }
        bladeRoot.localPosition = localStart;

        attacking = false;
        thrustCo = null;
    }

    private void DoTipHits(HashSet<Collider2D> alreadyHit, Vector2 aimDir)
    {
        var hits = Physics2D.OverlapCircleAll(tipTransform.position, hitRadius, targetMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h || alreadyHit.Contains(h)) continue;
            alreadyHit.Add(h);

            int dmg = baseDamage;
            playerInventory?.NotifyOnHit(h.gameObject, ref dmg);

            var hp = h.GetComponent<MonsterHealth>();
            if (hp != null)
            {
                bool killed = hp.TakeDamage(dmg);
                if (killed) playerInventory?.NotifyOnKill(h.gameObject);
            }

            var rb = h.attachedRigidbody;
            if (rb)
            {
                rb.AddForce(aimDir * knockback, ForceMode2D.Impulse);
                // Optional: try stun if component exists

                var mover = h.GetComponent<MonsterStandardMove>();
                if (mover != null)
                {
                    mover.Stun(0.1f);
                }

            }

        }
    }

    private static Vector3 WorldDirToLocal(Transform reference, Vector2 worldDir)
    {
        // Convert world direction into reference's local space (ignore scale skew)
        Vector3 worldEnd = reference.position + (Vector3)worldDir;
        Vector3 localEnd = reference.parent ? reference.parent.InverseTransformPoint(worldEnd) : (Vector3)worldDir;
        Vector3 localStart = reference.parent ? reference.parent.InverseTransformPoint(reference.position) : Vector3.zero;
        return (localEnd - localStart).normalized;
    }

    private static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
    private static float EaseInCubic(float x) => x * x * x;

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        if (!tipTransform) tipTransform = bladeRoot ? bladeRoot : transform;
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(tipTransform.position, hitRadius);

        // Preview of reach along current AimDir
        if (weaponStats != null)
        {
            Vector2 d = (weaponStats.AimDir.sqrMagnitude > 1e-6f) ? weaponStats.AimDir.normalized : Vector2.right;
            Vector3 start = (bladeRoot ? bladeRoot : transform).position;
            Vector3 end = start + (Vector3)d * thrustDistance;
            Gizmos.DrawLine(start, end);
        }
    }
}
