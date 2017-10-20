using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour 
{
    public string key;

    void OnEnable()
    {
    	EventManager.StartListening (EventManager.get().languageChangedEvent, changeText);
    	changeText(new GameConstants.LanguageChangedArgument(PlayerData.instance.lang));
    }

    void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().languageChangedEvent, changeText);
    }

    void changeText( GameConstants.LanguageChangedArgument arg)
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