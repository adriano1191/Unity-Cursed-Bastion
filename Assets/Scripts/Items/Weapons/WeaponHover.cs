using UnityEngine;
using UnityEngine.InputSystem; // new Input System (Mouse)

public class WeaponHover : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Radius of the weapon orbit around the player")] public float orbitRadius = 0.8f;
    [Tooltip("How fast the weapon chases its target hover position")] public float followSpeed = 7f;

    [Header("Refs")]
    [SerializeField] private WeaponStats weaponStats;
    [SerializeField] private Transform player;
    [SerializeField] private FindCloseTarget closeTarget;

    [Tooltip("true = aim nearest enemy, false = aim by mouse")]
    [SerializeField]
    private bool autoAim = true; // local switch; don't rely on WeaponStats internals

    public enum HandSide { Right, Left }
    public enum HandMode { Anatomical, ScreenSideLocked }

    public enum FacingSource
    {
        SpriteRendererFlipX,  // use gfx SpriteRenderer.flipX
        GfxScaleX,            // use gfxRoot.lossyScale.x sign
        PlayerScaleX,         // use player.lossyScale.x sign
        PlayerRight           // use sign of Vector2.Dot(player.right, +X)
    }

    [Header("Hand / Arc")]
    [SerializeField] private HandSide hand = HandSide.Right;
    [SerializeField, Range(10f, 360f)] private float handArcDeg = 100f;
    [Tooltip("Anatomical: hand stays anatomical (flips with player). ScreenSideLocked: swaps Left/Right when player flips to keep weapon on the same screen side.")]
    [SerializeField] private HandMode handMode = HandMode.Anatomical;

    [Header("Facing (GFX)")]
    [Tooltip("SpriteRenderer used to detect facing via flipX (preferred when only GFX flips). Optional if you use another source.")]
    [SerializeField] private SpriteRenderer gfxRenderer;
    [Tooltip("Root of the visual GFX (if you flip/scale only this object)")]
    [SerializeField] private Transform gfxRoot;
    [Tooltip("Where to read facing from")]
    [SerializeField] private FacingSource facingFrom = FacingSource.SpriteRendererFlipX;

    [Header("Visual Flip (optional)")]
    [Tooltip("Optional sprite root to mirror visually when aim flips horizontally")]
    [SerializeField] private Transform spriteRoot;
    [SerializeField] private bool mirrorSpriteByAimX = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Vector3 spriteBaseScale;
    private int lastFacingSign = +1; // +1 facing right, -1 facing left
    private Camera cam;
    private WeaponThurst weaponThurst; // pause hover while thrusting

    private void Awake()
    {
        cam = Camera.main;
        if (!weaponStats) weaponStats = GetComponent<WeaponStats>();
        if (!player) player = transform.parent;
        if (!closeTarget) closeTarget = GetComponent<FindCloseTarget>();
        if (!weaponThurst) weaponThurst = GetComponent<WeaponThurst>();

        // Try find a SpriteRenderer for GFX if not set
        if (!gfxRenderer)
        {
            gfxRenderer = player ? player.GetComponentInChildren<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
            if (gfxRenderer && !gfxRoot) gfxRoot = gfxRenderer.transform;
        }

        if (!spriteRoot)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr) spriteRoot = sr.transform;
        }
        spriteBaseScale = spriteRoot ? spriteRoot.localScale : Vector3.one;

        lastFacingSign = GetFacingSign();
    }

    private void Update()
    {
        if (weaponThurst && weaponThurst.IsAttacking) return; // pause hover while thrusting
        SyncFacingAndHand();
        Hover();
    }

    private void Hover()
    {
        if (!player) return;

        // 1) Determine aim position (auto-aim target or mouse)
        Vector3 aimPos;
        bool hasAutoTarget = autoAim && closeTarget && closeTarget.CurrentTarget;
        if (hasAutoTarget)
        {
            aimPos = closeTarget.CurrentTarget.position;
        }
        else if (!autoAim)
        {
            if (!cam || Mouse.current == null) return; // can't aim without camera/mouse
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            float z = Mathf.Abs(cam.transform.position.z - player.position.z);
            aimPos = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, z));
        }
        else 
        { 
            return; // no target to auto-aim at
        }
        aimPos.z = player.position.z; // keep in 2D plane

        // 2) Orbit position constrained by hand arc
        Vector2 toAimFromPlayer = (Vector2)(aimPos - player.position);
        Vector2 dirFromPlayer = toAimFromPlayer.sqrMagnitude > 1e-6f ? toAimFromPlayer.normalized : Vector2.right;
        dirFromPlayer = ClampToHandArc(dirFromPlayer);
        Vector3 targetPos = player.position + (Vector3)(dirFromPlayer * orbitRadius);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);

        // 3) Rotate weapon to look at aim position
        Vector2 aimDir = (Vector2)(aimPos - transform.position);
        if (aimDir.sqrMagnitude < 1e-6f) aimDir = (hand == HandSide.Right) ? Vector2.right : Vector2.left;
        aimDir.Normalize();
        transform.right = aimDir;

        // 4) Publish aimDir to WeaponStats
        if (weaponStats) weaponStats.AimDir = aimDir;

        // 5) Optional sprite mirroring
        /*
        if (spriteRoot && mirrorSpriteByAimX)
        {
            float signX = Mathf.Sign(aimDir.x == 0f ? transform.right.x : aimDir.x);
            spriteRoot.localScale = new Vector3(Mathf.Abs(spriteBaseScale.x) * signX, spriteBaseScale.y, spriteBaseScale.z);
        }
   
        if (debugLogs)
            Debug.Log($"[WeaponHover] facingSign={lastFacingSign} hand={hand} autoTarget={(hasAutoTarget ? "yes" : "no")} aimDir={aimDir} pos={transform.position}");
         */
    }

    private Vector2 ClampToHandArc(Vector2 desiredDir)
    {
        if (desiredDir.sqrMagnitude < 1e-6f)
            return (hand == HandSide.Right) ? Vector2.right : Vector2.left;
        if (!player) return desiredDir.normalized;

        Vector2 center = (hand == HandSide.Right) ? (Vector2)player.right : (Vector2)(-player.right);
        if (center.sqrMagnitude < 1e-6f) center = (hand == HandSide.Right) ? Vector2.right : Vector2.left;

        float signed = Vector2.SignedAngle(center, desiredDir);
        float half = handArcDeg * 0.5f;
        signed = Mathf.Clamp(signed, -half, +half);
        Vector2 clamped = (Vector2)(Quaternion.Euler(0f, 0f, signed) * (Vector3)center);
        return clamped.normalized;
    }

    private void SyncFacingAndHand()
    {
        int sign = GetFacingSign();
        if (sign != lastFacingSign)
        {
            if (handMode == HandMode.ScreenSideLocked)
            {
                hand = (hand == HandSide.Right) ? HandSide.Left : HandSide.Right; // swap on flip
            }
            lastFacingSign = sign;
        }
        //Debug.Log($"[WeaponHover] SyncFacingAndHand: sign={sign}, hand={hand}, handMode={handMode}");
    }

    private int GetFacingSign()
    {
        // Return +1 when looking right (screen +X), -1 when looking left
        switch (facingFrom)
        {
            case FacingSource.SpriteRendererFlipX:
                if (!gfxRenderer)
                {
                    // try lazy acquire once
                    gfxRenderer = player ? player.GetComponentInChildren<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
                }
                if (gfxRenderer)
                    return gfxRenderer.flipX ? -1 : +1;
                // fallback to player scale if no renderer
                goto case FacingSource.PlayerScaleX;

            case FacingSource.GfxScaleX:
                if (gfxRoot)
                    return (gfxRoot.lossyScale.x >= 0f) ? +1 : -1;
                // fallback
                goto case FacingSource.PlayerScaleX;

            case FacingSource.PlayerScaleX:
                if (player)
                    return (player.lossyScale.x >= 0f) ? +1 : -1;
                return +1;

            case FacingSource.PlayerRight:
            default:
                if (player)
                    return (Vector2.Dot(player.right, Vector2.right) >= 0f) ? +1 : -1;
                return +1;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!player) return;
        Vector2 center = (hand == HandSide.Right) ? (Vector2)player.right : (Vector2)(-player.right);
        if (center.sqrMagnitude < 1e-6f) center = (hand == HandSide.Right) ? Vector2.right : Vector2.left;
        float half = handArcDeg * 0.5f;

        Vector3 a = player.position + (Vector3)(Quaternion.Euler(0, 0, -half) * (Vector3)center) * orbitRadius;
        Vector3 b = player.position + (Vector3)(Quaternion.Euler(0, 0, +half) * (Vector3)center) * orbitRadius;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(player.position, a);
        Gizmos.DrawLine(player.position, b);
        Gizmos.DrawWireSphere(player.position, orbitRadius);
    }
}
