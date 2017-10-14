using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizationManager : MonoBehaviour 
{
    private static LocalizationManager instance;

    private Dictionary<string, string> localizedText;
    private bool isReady = false;
    private string missingTextString = "Localized text not found";

    // Use this for initialization
    void Awake () 
    {
        if (instance == null) 
        {
            instance = this;
        } 
        else if (instance != this)
        {
            Destroy (gameObject);
        }

        DontDestroyOnLoad (gameObject);
    }

    static public LocalizationManager get()
    {
        return instance;
    }

    static public string GetValue(string key)
    {
        return get().GetLocalizedValue(key);
    }

    
    public void LoadLocalizedText(string fileName)
    {
        localizedText = new Dictionary<string, string> ();
        TextAsset data = Resources.Load(fileName) as TextAsset;
        //string filePath = Path.Combine (Application.streamingAssetsPath, fileName);

        if (data) 
        {
            string dataAsJson = data.text;
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData> (dataAsJson);

            for (int i = 0; i < loadedData.items.Length; i++) 
            {
                localizedText.Add (loadedData.items [i].key, loadedData.items [i].value);   
            }

            Debug.Log ("Data loaded, dictionary contains: " + localizedText.Count + " entries");
        } 
        else 
        {
            Debug.LogError ("Cannot find file : " + fileName);
        }

        isReady = true;
    }

    public string GetLocalizedValue(string key)
    {
        string result = missingTextString;
        if (localizedText.ContainsKey (key)) 
        {
            result = localizedText [key];
        }

        return result;

    }

    public bool GetIsReady()
    {
        return isReady;
    }

}