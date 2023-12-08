using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    // user object
    public User user;
    public GameObject initialMessage;
    public GameObject accountUI;
    
    // Start is called before the first frame update
    public Button login;
    public Button createAccount;
    
    // create account
    public GameObject createAccountForm;
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField instagramField;
    public Button createAccountButtonTrigger;
    public  TextMeshProUGUI response;
    public Button scanScraps;
    
    // login account
    public GameObject loginForm;
    public TMP_InputField loginIdentification;
    public Button loginButtonTrigger;
    public  TextMeshProUGUI loginResponse;
        
    void ActivateLoginOption()
    {
        createAccount.gameObject.SetActive(false);
        login.gameObject.SetActive(false);
        loginForm.SetActive(true);
    }

    void CreateAccountOption()
    {
        createAccount.gameObject.SetActive(false);
        login.gameObject.SetActive(false);
        createAccountForm.SetActive(true);
    }
    
    void CreateAccountTrigger()
    {
        string username = usernameField.text;
        string email = emailField.text;
        string instagram = instagramField.text;
        string responseText;
        
        
        string baseRequest =
            "https://scraps-processing-api-delicate-pond-5077.fly.dev/create-user?username={0}&email={1}&instagram={2}";
        string requestAddress = String.Format(
            baseRequest,
            username,
            email,
            instagram
        );
        
        Debug.Log(requestAddress);
        
        UnityWebRequest request = UnityWebRequest.Post(requestAddress, "");
        
        var operation = request.SendWebRequest();
        while (!operation.isDone) {}
        
        // Check for errors
        if (request.responseCode != 200)
        {
            responseText = request.downloadHandler.text;
            Debug.Log("Error: " + responseText);
            response.SetText(responseText);
        }
        else
        {
            // Request successful, get the response
            responseText = request.downloadHandler.text;
            response.SetText("User Successfully Created!");
            string pattern = "\"user_id\":\\s*\"(.*?)\",\\s*\"username\"";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(responseText);
            user.userID = match.Groups[1].Value;
            scanScraps.gameObject.SetActive(true);
            // Process the response data here
        }
    }

    void LoginTrigger()
    {
        Debug.Log("login");
        //TODO do the actual request here
        
        string searchUsername = loginIdentification.text;
        string responseText;
        
        string baseRequest =
            "https://scraps-processing-api-delicate-pond-5077.fly.dev/user/{0}/user-id";
        string requestAddress = String.Format(
            baseRequest,
            searchUsername
            );
        
        UnityWebRequest request = UnityWebRequest.Get(requestAddress);
        
        var operation = request.SendWebRequest();
        while (!operation.isDone) {}
        
        // Check for errors
        if (request.responseCode == 204)
        {
            responseText = "This user does not exist";
            loginResponse.SetText(responseText);
        }
        else
        {
            // Request successful, get the response
            responseText = request.downloadHandler.text;
            loginResponse.SetText("Welcome!");
            user.userID = responseText;
            scanScraps.gameObject.SetActive(true);
            // Process the response data here
        }
    }

    void StartScrapsProcessing()
    {
        accountUI.SetActive(false);
        initialMessage.SetActive(true);
    }
    void Start()
    {
        login.onClick.AddListener(ActivateLoginOption);
        createAccount.onClick.AddListener(CreateAccountOption);
        createAccountButtonTrigger.onClick.AddListener(CreateAccountTrigger);
        loginButtonTrigger.onClick.AddListener(LoginTrigger);
        scanScraps.onClick.AddListener(StartScrapsProcessing);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
