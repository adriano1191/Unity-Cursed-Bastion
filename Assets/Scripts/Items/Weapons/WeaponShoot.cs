using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShoot : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 3f; // in seconds of flight time
    [SerializeField] private int projectileDamage = 25;
    [SerializeField] private float projectileKnockback = 2f;
    [Header("Piercing")]
    [Tooltip("How many enemies this projectile can pierce through before being destroyed. 0 = destroy on first hit.")]
    [Min(0)] public int pierce = 0;



    [Header("Targeting")]
    [SerializeField] private FindCloseTarget closeTarget;
    [Tooltip("true = auto-aims nearest enemy, false = uses mouse / external AimDir")]
    [SerializeField] private bool autoAim = true;

    [Header("Firing")]
    [Tooltip("true = fires automatically on cooldown; false = requires input (LMB by default)")]
    [SerializeField] private bool autoAttack = true;

    [Header("Refs")]
    [SerializeField] private WeaponStats weaponStats;
    [SerializeField] private Inventory playerInventory;

    private Camera cam;
    private float nextShotTime;

    private void Awake()
    {
        cam = Camera.main;
        if (!weaponStats) weaponStats = GetComponent<WeaponStats>();
        if (!playerInventory) playerInventory = GetComponentInParent<Inventory>();
        if (!closeTarget) closeTarget = GetComponent<FindCloseTarget>();
        if (!muzzle)
        {
            var t = transform.Find("Muzzle");
            if (t) muzzle = t; else Debug.LogWarning("WeaponShoot: 'muzzle' not assigned.");
        }

        if (!projectilePrefab)
            Debug.LogWarning("WeaponShoot: projectilePrefab not assigned.");
    }

    private void Update()
    {
        if (!weaponStats || !muzzle || !projectilePrefab) return;

        // Determine aim direction
        Vector2 aimDir = GetAimDirection();
        if (aimDir.sqrMagnitude < 1e-6f) return;

        // Firing gate by cooldown
        float cooldown = weaponStats.AttackCooldownSeconds; // AttackSpeed is attacks/second
        if (Time.time < nextShotTime) return;

        // Auto fire or manual input
        bool wantsToFire = autoAttack || IsFirePressed();

        // If auto-aim is on but we have no auto target, and you require auto-aim strictly, you can early return
        if (autoAim && (closeTarget == null || closeTarget.CurrentTarget == null) && !IsFirePressed())
            return;

        if (wantsToFire)
        {
            Shoot(aimDir);
            weaponStats.PlayAttackSfx();
            nextShotTime = Time.time + cooldown;
        }
    }

    private Vector2 GetAimDirection()
    {
        // 1) Auto-aim at closest target if available
        if (autoAim && closeTarget && closeTarget.CurrentTarget)
        {
            Vector3 delta = closeTarget.CurrentTarget.position - muzzle.position;
            delta.z = 0f;
            if (delta.sqrMagnitude > 1e-6f) return ((Vector2)delta).normalized;
        }

        // 2) Use WeaponStats-provided AimDir (fed by WeaponHover)
        if (weaponStats != null)
        {
            Vector2 dir = weaponStats.AimDir;
            if (dir.sqrMagnitude > 1e-6f) return dir.normalized;
        }

        // 3) Fallback to mouse position (if Input System and camera exist)
        if (cam && Mouse.current != null)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
            Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, z));
            Vector3 delta = mouseWorld - muzzle.position; delta.z = 0f;
            if (delta.sqrMagnitude > 1e-6f) return ((Vector2)delta).normalized;
        }

        // 4) Final fallback
        return Vector2.right;
    }

    private bool IsFirePressed()
    {
        // Left Mouse Button (Input System). Extend here for gamepad/keyboard bindings if needed.
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
    }

    private void Shoot(Vector2 aimDir)
    {
        var p = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        if (playerInventory) p.Init(playerInventory);
        p.Fire(aimDir, projectileSpeed, projectileLifeTime, projectileDamage, projectileKnockback, pierce);
    }
}
