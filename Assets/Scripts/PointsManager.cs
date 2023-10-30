using System;
using System.Collections;
using System.Collections.Generic;
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
        if (_hits.Count < 1)
        {
            return;
        }

        // If the user is touching the screen and make sure it only happens when we first touch the screen
        if (!(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            return;
        }

        if (_pointsAmount > scrapPointsGameObjects.Length)
        {
            return;
        }

        scrapPointsGameObjects[_pointsAmount].SetActive(true);
        scrapPointsGameObjects[_pointsAmount].transform.position = _hits[0].pose.position;
        _pointsAmount++;
    }

    void UpdatePoints()
    {
        if (_pointsAmount < 2)
        {
            return;
        }

        for (var index = 0; index < pointsDistances.Length; index++)
        {
            // Get the distance between the two points
            pointsDistances[index] = Vector3.Distance(
                scrapPointsGameObjects[index].transform.position,
                scrapPointsGameObjects[index + 1].transform.position
            );
            
        }
        
        
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
        UpdatePoints();
    }
}