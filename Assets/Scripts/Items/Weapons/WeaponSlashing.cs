using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class WeaponSlashing : MonoBehaviour
{
    Transform _parent;
    Vector3 _startLocal;
    bool _detached;

    [Header("Targeting")]
    [SerializeField] private FindCloseTarget closeTarget;
    private Transform targetEnemy;
    [Tooltip("true = auto-aims nearest enemy, false = uses mouse / external AimDir")]
    [SerializeField] private bool autoAim = true;

    [Header("Firing")]
    [Tooltip("true = fires automatically on cooldown; false = requires input (LMB by default)")]
    [SerializeField] private bool autoAttack = true;

    [Header("Refs")]
    [SerializeField] private WeaponStats weaponStats;
    [SerializeField] private Inventory playerInventory;

    [Header("Fly")]
    [SerializeField] float baseMoveSpeed = 10f;
    [SerializeField] float standoff = 2f; // odległość od celu

    [Header("Arm")]
    [SerializeField] private float baseSpeed = 50f;
    [SerializeField] private float armRise = 140f;
    [SerializeField] private float armDown = 0f;
    [SerializeField] private float armReset = 90f;
    [SerializeField] private float targetAngle = 0f;

    [Header("Weapon")]
    [SerializeField] private Transform weapon;
    [SerializeField] private float baseWeaponSpeed = 50f;
    [SerializeField] private float weaponRise = 140f;
    [SerializeField] private float weaponDown = 0f;
    [SerializeField] private float weaponReset = 90f;
    [SerializeField] private float weaponTargetAngle = 0f;

    [Header("Rotation")]
    [SerializeField] Transform pivot;
    [SerializeField] float rotateSpeed = 72f;   // deg/s (0 = natychmiast)
    [SerializeField] float forwardOffsetDeg = 0f;

    [SerializeField] private Vector3 startPostion = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 startRotation = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 startScale = new Vector3(1f, 1f, 1f);
    [SerializeField] bool canAttack = true;
    [SerializeField] bool isAttacking = false;
    [SerializeField] bool isFlying = false;
    [SerializeField] bool isReturning = false;
    [SerializeField] bool isRotating = false;
    [SerializeField] bool dealDamage = false;

    [SerializeField] Vector3 target;
    [SerializeField] Vector3 lastTarget;
    private float nextShotTime;
    float cooldown;
    float cooldownTimer;
    bool flipped = false;
    float xScale = 0f;

 

    private void Awake()
    {
        if (!weaponStats) weaponStats = GetComponent<WeaponStats>();
        if (!playerInventory) playerInventory = GetComponentInParent<Inventory>();
        if (!closeTarget) closeTarget = GetComponent<FindCloseTarget>();
        targetAngle = armRise;
        weaponTargetAngle = weaponRise;
        startPostion = transform.localPosition;
        startRotation = transform.localEulerAngles;
        startScale = transform.localScale;

        _parent = transform.parent;
        _startLocal = transform.localPosition;
        xScale = transform.localScale.x;

    }

    private void Update()
    {
        cooldown = weaponStats.AttackCooldownSeconds;
        //ArmAttackAnimation();
        //FlyToTarget(closeTarget.CurrentTarget);
        //RotateToTarget();

        // AttackSpeed is attacks/second
        //if (Time.time < nextShotTime) return;

        weaponStats.PlayAttackSfx();
        //nextShotTime = Time.time + cooldown;
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= cooldown && !isReturning && !isFlying && !isAttacking)
        {
            canAttack = true;
        }

        if (canAttack && closeTarget.CurrentTarget != null)
        {
            FlyToTarget();
            RotateToTarget();
            isFlying = true;
            cooldownTimer = 0f;

        }
        if (isAttacking)
        {
            
            ArmAttackAnimation();
            FlyToTarget();
            

        }
        if (isReturning)
        {
            FlyToTarget();
            RotateToTarget();

            if (transform.localPosition == startPostion)
            {
                isReturning = false;
                isFlying = false;
                //canAttack = true;
                targetAngle = armRise;
                weaponTargetAngle = weaponRise;
            }
        }
        if (!isFlying && !isAttacking && !isReturning)
        {
            RotateToTarget();
        }
        if(!closeTarget.CurrentTarget && isReturning)
        {
            FlyToTarget();
            
        }
    }

    public void Flip(float x)
    {
        if (xScale == x) 
        { 
            return;
        }
        else
        {
                armRise = armRise * -1f;
                armDown = armDown * -1f;
                armReset = armReset * -1f;
                targetAngle = targetAngle * -1f;
                xScale = x;
        }


    }

    public float SpeedFactor(float baseValue)
    {
        float speed = baseValue / weaponStats.AttackSpeedFactor;
        return speed;
    }

    public void ArmAttackAnimation()
    {
        float speed = SpeedFactor(baseSpeed);
        float weaponSpeed = SpeedFactor(baseWeaponSpeed);

        //bool reached = false; //sprawdz czy doszło do kąta ataku
        //Arm rotation
        float currentArm = transform.localEulerAngles.z;
        float nextZArm = Mathf.MoveTowardsAngle(currentArm, targetAngle, speed * Time.deltaTime);
        var e = transform.localEulerAngles;
        e.z = nextZArm;
        transform.localEulerAngles = e;

        //Weapon rotation
        float currentWeapon = weapon.localEulerAngles.z;
        float nextZWeapon = Mathf.MoveTowardsAngle(currentWeapon, weaponTargetAngle, weaponSpeed * Time.deltaTime);
        var we = weapon.localEulerAngles;
        we.z = nextZWeapon;
        weapon.localEulerAngles = we;




        //const float EPS = 0.1f; // tolerancja w stopniach
        // reached = Mathf.Abs(Mathf.DeltaAngle(nextZ, attackAngle)) <= EPS;
        //Debug.Log("Obrót ramienia: " + currentArm + " do " + targetAngle + " obrót broni: " + currentWeapon + " do " + weaponTargetAngle);
        if (Mathf.Abs(Mathf.DeltaAngle(currentArm, targetAngle)) <= 0.1f)
        {
            if (targetAngle == armRise && weaponTargetAngle == weaponRise)
            {
                dealDamage = true;
                targetAngle = armDown;
                weaponTargetAngle = weaponDown;
                weaponStats.PlayAttackSfx();
                //Debug.Log("Uderzenie");
                //TryHit();
            }
            else if (targetAngle == armDown && weaponTargetAngle == weaponDown)
            {
                targetAngle = armReset;
                weaponTargetAngle = weaponReset;
                dealDamage = false;
                // Debug.Log("Reset");
                //isAttacking = false;
            }
            else if (targetAngle == armReset && weaponTargetAngle == weaponReset)
            {
                isAttacking = false;
                isFlying = false;
                isReturning = true;
                targetAngle = armRise;
                weaponTargetAngle = weaponRise;
               // Debug.Log("Powrót" + canAttack);
            }
        }

    }

    public void FlyToTarget()
    {


        if (closeTarget.CurrentTarget && !isAttacking && !isFlying && !isReturning)
        {
            target = closeTarget.CurrentTarget.position;
            lastTarget = target;
        }
        else if(!closeTarget.CurrentTarget)
        {
            target = lastTarget;
        }
        //if (!target)
       // {
         //   return; // brak celu
       // }

        // gdy zaczynamy atak/return – odczep, by ruch rodzica nie wpływał
        if (!_detached) Detach();


        Vector3 from = transform.position;
        Vector3 to = target;
        Vector3 dir = (to - from);
        dir.z = 0f;
        if (dir.x < 0f) 
        { 
            transform.localScale = new Vector3(-1f, startScale.y, 1f);
            Flip(transform.localScale.x);
        }
        else
        {
            transform.localScale = new Vector3(1f, startScale.y, 1f);
            Flip(transform.localScale.x);
        }
        

        if (isReturning)
        {
            float moveSpeed = SpeedFactor(baseMoveSpeed);
            //transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPostion, moveSpeed * Time.deltaTime);
            Vector3 homeWorld = _parent ? _parent.TransformPoint(_startLocal) : _startLocal;
            transform.position = Vector3.MoveTowards(transform.position, homeWorld, moveSpeed * Time.deltaTime);
            if ((transform.position - homeWorld).sqrMagnitude <= 0.0001f)
            {
                Reattach();
                isReturning = false;
                

            }

            //return; // już wystarczająco blisko
        }
        else if(!isReturning)
        {
            float moveSpeed = SpeedFactor(baseMoveSpeed);
            Vector3 stopPos = to - dir.normalized * standoff;
            transform.position = Vector3.MoveTowards(from, stopPos, moveSpeed * Time.deltaTime);
            if (transform.position == stopPos)
            {

                canAttack = false;
                
                isAttacking = true;

            }
        }





    }

    void RotateToTarget()
    {
        
        Transform target = closeTarget.CurrentTarget;
        Transform t = pivot ? pivot : (weapon ? weapon : transform);
   

        if (t && !target)
        {
            if (rotateSpeed <= 0f)
                t.rotation = startRotation == null ? Quaternion.identity : Quaternion.Euler(startRotation); // natychmiast
            else
                t.rotation = Quaternion.RotateTowards(t.rotation, Quaternion.Euler(startRotation), rotateSpeed * Time.deltaTime);
        }
        
        if (!target) return;
        // Obracamy w stronę celu (pivot jeśli jest, inaczej weapon, inaczej ten transform)
        Vector3 d = target.position - t.position; d.z = 0f;
        if (d.sqrMagnitude < 1e-6f) return;

        float targetZ = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + forwardOffsetDeg;
        Quaternion q = Quaternion.Euler(0f, 0f, targetZ+90f); // +90 bo domyślnie w Unity obiekt patrzy w prawo (X+)

        if (rotateSpeed <= 0f)
            t.rotation = q; // natychmiast
        else
            //t.rotation = q;
        t.rotation = Quaternion.RotateTowards(t.rotation, q, rotateSpeed * Time.deltaTime);
    }


    void Detach()
    {
        if (_detached) return;
        transform.SetParent(null, true);   // odczep, zachowując world pose
        _detached = true;
    }

    void Reattach()
    {
        if (!_detached) return;
        transform.SetParent(_parent, false);    // wróć do parenta w local-space
        transform.localPosition = _startLocal;  // przywróć dokładne gniazdo
        _detached = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (hasHit) return;               // prevent multi‑hits on stacked colliders

        // Optional: ignore non‑monsters fast
        if (!other.CompareTag("Monster"))
        {
            
            return;
        }

        if (!dealDamage)
        {
            return;
        }

        //hasHit = true;
        int damage = GetDamage();
        playerInventory?.NotifyOnHit(other.gameObject, ref damage);

        var hp = other.GetComponent<MonsterHealth>();
        if (hp != null)
        {
            bool killed = hp.TakeDamage(damage);
            if (killed) playerInventory?.NotifyOnKill(other.gameObject);
        }

        // Knockback (use projectile flight dir if available)
        var targetRb = other.attachedRigidbody;
        if (targetRb != null)
        {
           /* Vector2 dir = (rb.linearVelocity.sqrMagnitude > 1e-6f)
                ? rb.linearVelocity.normalized
                : (Vector2)(other.transform.position - transform.position).normalized;

            targetRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
           */
            // Optional: try stun if component exists
            var mover = other.GetComponent<MonsterStandardMove>();
            if (mover != null)
            {
                mover.Stun(0.1f);
            }
        }
    }

    public int GetDamage()
    {
        int damage = weaponStats.GetCurrentDamage();
        return damage;
    }

}
