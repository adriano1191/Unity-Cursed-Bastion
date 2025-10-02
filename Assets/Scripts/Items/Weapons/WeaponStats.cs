using UnityEngine;
using UnityEngine.Serialization; // for FormerlySerializedAs

/// <summary>
/// Holds weapon-related runtime settings (aim/attack) and reacts to PlayerStats.AttackSpeed changes.
/// Now uses seconds-per-shot × factor model:
///   finalCooldown = baseCooldownSeconds * clamp(attackSpeedFactor, minFactor..∞)
/// Backward compatible: AttackSpeed (APS) = 1f / AttackCooldownSeconds.
/// </summary>
public class WeaponStats : MonoBehaviour
{
    #region Refs
    [Header("Refs")]
    [Tooltip("Root Player transform (will fallback to parent if not set)")][SerializeField] private Transform player;
    [SerializeField] private FindCloseTarget closeTarget;
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private PlayerStats playerStats;
    #endregion

    #region Aim & Fire
    [Header("Aim & Fire")]
    [Tooltip("true = auto-aims nearest enemy, false = aim at mouse")][SerializeField] private bool autoAim = true;
    [SerializeField] private bool autoAttack = true;

    [Tooltip("Base cooldown of this weapon in seconds per attack (e.g., 2.0s for crossbow: one shot every 2 seconds)")]
    [Min(0.01f)]
    [FormerlySerializedAs("baseWeaponAttackSpeed")] // migrate old serialized field
    [SerializeField] private float baseAttackCooldownSeconds = 2.0f; // seconds/attack (SPA)

    [Tooltip("Global attack speed factor from PlayerStats/items. 1.0 = no change; 0.5 = 2x faster (halves cooldown); 2.0 = 2x slower.")]
    [FormerlySerializedAs("attackSpeedModifier")] // migrate old serialized field
    [SerializeField] private float attackSpeedFactor = 1.0f;

    [Tooltip("Minimum allowed factor (max acceleration). Example: 0.2 => cooldown won't go below 20% of base.")]
    [Range(0.05f, 1f)][SerializeField] private float minFactor = 0.2f;

    /// <summary>
    /// Final seconds per attack after factor & clamp.
    /// </summary>
    public float AttackCooldownSeconds => Mathf.Max(0.001f, baseAttackCooldownSeconds * Mathf.Max(minFactor, attackSpeedFactor));

    /// <summary>
    /// Backward-compat convenience: attacks per second derived from cooldown.
    /// </summary>
    public float AttackSpeed => 1f / AttackCooldownSeconds;

    /// <summary>
    /// Direction the weapon should aim at (world-space, normalized). Set by hover/aim systems.
    /// </summary>
    public Vector2 AimDir { get; set; }
    #endregion

    #region SFX
    [Header("SFX (optional)")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private AudioClip hitClip;
    [Range(0, 1)][SerializeField] private float volume = 1f;
    #endregion

    private Camera cam;

    #region Unity
    private void Awake()
    {
        cam = Camera.main;
        if (!player) player = transform.parent; // assume child of player
        if (!playerInventory) playerInventory = GetComponentInParent<Inventory>();
        if (!playerStats) playerStats = GetComponentInParent<PlayerStats>();
        if (!closeTarget) closeTarget = GetComponent<FindCloseTarget>();
        if (!sfx) sfx = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.attackSpeedChange += OnAttackSpeedChanged;
            // Initialize from current stats value once on enable
            OnAttackSpeedChanged(playerStats.AttackSpeed, playerStats.AttackSpeed);
        }
        else
        {
            Debug.LogWarning("WeaponStats: playerStats reference missing on OnEnable(). Attack speed factor will stay at default 1.0.");
            attackSpeedFactor = 1f; // safe default
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.attackSpeedChange -= OnAttackSpeedChanged;
    }

    // private void Update() { /* optional debug */ }
    #endregion

    #region API
    /// <summary>
    /// Set weapon's intrinsic cadence in seconds per shot.
    /// </summary>
    public void SetBaseAttackCooldown(float secondsPerShot)
    {
        baseAttackCooldownSeconds = Mathf.Max(0.01f, secondsPerShot);
    }

    public void PlayAttackSfx()
    {
        if (!sfx || attackClips == null || attackClips.Length == 0) return;
        sfx.pitch = Random.Range(0.95f, 1.05f);
        sfx.PlayOneShot(attackClips[Random.Range(0, attackClips.Length)], volume);
    }

    public void PlayHitSfx()
    {
        if (!sfx || !hitClip) return;
        sfx.pitch = 1f;
        sfx.PlayOneShot(hitClip, volume);
    }
    #endregion

    #region Handlers
    private void OnAttackSpeedChanged(float oldValue, float newValue)
    {
        // Interpret PlayerStats.AttackSpeed as a FACTOR applied to time.
        // 1.0 = unchanged, 0.5 = 2x faster (halves cooldown), 2.0 = 2x slower.
        attackSpeedFactor = Mathf.Max(minFactor, newValue);
    }
    #endregion
}
