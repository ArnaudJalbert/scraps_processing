using System;
using System.Collections.Generic;
using ScrapsGeometries;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TextMeshPro = TMPro.TextMeshPro;
using Toggle = UnityEngine.UI.Toggle;
using UnityEngine.Networking;
using SaveScreenshot;
using GetLocation;
using System.Text.RegularExpressions;
using Unity.VisualScripting;


public class ScrapsPointsManager : MonoBehaviour
{
    // ------------ GAME AND OBJECTS COMPONENTS ----------------
    // Camera of the phone
    public Camera phoneCamera;
    public ScreenshotUploader imageUploader;
    public TestLocationService location;
    public User user;

    // The points that have been ray casted by the user
    public List<GameObject> scrapPoints;

    // Shows user where they are currently pointing
    public GameObject reticle;

    // Anchor point to identify the first point
    private GameObject _anchor;

    // Lines between the points
    private List<GameObject> _scrapPointsLines;

    // Texts displaying the distance
    private List<GameObject> _distanceTexts;

    // ------------ UI COMPONENTS ----------------
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

    // Message to tell user that a scrap has been captured
    public GameObject scrapCaptureMessage;

    // Register Scrap UI
    public GameObject registerForm;
    public Button cancelRegisterForm;
    public Button submitRegisterForm;
    public TMP_InputField scrapNotes;
    public TMP_Dropdown textileType;
    public TMP_Dropdown textileClass;
    public GameObject useGeolocation;
    public Toggle useGeolocationComponent;

    // confirmation of registration
    public GameObject confiirmation;
    public TextMeshProUGUI scrapIDLabel;
    public Button scanNewScrapButton;

    public GameObject takePicureObject;
    public Button takePicureButton;

    // capture messages
    private bool _displayMessages = true;
    public GameObject onePoint;
    public GameObject twoPoints;
    public GameObject threePoints;

    // ----------- AR COMPONENTS ---------------
    // To manage the rays casted by the users
    private ARRaycastManager _raycastManager;

    // Hits with the raycasters
    private List<ARRaycastHit> _hits;

    // Point where the raycaster hit
    private Vector3 _hitPoint;

    // ---------- SCRAP POINTS AND DISTANCES ----------
    // Distances between points
    private List<float> _pointsDistances;

    // Collection of scrap points to be sent
    private ScrapPointsCollection _scrapPointsCollection;

    // --------- STATES VARIABLES -----------
    private bool _inAnchorMode = false;

    // Disable the capturing capabilities when a loop of points has been selected
    private bool _capturingDisabled;
    private bool _confirmationUp = false;

    // Indicates if the raycaster has intersected with a plane
    private bool _hasHit = false;

    private string _timestamp;

