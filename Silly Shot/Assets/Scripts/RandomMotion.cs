using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class RandomMotion : MonoBehaviour
{
    // Movement Variables
    [SerializeField] private float moveSpeed = 0.005f;         // Speed of movement
    private bool hasDestination;                              // Flag for valid destination
    private Vector3 destination;                              // Target destination
    private Quaternion rotation;                              // Rotation towards destination
    private bool moving ;                                     // Flag to start moving

    // AR and Plane Variables
    private ARPlane movementPlane;                            // The AR plane the target moves on
    private Vector3 planeCenter;                              // The center of the AR plane
    private float planeRange;                                 // Maximum range for movement on the plane
    private float rayYoffset = 0.5f;                          // Offset for raycasting (Y-axis)
    private float colliderHeight;                             // Height of the collider


    // Update is called once per frame
    void Update()
    {
        if (!moving) return;
        if (hasDestination)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, moveSpeed * 9);            
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed);
            if (Vector3.Distance(transform.position, destination) < 0.01f) hasDestination = false;
        }
        else
        {
            hasDestination = RandomPoint(planeCenter, rayYoffset, planeRange, out destination);
            rotation = Quaternion.LookRotation(destination - transform.position, Vector3.up);
        }
    }
    public void StartMoving(ARPlane plane)
    {
        movementPlane = plane;
        planeCenter = plane.center;
        planeRange = Mathf.Max(plane.size.x, plane.size.y);
        colliderHeight = transform.localScale.y * GetComponent<CapsuleCollider>().height;
        transform.position = planeCenter + Vector3.up * colliderHeight / 2;
        hasDestination = RandomPoint(planeCenter, rayYoffset, planeRange, out destination);
        rotation = Quaternion.LookRotation(destination - transform.position, Vector3.up);
        moving = true;
    }
    
    public bool RandomPoint(Vector3 center, float rayYoffset, float range, out Vector3 result)
    {
        Vector3 next = center + Random.insideUnitSphere * range;
        RaycastHit hit;
        if (Physics.Raycast(next, Vector3.down, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject == movementPlane.gameObject)
            {
                result = hit.point + Vector3.up * colliderHeight / 2;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}
