using UnityEngine;
using UnityEngine.InputSystem; // nowy Input System


public class Crossbow : MonoBehaviour
{
    [Header("Setup")]
    public Transform muzzle;            // wska¿ dziecko "Muzzle"
    public Projectile boltPrefab;       // wska¿ prefab be³tu

    [Header("Params")]
    public float boltSpeed = 12f;
    public float fireRate = 0.5f;       // sekundy miêdzy strza³ami

    private float nextShotTime;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }


    private void Update()
    {
        if (muzzle == null || cam == null) return;

        // 1) Kierunek do myszy
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 dir = ((Vector2)(mouseWorld - muzzle.position)).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        // 2) Obróæ kuszê w stronê myszy (zak³adamy, ¿e grafika kuszy "celuje" w +X)
        transform.right = dir;

        // 3) Strza³ po LPM
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextShotTime)
        {
            var p = Instantiate(boltPrefab, muzzle.position, Quaternion.identity);
            //p.Fire(dir, boltSpeed);
            nextShotTime = Time.time + fireRate;
        }
    }


    }
