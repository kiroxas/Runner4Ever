using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

/*
	Class that handles input, and triggers actions
*/
public class GameFlow : MonoBehaviour
{
	static protected GameFlow s_Instance;
	static public GameFlow instance { get { return s_Instance; } }

    private ScreenOrientation orientation;

 	void Awake()
    {
    	if (s_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        s_Instance = this;
        
    	DontDestroyOnLoad(gameObject);

        PlayerData.Create();
        setLang(PlayerData.get().lang);
        orientation = Screen.orientation;
    }

    static public GameFlow get()
    {
        return instance;
    }

    public ScreenOrientation getOrientation()
    {
        return orientation;
    }

	public void LoadLevel(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void LoadMainGame()
    {
    	LoadLevel("MainGame");
    }

    public void LoadMainMenu()
    {
        LoadLevel("MainMenu");
    }

    public void Update()
    {
        PlayerData.instance.UpdateMissions(TrackingManager.get());
        updateOrientation();
    }

    public void updateOrientation()
    {
        if(DeviceUtils.getDeviceType() == DeviceUtils.Device.Mobile && orientation != Screen.orientation)
        {
            orientation = Screen.orientation;
            EventManager.TriggerEvent( EventManager.get().orientationChangedEvent, new GameConstants.OrientationChangedArgument(orientation));
        }
    }

    public void setLang(LocalizationUtils.Languages lang)
    {
        string locPath = LocalizationUtils.getLocFilePath(lang);
        LocalizationManager.get().LoadLocalizedText(locPath);

        if(PlayerData.get().lang != lang)
        {
            PlayerData.get().lang = lang;
            PlayerData.get().Save();
        }

        EventManager.TriggerEvent(EventManager.get().languageChangedEvent, new GameConstants.LanguageChangedArgument(lang));
    }
}