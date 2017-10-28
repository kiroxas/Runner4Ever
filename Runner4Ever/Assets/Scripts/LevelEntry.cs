using UnityEngine;
using UnityEngine.UI;

public class LevelEntry : MonoBehaviour
{
   private string levelName;

   public void setName(string s)
   {
        levelName = s;

        GetComponentInChildren<Text>().text = levelName;
   }

   public void onClick()
   {
        EventManager.TriggerEvent( EventManager.get().levelSelectedEvent, new GameConstants.LevelSelectedArgument(levelName));
   }
}
