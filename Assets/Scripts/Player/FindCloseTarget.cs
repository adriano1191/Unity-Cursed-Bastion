using UnityEngine;

public class FindCloseTarget : MonoBehaviour
{
    public Transform origin;                // zwykle gracz
    public float range = 12f;
    public LayerMask targetMask;            // warstwa przeciwników
    public Transform CurrentTarget;      // aktualnie najbli¿szy cel

    void Awake() { if (!origin) origin = transform; }

    void Update() { CurrentTarget = FindNearest(); }

    Transform FindNearest()
    {
        Vector2 o = origin.position;
        var hits = Physics2D.OverlapCircleAll(o, range, targetMask);
        Transform best = null; float bestD2 = float.PositiveInfinity;
        foreach (var c in hits)
        {
            Vector2 p = c.bounds.ClosestPoint(o);
            float d2 = (p - o).sqrMagnitude;
            if (d2 < bestD2) { bestD2 = d2; best = c.transform; }
        }
        return best;
    }

    void OnDrawGizmosSelected()
    {
        var src = origin ? origin : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(src.position, range);
    }
}
