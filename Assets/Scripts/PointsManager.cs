using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Serialization;

public class ScrapsPointsManager : MonoBehaviour
{
    // The points that have been ray casted by the user
    public GameObject[] scrapPointsGameObjects;
    public LineRenderer line;

    // The distances between all the points, following the order in which they have been placed
    public float[] pointsDistances;

    // Shows user where they are currently pointing
    private GameObject _reticle;

    // Display the distance in text
    public TMP_Text distanceText;

    // Keeps track of the amount of points we have
    private int _pointsAmount = 0;

    // To manage the rays casted by the users
    private ARRaycastManager _raycastManager;

    // Hits with the raycasters
    private List<ARRaycastHit> _hits;

    void SetRaycastManager()
    {
        _hits = new List<ARRaycastHit>();
        _raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), _hits,
            TrackableType.PlaneWithinPolygon);
    }

    void CheckHitPoints()
    {
        // Check if there is a raycast hit
        if (_hits.Count < 1 && _pointsAmount >= scrapPointsGameObjects.Length)
        {
            return;
        }

        // If the user is touching the screen and make sure it only happens when we first touch the screen
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            scrapPointsGameObjects[_pointsAmount].SetActive(true);
            scrapPointsGameObjects[_pointsAmount].transform.position = _hits[0].pose.position;
            _pointsAmount++;
        }
        // If the user has ended its touch down of the screen, then we display the distance between the points
        else if (_pointsAmount >= 2 && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            Console.Write("Checking distances and rendering lines");
            Vector3 point1 = scrapPointsGameObjects[_pointsAmount - 2].transform.position;
            Vector3 point2 = scrapPointsGameObjects[_pointsAmount - 1].transform.position;
            distanceText.text = GetPointsDistances(point1, point2).ToString();
            line.enabled = true;
            line.SetPosition(0, point1); 
            line.SetPosition(1, point2);
        }
        
    }

    float GetPointsDistances(Vector3 point1, Vector3 point2)
    {
        return Vector3.Distance(point1, point2);
    }

    // Start is called before the first frame update
    void Start()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        pointsDistances = new float[scrapPointsGameObjects.Length];
    }

    // Update is called once per frame
    void Update()
    {
        SetRaycastManager();
        CheckHitPoints();
    }
}