using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ScrapsPointsManager : MonoBehaviour
{
    // The points that have been ray casted by the user
    public List<GameObject> scrapPoints;

    // Shows user where they are currently pointing
    public GameObject reticle;

    // Register Scrap Button
    public Button registerButton;

    // Reset Button
    public Button resetButton;

    // Lines between the points
    private List<LineRenderer> _scrapPointsLines;

    // Distances between points
    private List<float> _pointsDistances;

    // To manage the rays casted by the users
    private ARRaycastManager _raycastManager;

    // Hits with the raycasters
    private List<ARRaycastHit> _hits;

    // Default scale of the points
    private Vector3 _sphereScale = new Vector3(0.01f, 0.01f, 0.01f);

    void UpdateReticle()
    {
        if (_hits.Count < 1)
        {
            return;
        }

        reticle.transform.position = _hits[0].pose.position;
        reticle.transform.rotation = _hits[0].pose.rotation;
    }

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
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Create a new sphere to place into space as a point
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.name = "Point #" + scrapPoints.Count;
            point.transform.localScale = _sphereScale;
            point.transform.position = _hits[0].pose.position;
            scrapPoints.Add(point);
        }
    }

    void DrawDistanceLines()
    {
        // Drawing the lines as the points are created
        if (scrapPoints.Count >= _scrapPointsLines.Count + 2)
        {
            // Get the 3D position of the scrap points
            Vector3 point1 = scrapPoints[scrapPoints.Count - 1].transform.position;
            Vector3 point2 = scrapPoints[scrapPoints.Count - 2].transform.position;

            // Distance between the 2 points
            float distance = Vector3.Distance(point1, point2);

            // Set up the game object for the distance lines
            GameObject distanceLineObject = new GameObject("Distance Line #" + _scrapPointsLines.Count);

            // Set up the rendered for the lines and link it to the game object
            LineRenderer distanceLine = distanceLineObject.AddComponent<LineRenderer>();

            // Make sure there is a material attached
            distanceLine.material = new Material(Shader.Find("Sprites/Default"));
            distanceLine.positionCount = 2;
            distanceLine.startWidth = 0.01f;
            distanceLine.endWidth = 0.01f;
            distanceLine.startColor = Color.white; // Set the start color of the line
            distanceLine.endColor = Color.white; // Set the end color of the line
            distanceLine.SetPosition(0, point1);
            distanceLine.SetPosition(1, point2);
            _scrapPointsLines.Add(distanceLine);

            // Put text along the line
            Console.Write(distance);
        }
    }

    void RegisterScraps()
    {
        Console.Write("Register");
    }

    private void ResetPoints()
    {
        foreach (var scrapPoint in scrapPoints)
        {
            Destroy(scrapPoint);
        }

        foreach (var scrapPointsLine in _scrapPointsLines)
        {
            Destroy(scrapPointsLine);
        }

        // Init the list of scrap points
        scrapPoints = new List<GameObject>();
        // Init the list of lines between scrap points
        _scrapPointsLines = new List<LineRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set up the raycast manager
        _raycastManager = GetComponent<ARRaycastManager>();
        // Init the list of scrap points
        scrapPoints = new List<GameObject>();
        // Init the list of lines between scrap points
        _scrapPointsLines = new List<LineRenderer>();
        // Event listener to register the scrap
        registerButton.onClick.AddListener(RegisterScraps);
        // Event listener to reset the points
        resetButton.onClick.AddListener(ResetPoints);
    }

    // Update is called once per frame
    void Update()
    {
        SetRaycastManager();
        UpdateReticle();
        CheckHitPoints();
        DrawDistanceLines();
    }
}