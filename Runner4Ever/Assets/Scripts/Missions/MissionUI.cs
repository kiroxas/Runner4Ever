using UnityEngine;


public class MissionUI : MonoBehaviour
{
    public RectTransform missionPlace;
    public GameObject missionEntryPrefab;
    //public AdsForMission addMissionButtonPrefab;

    public void Open()
    {
        gameObject.SetActive(true);

        foreach (Transform t in missionPlace)
            Destroy(t.gameObject);

        for(int i = 0; i < 3; ++i)
        {
            if (PlayerData.instance.missions.Count > i)
            {
                GameObject entry = Instantiate(missionEntryPrefab);

                entry.GetComponent<Transform>().SetParent(missionPlace, false);
                MissionEntry mission = entry.GetComponent<MissionEntry>();

                mission.FillWithMission(PlayerData.instance.missions[i], this);
            }
           /* else
            {
                AdsForMission obj = Instantiate(addMissionButtonPrefab);
                obj.missionUI = this;
                obj.transform.SetParent(missionPlace, false);
            }*/
        }
    }

    public void Claim(MissionBase m)
    {
        PlayerData.instance.ClaimMission(m);

        // Rebuild the UI with the new missions
        Open();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
