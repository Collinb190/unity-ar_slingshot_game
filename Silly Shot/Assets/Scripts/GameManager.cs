using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
   // public
   [Header("Settings")]
   public GameObject enemyPrefab;
   public GameObject ammoImagePrefab;
   public int enemieCount = 5;
   public int ammoCount = 7;
   public Material PlaneOcclusionMaterial;

   [Header("Canvas Objects")]
   public GameObject findingUI;
   public GameObject pickingUI;
   public GameObject startButton;
   public GameObject gameUI;
   public GameObject ammoUI;
   public GameObject replayButton;
   public Text score;
   
   // private
   int scoreTotal;
  
   SlingShot slingShot;
   ARSession session;
   ARPlane myPlane;   
   ARRaycastManager raycastManager;
   ARPlaneManager planeManager;

   List<ARRaycastHit> hits = new();
   Dictionary<int, GameObject> enemies = new();

   //Events
   public delegate void PlaneSelectedEventHandler(ARPlane thePlane);
   public event PlaneSelectedEventHandler OnPlaneSelected;
   
   void Awake()
   {
       session = FindObjectOfType<ARSession>();
       session.Reset();
   }   
  
   // Start is called before the first frame update
   void Start()
   {
       raycastManager = FindObjectOfType<ARRaycastManager>();
       planeManager = FindObjectOfType<ARPlaneManager>();
       slingShot = FindObjectOfType<SlingShot>();
      
       planeManager.planesChanged += PlanesFound;
       OnPlaneSelected += PlaneSelected;
   }
   
   // Update is called once per frame
   void Update()
   {
       if (Input.touchCount > 0 && myPlane == null && planeManager.trackables.count > 0) SelectPlane();
   }
   
   private void SelectPlane()
   {
       Touch press = Input.GetTouch(0);

       if (press.phase == TouchPhase.Began)
       {
           if (raycastManager.Raycast(press.position, hits, TrackableType.PlaneWithinPolygon))
           {
               ARRaycastHit hit = hits[0];
               myPlane =  planeManager.GetPlane(hit.trackableId);               
               myPlane.GetComponent<LineRenderer>().positionCount = 0;
               myPlane.GetComponent<Renderer>().material = PlaneOcclusionMaterial;
              
               foreach(ARPlane plane in planeManager.trackables)
               {
                   if (plane != myPlane)
                   {
                       plane.gameObject.SetActive(false);
                   }
               }
               planeManager.enabled = false;
               pickingUI.SetActive(false);
               OnPlaneSelected?.Invoke(myPlane);
           }
       }
   }
   
   void PlanesFound(ARPlanesChangedEventArgs args)
   {
       if (myPlane == null && planeManager.trackables.count > 0)
       {
           findingUI.SetActive(false);
           pickingUI.SetActive(true);
           planeManager.planesChanged -= PlanesFound;
       }
   }

   void PlaneSelected(ARPlane plane)
   {
       foreach (KeyValuePair<int, GameObject> e in enemies) Destroy(e.Value);
       enemies.Clear();
       
       startButton.SetActive(true);
       for (int i = 1; i <= enemieCount; i++)
       {
           GameObject enemie = Instantiate(enemyPrefab, plane.center, plane.transform.rotation, plane.transform);
           enemie.GetComponent<RandomMotion>().StartMoving(plane);
           enemie.GetComponent<Enemy>().ID = i;
           enemie.GetComponent<Enemy>().TargetDestroyed += HandleHitEnemie;
           enemies.Add(i, enemie);
       }
   }

   void HandleHitEnemie(int id, int points)
   {
       enemies.Remove(id);
       scoreTotal += points;
       score.text = scoreTotal.ToString();
       if (enemies.Count == 0)
       {
           ShowPlayAgain();
       }
   }
   
   public void StartGame()
   {
       slingShot.ammoCache = ammoCount;
       slingShot.OnReload += SlingShotRefill;
       slingShot.Reload();
       scoreTotal = 0;
       score.text = scoreTotal.ToString();
       startButton.SetActive(false);
       gameUI.SetActive(true);

       for (int i = 0; i < slingShot.ammoCache; i++)
       {
           GameObject ammo = Instantiate(ammoImagePrefab);
           ammo.transform.SetParent(ammoUI.transform, false);
       }
   }
   
   void SlingShotRefill(int ammoLeft)
   {
       if (ammoUI.transform.childCount > 0 && ammoLeft >= 0) Destroy(ammoUI.transform.GetChild(0).gameObject);
       else if (ammoLeft == 0) ShowPlayAgain(); 
   }
   
   public void ShowPlayAgain()
   {
       foreach (Transform image in ammoUI.transform) Destroy(image.gameObject);
 
       slingShot.ClearAmmo();
       slingShot.OnReload -= SlingShotRefill;
       replayButton.SetActive(true);
   }
   
   public void Replay()
   {
       PlaneSelected(myPlane);
   }
   
   public void ExitGame()
   {
       Application.Quit();
   }
   
   public void ResetGame()
   {
       SceneManager.LoadScene("ARSlingshotGame");
   }
}
