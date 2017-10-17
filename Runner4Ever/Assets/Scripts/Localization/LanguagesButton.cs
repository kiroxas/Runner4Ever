using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class LanguagesButton : MonoBehaviour 
{
	public enum Languages
	{
		en, // english
		fr  // french
	}

	static Languages GetNext(Languages value)
	{
    	return (from Languages val in System.Enum.GetValues(typeof (Languages)) 
            where val > value 
            orderby val 
            select val).DefaultIfEmpty().First();
	}

    public Languages lang = Languages.en;

    public void setLang(Languages l)
    {
    	lang = l;

    	string spritePath = "UI/" + lang.ToString();
    	string locPath = "Localization/" + lang.ToString();

    	GetComponent<Image>().sprite = Resources.Load<Sprite>(spritePath);
    	LocalizationManager.get().LoadLocalizedText(locPath);

    	EventManager.TriggerEvent (GameConstants.languageChangedEvent);
    }

    public void OnCLick()
    {
    	Languages l = GetNext(lang);

    	setLang(l);
    }

    public void Start()
    {
    	setLang(lang);
    }
}

