using UnityEngine;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Save data for the game. This is stored locally in this case, but a "better" way to do it would be to store it on a server
/// somewhere to avoid player tampering with it. Here potentially a player could modify the binary file to add premium currency.
/// </summary>
public class PlayerData
{
    static protected PlayerData m_Instance;
    static public PlayerData instance { get { return m_Instance; } }

    protected string saveFile = "";

    public List<string> characters = new List<string>();    // Inventory of characters owned.
    public int usedCharacter;                               // Currently equipped character.

    public List<string> themes = new List<string>();                // Owned themes.
    public int usedTheme;                                           // Currently used theme.

    public List<MissionBase> missions = new List<MissionBase>();

	public string previousName = "Kiro";
    public LocalizationUtils.Languages lang;

    public bool licenceAccepted;

	public float masterVolume = float.MinValue, musicVolume = float.MinValue, masterSFXVolume = float.MinValue;

    // This will allow us to add data even after production, and so keep all existing save STILL valid. See loading & saving for how it work.
    // Note in a real production it would probably reset that to 1 before release (as all dev save don't have to be compatible w/ final product)
    // Then would increment again with every subsequent patches. We kept it to its dev value here for teaching purpose. 
    static int s_Version = 2; 

    // version 2, Added language

    public void AddCharacter(string name)
    {
        characters.Add(name);
    }

    public void AddTheme(string theme)
    {
        themes.Add(theme);
    }

    // Mission management

    // Will add missions until we reach 2 missions.
    public void CheckMissionsCount()
    {
        while (missions.Count < 2)
            AddMission();
    }

    public void AddMission()
    {
        int val = Random.Range(0, (int)MissionBase.MissionType.MAX);
        
        MissionBase newMission = MissionBase.GetNewMissionFromType((MissionBase.MissionType)val);
        newMission.Created();

        missions.Add(newMission);
    }

    public void StartRunMissions(TrackingManager manager)
    {
        for(int i = 0; i < missions.Count; ++i)
        {
            missions[i].RunStart(manager);
        }
    }

    public void UpdateMissions(TrackingManager manager)
    {
        for(int i = 0; i < missions.Count; ++i)
        {
            missions[i].Update(manager);
        }
    }

    public bool AnyMissionComplete()
    {
        for (int i = 0; i < missions.Count; ++i)
        {
            if (missions[i].isComplete) return true;
        }

        return false;
    }

    public void ClaimMission(MissionBase mission)
    {
        // TODO here, add reward if any

        missions.Remove(mission);

        CheckMissionsCount();

        Save();
    }

    static public PlayerData get()
    {
         return instance;
    }

    // File management

    static public void Create()
    {
		if (m_Instance == null)
		{
			m_Instance = new PlayerData();
		}

        m_Instance.saveFile = Application.persistentDataPath + "/save.bin";

        if (File.Exists(m_Instance.saveFile))
        {
            // If we have a save, we read it.
            m_Instance.Read();
        }
        else
        {
            // If not we create one with default data.
			NewSave();
        }

        m_Instance.CheckMissionsCount();
    }

	static public void NewSave()
	{
		m_Instance.characters.Clear();
		m_Instance.themes.Clear();
		m_Instance.missions.Clear();

		m_Instance.usedCharacter = 0;
		m_Instance.usedTheme = 0;

		m_Instance.characters.Add(GameConstants.defaultCharac);
		m_Instance.themes.Add(GameConstants.defaultTheme);

        m_Instance.lang = LocalizationUtils.getDeviceLanguage();

        m_Instance.CheckMissionsCount();

		m_Instance.Save();
	}

    public void Read()
    {
        BinaryReader r = new BinaryReader(new FileStream(saveFile, FileMode.Open));

        int ver = r.ReadInt32();

		if(ver < s_Version)
		{
			r.Close();

			NewSave();
			r = new BinaryReader(new FileStream(saveFile, FileMode.Open));
			ver = r.ReadInt32();
		}

        // Read character.
        characters.Clear();
        int charCount = r.ReadInt32();
        for(int i = 0; i < charCount; ++i)
        {
            characters.Add(r.ReadString());
        }

        usedCharacter = r.ReadInt32();

        // Read Themes.
        themes.Clear();
        int themeCount = r.ReadInt32();
        for (int i = 0; i < themeCount; ++i)
        {
            themes.Add(r.ReadString());
        }

        usedTheme = r.ReadInt32();

       
        missions.Clear();

        int count = r.ReadInt32();
        for(int i = 0; i < count; ++i)
        {
            MissionBase.MissionType type = (MissionBase.MissionType)r.ReadInt32();
            MissionBase tempMission = MissionBase.GetNewMissionFromType(type);

            tempMission.Deserialize(r);

            if (tempMission != null)
            {
                missions.Add(tempMission);
            }
        }
        
		previousName = r.ReadString();
        licenceAccepted = r.ReadBoolean();
        
		masterVolume = r.ReadSingle ();
		musicVolume = r.ReadSingle ();
		masterSFXVolume = r.ReadSingle ();

        // v2

        lang = (LocalizationUtils.Languages)r.ReadInt32();

        Debug.Log("Reading lang : " + lang);

        r.Close();
    }

    public void Save()
    {
        BinaryWriter w = new BinaryWriter(new FileStream(saveFile, FileMode.OpenOrCreate));

        w.Write(s_Version);

        // Write characters.
        w.Write(characters.Count);
        foreach (string c in characters)
        {
            w.Write(c);
        }

        w.Write(usedCharacter);

        // Write themes.
        w.Write(themes.Count);
        foreach (string t in themes)
        {
            w.Write(t);
        }

        w.Write(usedTheme);

        // Write missions.
        w.Write(missions.Count);
        for(int i = 0; i < missions.Count; ++i)
        {
            w.Write((int)missions[i].GetMissionType());
            missions[i].Serialize(w);
        }

		// Write name.
		w.Write(previousName);

        w.Write(licenceAccepted);

		w.Write (masterVolume);
		w.Write (musicVolume);
		w.Write (masterSFXVolume);

        // ver 2

        w.Write((int)lang);

        Debug.Log("Writing lang : " + lang);

        w.Close();
    }


}

// Helper class to cheat in the editor for test purpose
#if UNITY_EDITOR
public class PlayerDataEditor : Editor
{
	[MenuItem("Trash Dash Debug/Clear Save")]
    static public void ClearSave()
    {
        File.Delete(Application.persistentDataPath + "/save.bin");
    }

    /*[MenuItem("Trash Dash Debug/Give 1000000 fishbones and 1000 premium")]
    static public void GiveCoins()
    {
       
    }

    [MenuItem("Trash Dash Debug/Give 10 Consumables of each types")]
    static public void AddConsumables()
    {
       
        
    }*/
}
#endif