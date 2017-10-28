using UnityEngine;


public class LevelSelectionUI : MonoBehaviour
{
    public RectTransform scrollPlace;
    public GameObject levelZonePrefab;

    void OnEnable()
    {
        EventManager.StartListening(EventManager.get().levelSelectedEvent, onLevelSelected);
    }

    void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().levelSelectedEvent, onLevelSelected);
    }

    public void onLevelSelected(GameConstants.LevelSelectedArgument arg)
    {
        Close();
    }
   
    public void Open()
    {
        gameObject.SetActive(true);

        foreach (Transform t in scrollPlace)
            Destroy(t.gameObject);

        foreach(string levelName in  GameFlow.instance.levels.files)
        {
            GameObject entry = Instantiate(levelZonePrefab);

            entry.GetComponent<Transform>().SetParent(scrollPlace, false);
            LevelEntry level = entry.GetComponent<LevelEntry>();
            level.setName(levelName);
        }     
    }

   
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
