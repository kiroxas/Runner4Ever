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

        if(text == null)
        {
            Debug.LogError("Cannot find a text component for key : " + key + " on gameobject " + transform.name);
        }
        else
        {
            text.text = LocalizationManager.GetValue(key);
        }
    }
}