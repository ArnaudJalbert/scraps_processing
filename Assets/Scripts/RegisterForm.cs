using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class RegisterForm : MonoBehaviour
{
    List<string> textileTypes = new List<string>
    {
        "Unknown",
        "Cashmere",
        "Chenille",
        "Chiffon",
        "Cotton",
        "Leather",
        "Linen",
        "Merino Wool",
        "Suede",
        "Spandex",
        "Polyester",
        "Nylon",
        "Acrylic",
        "Polyethylene",
        "Polypropylene",
        "Rayon",
        "Canvas"
    };


    // Start is called before the first frame update
    void Start()
    {
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