    // ---------- GEOMETRY STUFF -------------
    // Default scale of the points
    private Vector3 _sphereScale = new Vector3(0.01f, 0.001f, 0.01f);

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
    }

    void SetHasHit()
    {
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
    }

    void UpdateCloserMessage()
    {
        // If the phone is too fare from plane, alert the user to get closer
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
        // There needs to be at least 2 points set to anchor to the initial point
        if (scrapPoints.Count < 3)
        {
            return;
        }

        if (Vector3.Distance(reticle.transform.position, _anchor.transform.position) < 0.015)
        {
            SetReticleToAnchorMode();
        }
        else if
            (_inAnchorMode) // If in anchor mode and not close enough to anchor, disable the anchor mode and reset reticle
        {
            SetReticleToCaptureMode();
        }
    }

    void DisplayMessages()
    {
        onePoint.SetActive(false);
        twoPoints.SetActive(false);
        threePoints.SetActive(false);
        
        if (_displayMessages)
        {
            // There needs to be at least 2 points set to anchor to the initial point
            if (scrapPoints.Count < 1)
            {
                onePoint.SetActive(true);
                twoPoints.SetActive(false);
                threePoints.SetActive(false);
            }

            if (scrapPoints.Count >= 1)
            {
                onePoint.SetActive(false);
                twoPoints.SetActive(true);
                threePoints.SetActive(false);
            }

            if (scrapPoints.Count > 2)
            {
                onePoint.SetActive(false);
                twoPoints.SetActive(false);
                threePoints.SetActive(true);
            }
        }
    }

    void SetReticleToAnchorMode()
    {
        reticle.transform.position = _anchor.transform.position + (Vector3.up * 0.005f);
        reticle.GetComponent<MeshRenderer>().material.color = Color.blue;
        _inAnchorMode = true;
    }

    void SetReticleToCaptureMode()
    {
        reticle.GetComponent<MeshRenderer>().material.color = Color.red;
        _inAnchorMode = false;
    }

    void CheckHitPoints()
    {
        // If the user is touching the screen and make sure it only happens when we first touch the screen
        if (!EventSystem.current.IsPointerOverGameObject() &&
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Create a new sphere to place into space as a point
            GameObject point = CreatePoint();

            // Set point position depeing on anchor mode and current hit position
            SetPointPosition(point);

            // If it is the first point, identify as the anchor
            if (scrapPoints.Count == 0)
            {
                _anchor = point;
            }

            // Add it to the collection of points
            scrapPoints.Add(point);
        }
    }

    GameObject CreatePoint()
    {
        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        point.name = "Point #" + scrapPoints.Count;
        point.transform.localScale = _sphereScale;
        return point;
    }

    void SetPointPosition(GameObject point)
    {
        // If we are anchored to the first point, move the hit point there so it lines up
        if (_inAnchorMode && scrapPoints.Count > 0)
        {
            point.transform.position = scrapPoints[0].transform.position;
            _capturingDisabled = true;
        }
        else
        {
            point.transform.position = _hitPoint;
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
        distanceTextMesh.fontSize = 0.05f;
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
            distanceLine.startWidth = 0.001f;
            distanceLine.endWidth = 0.001f;
            distanceLine.startColor = Color.white; // Set the start color of the line
            distanceLine.endColor = Color.white; // Set the end color of the line
            distanceLine.SetPosition(0, point1);
            distanceLine.SetPosition(1, point2);
            _scrapPointsLines.Add(distanceLineObject);

            // Put text along the line
            WriteDistanceOnLine(point1, point2, distance, position);
        }
    }

    void DeactivateCaptureUI()
    {
        registerButtonGameObject.SetActive(false);
        resetButtonGameObject.SetActive(false);
        scrapCaptureMessage.SetActive(false);
    }

    void ActivateCaptureUI()
    {
        registerButtonGameObject.SetActive(true);
        resetButtonGameObject.SetActive(true);
    }

    void RegisterScraps()
    {
        // Collection of scraps to send to the database
        _scrapPointsCollection = new ScrapPointsCollection();
        for (int i = 0; i < scrapPoints.Count; i++)
        {
            _scrapPointsCollection.AddScrap(scrapPoints[i].transform.position, i);
        }

        _displayMessages = false;
        DeactivateCaptureUI();

        takePicureObject.SetActive(true);
    }

    void SubmitScrap()
    {
        _displayMessages = false;
        // Extracting information
        string enteredScrapNotes = scrapNotes.text;
        string selectedTextileClass = textileClass.options[textileClass.value].text;
        string selectedTextileType = textileType.options[textileType.value].text;
        // TODO: Get real data here
        string scrapColor = "f0f";

        string baseRequest =
            "https://scraps-processing-api.fly.dev/scraps?textile-class={0}&textile-type={1}&color={2}&owner={3}&note='{4}'&dimensions={5}&image={6}";
        string requestAddress = String.Format(
            baseRequest,
            selectedTextileClass,
            selectedTextileType,
            scrapColor,
            user.userID,
            enteredScrapNotes,
            _scrapPointsCollection,
            _timestamp
        );
        if (useGeolocationComponent)
        {
            string selectedUseGeolocation = "[" + location.latitude.ToString().Replace(",", ".") + "," +
                                            location.longitude.ToString().Replace(",", ".") + "]";
            requestAddress += ("&geolocation=" + selectedUseGeolocation);
        }

        UnityWebRequest request = UnityWebRequest.Put(requestAddress, "");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
        }

        string scrapInfo;

        // Check for errors
        if (request.responseCode == 204)
        {
            // TODO handle errors
        }
        else
        {
            // Request successful, get the response
            scrapInfo = request.downloadHandler.text;
            string pattern = "\"id\"\\s*:\\s*\"([^\"]*)\"";
            MatchCollection matches = Regex.Matches(scrapInfo, pattern);
            ActivateScanNewScrapsInterface(matches[0].Groups[1].Value);
        }

        DeactivateRegisterForm();
    }

    private void ActivateScanNewScrapsInterface(string scrapID)
    {
        confiirmation.SetActive(true);
        scrapIDLabel.SetText("Scrap ID: " + scrapID);
    }

    private void ResetPoints()
    {
        confiirmation.SetActive(false);
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
        // Init the scrap points collection
        _scrapPointsCollection = new ScrapPointsCollection();
        // Disable anchor mode
        _inAnchorMode = false;
        // Set reticle to capture mode again
        SetReticleToCaptureMode();
        // Re-enable capturing
        _capturingDisabled = false;
        _confirmationUp = false;
        // Remove capturing message
        scrapCaptureMessage.SetActive(false);
        _displayMessages = true;
    }

    void TakeScreenshot()
    {
        _timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString();
        imageUploader.UploadScreenshot(_timestamp);
        ActivateRegisterForm();
        takePicureObject.SetActive(false);
    }

    void ActivateRegisterForm()
    {
        _confirmationUp = true;
        registerForm.SetActive(true);
    }

    void DeactivateRegisterForm()
    {
        registerForm.SetActive(false);
        ActivateCaptureUI();
    }

    // Start is called before the first frame update
    void Start()
    {
        // start location request
        location.StartLocation();
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
        // Event listener to submit the scrap capture
        submitRegisterForm.onClick.AddListener(SubmitScrap);
        // Event listener to terminate the register state
        cancelRegisterForm.onClick.AddListener(DeactivateRegisterForm);

        // Get the toggle geolocation component directly
        useGeolocationComponent = useGeolocation.GetComponent<Toggle>();

        // scan new scraps
        scanNewScrapButton.onClick.AddListener(ResetPoints);
        takePicureButton.onClick.AddListener(TakeScreenshot);
    }

    // Update is called once per frame
    void Update()
    {
        SetRaycastManager();
        SetHasHit();
        DisplayMessages();

        if (_hasHit && !_capturingDisabled)
        {
            if (initialMessage.activeSelf)
            {
                DisableInitialMessage();
            }
            UpdateReticle();
            UpdateCloserMessage();
            CheckIfInAnchorZone();
            CheckHitPoints();
            DrawDistanceLines();
        }
        else if (_confirmationUp)
        {
            
            scrapCaptureMessage.SetActive(false);
        }
        else if (_capturingDisabled)
        {
            _displayMessages = false;
            scrapCaptureMessage.SetActive(true);
        }
    }
}