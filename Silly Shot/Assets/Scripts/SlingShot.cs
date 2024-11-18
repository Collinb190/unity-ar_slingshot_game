using System.Collections;
using UnityEngine;


public class SlingShot : MonoBehaviour
{
   // Ammo-related variables
   public GameObject ammoPrefab;
   public GameObject loadedAmmo;
   public int ammoCache;
  
   // Force-related variables
   private int power = 750;
   private int scalar = 3;


   // State tracking variables
   public bool isPulling;
   private float Zpoint;
   private Vector3 fingerOffset;


   // Delegate and event for reloading
   public delegate void OnReloadEventHandler(int ammoLeft);
   public event OnReloadEventHandler OnReload;
   
   public void Reload()
   {
       if (loadedAmmo == null && transform.childCount == 0 && ammoCache > 0)
       {
           loadedAmmo = Instantiate(ammoPrefab, transform.position, transform.rotation, transform);
           loadedAmmo.GetComponent<Rigidbody>().isKinematic = true;
           loadedAmmo.GetComponent<Ammo>().AmmoHit += LoadedAmmoHit;
           ammoCache--;           
       }
       OnReload?.Invoke(ammoCache);
   }
   void Shoot()
   {
       if (loadedAmmo != null && transform.childCount == 1)
       {
           loadedAmmo.transform.parent = null;
           loadedAmmo.GetComponent<Rigidbody>().isKinematic = false;
           loadedAmmo.GetComponent<Rigidbody>().AddForce(GetShotPower());
           loadedAmmo.GetComponent<Ammo>().hasShot = true;
           loadedAmmo.GetComponent<Ammo>().shootRoot = loadedAmmo.transform.position;      
           loadedAmmo = null;
       }
   }
   public void ClearAmmo()
   {
       if (loadedAmmo)
       {
           Destroy(loadedAmmo);
           loadedAmmo = null;           
       }
       ammoCache = 0;
   }


   IEnumerator DelayedRefill(float RefillTime)
   {
       yield return new WaitForSeconds(RefillTime);
       Reload();
   }


   void LoadedAmmoHit()
   {
       StartCoroutine(DelayedRefill(1.5f));
   }


   public Vector3 GetShotPower()
   {
       Vector3 force = (transform.position - loadedAmmo.transform.position) * power;
       force = ((transform.forward * force.magnitude * scalar) + force);
       return force;
   }


   public Vector3 GetFingerPoint()
   {
       Vector3 fingerPoint = Input.mousePosition;
       fingerPoint.z = Zpoint;
       return Camera.main.ScreenToWorldPoint(fingerPoint);
   }


   void Update()
   {
       if (loadedAmmo == null)
           return;
       if (Input.GetKeyDown(KeyCode.Mouse0) && !isPulling)
       {
           RaycastHit hit;
           Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
           if (Physics.Raycast(ray, out hit))
           {
               if (hit.transform.gameObject == loadedAmmo)
               {
                   Zpoint = Camera.main.WorldToScreenPoint(loadedAmmo.transform.position).z;
                   fingerOffset = loadedAmmo.transform.position - GetFingerPoint();
                   isPulling = true;
               }
           }
       }
       if (isPulling)
       {
           loadedAmmo.transform.position = GetFingerPoint() + fingerOffset;
           loadedAmmo.transform.forward = GetShotPower().normalized;
       }
       if (Input.GetKeyUp(KeyCode.Mouse0) && isPulling)
       {
           Shoot();
           isPulling = false;
       }
   }
}
