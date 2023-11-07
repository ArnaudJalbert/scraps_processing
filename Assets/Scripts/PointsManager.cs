using System;
using System.Collections.Generic;
using ScrapsGeometries;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TextMeshPro = TMPro.TextMeshPro;

public class ScrapsPointsManager : MonoBehaviour
{
    // Camera of the phone
    public Camera phoneCamera;

    // The points that have been ray casted by the user
    public List<GameObject> scrapPoints;

    // Shows user where they are currently pointing
    public GameObject reticle;

    // Register Scrap Button
    public GameObject registerButtonGameObject;
    public Button registerButton;

    // Reset Button
    public GameObject resetButtonGameObject;
    public Button resetButton;
    
    // Panel to cover screen while the environment is being scanned
    public GameObject initialMessage;
    
    // Message to tell user to get closer
    public GameObject closerMessage;

    // Lines between the points
    private List<GameObject> _scrapPointsLines;

    // Distances between points
    private List<float> _pointsDistances;

    // Texts displaying the distance
    private List<GameObject> _distanceTexts;

    // To manage the rays casted by the users
    private ARRaycastManager _raycastManager;

    // Hits with the raycasters
    private List<ARRaycastHit> _hits;
    private bool _hasHit = false;
    private Vector3 _hitPoint;

    // Anchor point to identify the first point
    private GameObject _anchor;
    private bool _inAnchorMode = false;

    // Collection of scrap points to be sent
    private ScrapPointsCollection _scrapPointsCollection;

    // Default scale of the points
    private Vector3 _sphereScale = new Vector3(0.01f, 0.01f, 0.01f);

    // Z Offset for the text
    private Vector3 _yOffset = new Vector3(0f, 0.01f, 0f);

    void DisableInitialMessage()
    {
        initialMessage.SetActive(false);
        registerButtonGameObject.SetActive(true);
        resetButtonGameObject.SetActive(true);
    }
    
    void SetRaycastManager()
    {
        _hits = new List<ARRaycastHit>();
        _raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), _hits,
            TrackableType.PlaneWithinPolygon);
        if (_hits.Count > 0)
        {
            _hasHit = true;
            _hitPoint = _hits[0].pose.position;
        }
        else
        {
            _hasHit = false;
        }
    }

    void UpdateReticle()
    {
        reticle.transform.position = _hits[0].pose.position + (Vector3.up * 0.005f);
        reticle.transform.rotation = _hits[0].pose.rotation;

        if (Vector3.Distance(reticle.transform.position, phoneCamera.transform.position) > 0.75)
        {
            closerMessage.SetActive(true);
        }
        else
        {
            closerMessage.SetActive(false);
        }
    }

    void CheckIfInAnchorZone()
    {
        if (scrapPoints.Count < 3)
        {
            return;
        }

        if (Vector3.Distance(reticle.transform.position, _anchor.transform.position) < 0.01)
        {
            reticle.transform.position = _anchor.transform.position + (Vector3.up * 0.005f);
            reticle.GetComponent<MeshRenderer>().material.color = Color.blue;
            _inAnchorMode = true;
        }
        else if (_inAnchorMode)
        {
            reticle.GetComponent<MeshRenderer>().material.color = Color.red;
            _inAnchorMode = false;
        }
    }

    void CheckHitPoints()
    {
        // If the user is touching the screen and make sure it only happens when we first touch the screen
        if (!EventSystem.current.IsPointerOverGameObject() &&
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Create a new sphere to place into space as a point
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.name = "Point #" + scrapPoints.Count;
            point.transform.localScale = _sphereScale;

            // If we are anchored to the first point, move the hit point there so it lines up
            if (_inAnchorMode)
            {
                point.transform.position = scrapPoints[0].transform.position;
            }
            else
            {
                point.transform.position = _hitPoint;
            }

            // If it is the first point, identify as the anchor
            if (scrapPoints.Count == 0)
            {
                _anchor = point;
            }

            // Add it to the collection of points
            scrapPoints.Add(point);
        }
    }

    void WriteDistanceOnLine(Vector3 point1, Vector3 point2, float distance, int position)
    {
        // Init the game object and the text mesh
        GameObject distanceTextObject = new GameObject("Distance Text #" + position);
        TextMeshPro distanceTextMesh = distanceTextObject.AddComponent<TextMeshPro>();

        // Convert the distance into centimeters
        float distanceInCM = distance * 100;
        string textValue = (distanceInCM.ToString("0.00") + "cm");

        // Set up the text mesh
        distanceTextMesh.text = textValue;
        distanceTextMesh.fontSize = 0.1f;
        distanceTextMesh.color = Color.black;
        distanceTextMesh.alignment = TextAlignmentOptions.Center;
        distanceTextMesh.transform.position = ((point1 + point2) / 2f) + _yOffset;
        distanceTextMesh.transform.LookAt(phoneCamera.transform);
        distanceTextMesh.transform.Rotate(new Vector3(0, 180, 0));
        _distanceTexts.Add(distanceTextObject);
    }

    void DrawDistanceLines()
    {
        // Drawing the lines as the points are created
        if (scrapPoints.Count >= _scrapPointsLines.Count + 2)
        {
            // Position of the line
            int position = _scrapPointsLines.Count;

            // Get the 3D position of the scrap points
            Vector3 point1 = scrapPoints[scrapPoints.Count - 1].transform.position;
            Vector3 point2 = scrapPoints[scrapPoints.Count - 2].transform.position;

            // Distance between the 2 points
            float distance = Vector3.Distance(point1, point2);

            // Set up the game object for the distance lines
            GameObject distanceLineObject = new GameObject("Distance Line #" + position);

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
            _scrapPointsLines.Add(distanceLineObject);

            // Put text along the line
            WriteDistanceOnLine(point1, point2, distance, position);
        }
    }

    void RegisterScraps()
    {
        // Collection of scraps to send to the database
        _scrapPointsCollection = new ScrapPointsCollection();
        for (int i = 0; i < scrapPoints.Count; i++)
        {
            _scrapPointsCollection.AddScrap(scrapPoints[i].transform.position, i);
        }

        Console.Write(_scrapPointsCollection);
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

        foreach (var distanceText in _distanceTexts)
        {
            Destroy(distanceText);
        }

        // Init the list of scrap points
        scrapPoints = new List<GameObject>();
        // Init the list of lines between scrap points
        _scrapPointsLines = new List<GameObject>();
        // Init the distance texts
        _distanceTexts = new List<GameObject>();
        // Init the 
        _scrapPointsCollection = new ScrapPointsCollection();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set up the raycast manager
        _raycastManager = GetComponent<ARRaycastManager>();
        // Init the list of scrap points
        scrapPoints = new List<GameObject>();
        // Init the list of lines between scrap points
        _scrapPointsLines = new List<GameObject>();
        // Init the distance texts
        _distanceTexts = new List<GameObject>();
        // Event listener to register the scrap
        registerButton.onClick.AddListener(RegisterScraps);
        // Event listener to reset the points
        resetButton.onClick.AddListener(ResetPoints);
    }

    // Update is called once per frame
    void Update()
    {
        SetRaycastManager();
        if (_hasHit)
        {
            if (initialMessage.activeSelf)
            {
                DisableInitialMessage();
            }

            UpdateReticle();
            CheckIfInAnchorZone();
            CheckHitPoints();
            DrawDistanceLines();
        }
    }
}