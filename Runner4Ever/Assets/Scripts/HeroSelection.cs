using UnityEngine;

public class HeroSelection : MonoBehaviour
{
    public void Open()
    {
        GameFlow.get().LoadMiniGame();
    }

}