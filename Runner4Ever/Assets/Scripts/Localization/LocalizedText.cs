using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour 
{
    public string key;

    void OnEnable()
    {
    	EventManager.StartListening (GameConstants.languageChangedEvent, changeText);
    	changeText();
    }

    void OnDisable ()
    {
        EventManager.StopListening (GameConstants.languageChangedEvent, changeText);
    }

    void changeText()
    {
    	Text text = GetComponent<Text> ();
        text.text = LocalizationManager.GetValue(key);
    }
}