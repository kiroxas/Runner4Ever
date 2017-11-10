using UnityEngine;

public class LevelSelectionUI : MonoBehaviour
{
    public RectTransform scrollPlace;
    public GameObject levelZonePrefab;
    public LevelSelectionMode mode; 

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

    public void populateList()
    {
        foreach (Transform t in scrollPlace)
            Destroy(t.gameObject);

        var files = mode.getActiveMode() == GameConstants.Mode.Solo ? GameFlow.instance.levels.files : GameFlow.instance.multiLevels.files;

        foreach(string levelName in files)
        {
            GameObject entry = Instantiate(levelZonePrefab);

            entry.GetComponent<Transform>().SetParent(scrollPlace, false);
            LevelEntry level = entry.GetComponent<LevelEntry>();
            level.setName(levelName);
        }     
    }
   
    public void Open()
    {
        gameObject.SetActive(true);

        populateList();
    }

   
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
