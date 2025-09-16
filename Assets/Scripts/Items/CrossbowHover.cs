using UnityEngine;
using UnityEngine.InputSystem; // nowy Input System

public class CrossbowHover : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;        // wska¿ Player (root)
    public Transform muzzle;        // dziecko "Muzzle"
    public Projectile boltPrefab;   // prefab be³tu
    public Inventory playerInventory;

    [Header("Movement")]
    public float orbitRadius = 1.2f;   // promieñ „orbitowania” wokó³ gracza
    public float followSpeed = 12f;    // jak szybko dogania celow¹ pozycjê

    [Header("Shooting")]
    public float boltSpeed = 12f;
    public float fireRate = 0.4f;

    private Camera cam;
    private float nextShot;

    public bool autoShot = false;

    [Header("SFX")]
    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip[] shotClips;
    [SerializeField, Range(0f, 1f)] float volume = 1f;

    private void Awake()
    {
        cam = Camera.main;
        playerInventory = GetComponentInParent<Inventory>();
    }

    private void Update()
    {
        if (!player || !muzzle || !cam) return;

        // 1) Pozycja docelowa na okrêgu wokó³ gracza — w kierunku myszy
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        Vector2 toMouseFromPlayer = (mouseWorld - player.position);
        Vector2 dirFromPlayer = toMouseFromPlayer.sqrMagnitude > 0.0001f
            ? toMouseFromPlayer.normalized
            : Vector2.right;

        Vector3 targetPos = player.position + (Vector3)(dirFromPlayer * orbitRadius);

        // 2) P³ynne pod¹¿anie do targetPos
        transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);

        // 3) Obrót kuszy: przód (+X) w stronê myszy
        Vector2 aimDir = (mouseWorld - transform.position).normalized;
        if (aimDir.sqrMagnitude < 0.0001f) aimDir = Vector2.right;
        transform.right = aimDir;

        if (!autoShot)
        {
            // 4) Strza³ – lewy przycisk myszy + cooldown
            if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextShot)
            {
                var p = Instantiate(boltPrefab, muzzle.position, Quaternion.identity);
                p.Fire(aimDir, boltSpeed);     // patrz skrypt Projectile ni¿ej
                nextShot = Time.time + fireRate;

                PlayShot();
            }
        }
        else
        {
            if (Time.time >= nextShot)
            {
                var p = Instantiate(boltPrefab, muzzle.position, Quaternion.identity);
                p.Init(playerInventory);
                p.Fire(aimDir, boltSpeed);     // patrz skrypt Projectile ni¿ej
                nextShot = Time.time + fireRate;

                PlayShot();
            }
        }


    }

    public void PlayShot()
    {
        sfx.pitch = Random.Range(0.9f, 1.1f); // lekkie zró¿nicowanie
        var clip = shotClips[Random.Range(0, shotClips.Length)];
        sfx.PlayOneShot(clip, volume);
    }
}
