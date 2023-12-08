using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class RegisterForm : MonoBehaviour
{
    private List<string> textileTypes;

    void GetTextileTypes()
    {
        string requestAddress = "https://scraps-processing-api-delicate-pond-5077.fly.dev/textile-types";
        string data;
        UnityWebRequest request = UnityWebRequest.Get(requestAddress);
        var operation = request.SendWebRequest();
        while (!operation.isDone) {}
        
        // Check for errors
        if (request.responseCode == 204)
        {
            data = "could not retrieve this data";
        }
        else
        {
            // Request successful, get the response
            data = request.downloadHandler.text;
            // Removing square brackets and splitting the string by comma and space to get individual elements
            Debug.Log(data);
            string[] elements = data.Trim('[', ']').Trim('"', '"').Split(new[] { "\", \"" }, StringSplitOptions.None);
            textileTypes = new List<string>(elements);
            // Process the response data here
        }
        
    }



    // Start is called before the first frame update
    void Start()
    {
        GetTextileTypes();
        var dropdown = GetComponent<TMP_Dropdown>();

        dropdown.options.Clear();

        foreach (var textileType in textileTypes)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(textileType.ToLower()));
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}