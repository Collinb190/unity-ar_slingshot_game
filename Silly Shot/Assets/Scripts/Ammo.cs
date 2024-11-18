using UnityEngine;

public class Ammo : MonoBehaviour
{    
    // Ammo State
    [SerializeField] private float shotTime = 3f;
    public bool hasShot;
    public Vector3 shootRoot;         

    // Events
    public delegate void HitEventHandler();
    public event HitEventHandler AmmoHit;

    // Update is called once per frame
    void Update()
    {
        if (hasShot)
        {
            transform.Rotate(360 *Time.deltaTime , 0, 0);
            shotTime -= Time.deltaTime;
            if (shotTime <= 0)
            {
                Hit();
                hasShot = false;
            }
        }
    }

    void Hit()
    {
        AmmoHit?.Invoke();
        AmmoHit = null;
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasShot) return;
        if (other.GetComponent<Enemy>() != null) other.GetComponent<Enemy>().TakeDamage(1, shootRoot);
        Hit();
    }
}