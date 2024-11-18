using UnityEngine;

public class Enemy : MonoBehaviour
{
    // public
    public int ID;
    public int points = 10;
    
    // private
    private int health = 1;
    
    // events
    public delegate void TargetDestroyedEventHandler (int id, int points);
    public event TargetDestroyedEventHandler TargetDestroyed;
    
    // methods
    public void TakeDamage(int damage, Vector3 shootRoot)
    {
        health -= damage;
        if (health <= 0)
        {
            TargetDestroyed?.Invoke(ID, LongShotPoints(shootRoot));
            TargetDestroyed = null;
            Destroy(gameObject);
        }
    }

    int LongShotPoints(Vector3 shootRoot)
    {
        return points + (int)Vector3.Distance(transform.position, shootRoot);
    }
}
