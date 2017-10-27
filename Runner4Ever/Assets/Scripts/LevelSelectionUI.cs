using UnityEngine;


public class LevelSelectionUI : MonoBehaviour
{
    public RectTransform scrollPlace;
    public MissionEntry levelZonePrefab;
    //public AdsForMission addMissionButtonPrefab;

    public void Open()
    {
        gameObject.SetActive(true);

        foreach (Transform t in scrollPlace)
            Destroy(t.gameObject);

        
    }

   
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
