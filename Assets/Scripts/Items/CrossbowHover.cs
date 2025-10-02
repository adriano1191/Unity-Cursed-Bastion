using UnityEngine;
using UnityEngine.InputSystem; // nowy Input System

public class CrossbowHover : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;        // wska¿ Player (root)
    public Transform muzzle;        // dziecko "Muzzle"
    public Projectile boltPrefab;   // prefab be³tu
    [SerializeField] FindCloseTarget closeTarget;
    public Inventory playerInventory;
    public PlayerStats playerStats;

    [Header("Movement")]
    public float orbitRadius = 1.2f;   // promieñ „orbitowania” wokó³ gracza
    public float followSpeed = 12f;    // jak szybko dogania celow¹ pozycjê

    [Header("Shooting")]
    public float boltSpeed = 12f;
    public float baseFireRate = 0.4f;
    public float fireRate = 0.4f;

    private Camera cam;
    private float nextShot;

    public bool autoAim = true;
    public bool autoShot = false;

    [Header("SFX")]
    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip[] shotClips;
    [SerializeField, Range(0f, 1f)] float volume = 1f;

    public enum HandSide { Right, Left }

    [Header("Hand Arc")]
    [SerializeField] HandSide hand = HandSide.Right;      // prawa/lewa „d³oñ”
    [SerializeField, Range(10f, 180f)] float handArcDeg = 100f;

    private void Awake()
    {
        cam = Camera.main;
        if (!player) player = transform.parent; // zak³adamy, ¿e dziecko gracza
        if (!playerInventory) playerInventory = GetComponentInParent<Inventory>();
        if (!playerStats) playerStats = GetComponentInParent<PlayerStats>();
        if (!closeTarget) closeTarget = GetComponent<FindCloseTarget>();

    }

    private void Update()
    {
        if (!player || !muzzle) return;
        // kamera potrzebna tylko, gdy celujemy myszk¹ lub gdy autoAim bez targetu ma fallback do myszki
        if (!autoAim && !cam) return;

        fireRate = baseFireRate * playerStats.GetComponent<PlayerStats>().AttackSpeed;

        // --- 1) Wyznacz punkt celowania ---
        Vector3 aimPos;
        if (autoAim && closeTarget != null && closeTarget.CurrentTarget != null)
        {
             aimPos = closeTarget.CurrentTarget.position;

        }
        else
        {
            if (!cam) return; // brak kamery = nie mamy sk¹d wzi¹æ pozycji myszy
            aimPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        aimPos.z = 0f;



        // --- 2) Pozycja kuszy dooko³a gracza (jak by³o) ---
        Vector2 toAimFromPlayer = (aimPos - player.position);
        Vector2 dirFromPlayer = toAimFromPlayer.sqrMagnitude > 0.0001f ? toAimFromPlayer.normalized : Vector2.right;
        dirFromPlayer = ClampToHandArc(dirFromPlayer);
        Vector3 targetPos = player.position + (Vector3)(dirFromPlayer * orbitRadius);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);

        // --- 3) Obrót kuszy w kierunku celu ---
        Vector2 aimDir = (aimPos - transform.position).normalized;
        if (aimDir.sqrMagnitude < 0.0001f) aimDir = Vector2.right;
        transform.right = aimDir;

        if (!autoShot)
        {
            // 4) Strza³ – lewy przycisk myszy + cooldown
            if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextShot)
            {
                Shoot(aimDir);

                PlayShot();
            }
        }
        else
        {
            if (autoAim && !closeTarget.CurrentTarget)
            {
                return;
            }
            if (Time.time >= nextShot)
            {
                Shoot(aimDir);

                PlayShot();
            }
        }


    }

    private void Shoot(Vector2 aimDir)
    {
        var p = Instantiate(boltPrefab, muzzle.position, Quaternion.identity);
        p.Init(playerInventory);           // wa¿ne dla efektów OnHit z ekwipunku
        //p.Fire(aimDir, boltSpeed);

        nextShot = Time.time + fireRate;
        PlayShot();
    }

    public void PlayShot()
    {
        sfx.pitch = Random.Range(0.9f, 1.1f); // lekkie zró¿nicowanie
        var clip = shotClips[Random.Range(0, shotClips.Length)];
        sfx.PlayOneShot(clip, volume);
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
        if (!player) return;
        Vector2 center = (hand == HandSide.Right) ? (Vector2)player.right : (Vector2)(-player.right);
        float half = handArcDeg * 0.5f;
        Vector3 a = player.position + (Vector3)(Quaternion.Euler(0, 0, -half) * (Vector3)center) * orbitRadius;
        Vector3 b = player.position + (Vector3)(Quaternion.Euler(0, 0, half) * (Vector3)center) * orbitRadius;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(player.position, a);
        Gizmos.DrawLine(player.position, b);
        Gizmos.DrawWireSphere(player.position, orbitRadius);
    }



}
