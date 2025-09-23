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

    private void Awake()
    {
        cam = Camera.main;
        playerInventory = GetComponentInParent<Inventory>();
        playerStats = GetComponentInParent<PlayerStats>();
        closeTarget = GetComponent<FindCloseTarget>();
    }

    private void Update()
    {
        if (!player || !muzzle) return;
        // kamera potrzebna tylko, gdy celujemy myszk¹ lub gdy autoAim bez targetu ma fallback do myszki
        if (!autoAim && !cam) return;

        fireRate = baseFireRate * playerStats.GetComponent<PlayerStats>().attackSpeed;

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
        p.Fire(aimDir, boltSpeed);

        nextShot = Time.time + fireRate;
        PlayShot();
    }

    public void PlayShot()
    {
        sfx.pitch = Random.Range(0.9f, 1.1f); // lekkie zró¿nicowanie
        var clip = shotClips[Random.Range(0, shotClips.Length)];
        sfx.PlayOneShot(clip, volume);
    }


}
