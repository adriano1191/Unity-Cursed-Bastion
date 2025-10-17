using UnityEngine;
using UnityEngine.Rendering;
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

    
    public enum WeaponMainStat
    {
        [InspectorName("Siła")] Strength,
        [InspectorName("Zręczność")] Agility,
        [InspectorName("Inteligencja")] Intellect
    }
    [Tooltip("Main stat that scales this weapon's damage.")] [SerializeField] private WeaponMainStat mainStat = WeaponMainStat.Strength;

    [SerializeField] private int baseDamage = 10;
    public WeaponMainStat MainStat => mainStat;
    [SerializeField, Min(0f)] private float scalingDamage = 0.1f;
    [SerializeField] float flatBonusDamage = 0f;
    [SerializeField] float percentBonusDamage = 0f;



    [Tooltip("Base cooldown of this weapon in seconds per attack (e.g., 2.0s for weapon: one shot every 2 seconds)")]
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
    public float AttackSpeedFactor => attackSpeedFactor;

    /// <summary>
    /// Direction the weapon should aim at (world-space, normalized). Set by hover/aim systems.
    /// </summary>
    public Vector2 AimDir { get; set; }
    #endregion

    #region SFX
    [Header("SFX (optional)")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] [Range(0f, 1f)] private float attackClipVolume = 1f;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] [Range(0f, 1f)] private float hitClipVolume = 1f;

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
        if (!sfx) return;
        sfx.pitch = Random.Range(0.95f, 1.05f);
        //sfx.PlayOneShot(attackClips[Random.Range(0, attackClips.Length)], volume);  // random clip from array
        sfx.PlayOneShot(attackClip, attackClipVolume);
    }

    public void PlayHitSfx()
    {
        if (!sfx || !hitClip) return;
        sfx.pitch = Random.Range(0.95f, 1.05f);
        sfx.PlayOneShot(hitClip, hitClipVolume);
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

    float GetMainStat()
    {
        return mainStat switch
        {
            WeaponMainStat.Strength => playerStats.Strength,
            WeaponMainStat.Agility => playerStats.Agility,
            WeaponMainStat.Intellect => playerStats.Intellect,
            _ => 0f
        };
    }

    public int GetCurrentDamage()
    {
        float stat = GetMainStat();
        float multiplier = 1f + stat * scalingDamage + percentBonusDamage;
        float raw = (baseDamage + flatBonusDamage) * multiplier;
        return Mathf.RoundToInt(raw);
    }

}
