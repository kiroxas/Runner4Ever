using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class LanguagesButton : MonoBehaviour 
{
    public LocalizationUtils.Languages lang = LocalizationUtils.Languages.en;

    public void setLang(LocalizationUtils.Languages l)
    {
    	lang = l;

    	string spritePath = LocalizationUtils.getFlagSpritePath(lang);

    	GetComponent<Image>().sprite = Resources.Load<Sprite>(spritePath);
    	GameFlow.instance.setLang(lang);
    }

    public void OnCLick()
    {
    	setLang(LocalizationUtils.GetNext(lang));
    }

    public void Start()
    {
    	setLang(lang);
    }
}

