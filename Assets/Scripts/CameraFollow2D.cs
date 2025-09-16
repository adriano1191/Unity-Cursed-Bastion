using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;          // gracz
    public Vector3 offset = new Vector3(0, 0f, -10f);
    public float smoothTime = 0.15f;  // im wiêksze, tym „ciê¿sza” kamera

    private Vector3 velocity;         // do SmoothDamp

    void LateUpdate()
    {
        if (!target) return;

        Vector3 goal = target.position + offset;
        goal.z = offset.z; // utrzymaj -10
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }
}
