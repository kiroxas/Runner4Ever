using UnityEngine;

public class LevelSelectionMode : MonoBehaviour
{
    private GameConstants.Mode activeMode = GameConstants.Mode.Solo;

    public void setSolo()
    {
        activeMode = GameConstants.Mode.Solo;
    }

    public void setMulti()
    {
        activeMode = GameConstants.Mode.Multiplayer;
    }

    public GameConstants.Mode getActiveMode()
    {
        return activeMode;
    }

}