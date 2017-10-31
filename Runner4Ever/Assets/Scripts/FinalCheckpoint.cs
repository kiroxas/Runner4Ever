using UnityEngine;
using UnityEngine.UI;

public class FinalCheckpoint : MonoBehaviour
{
    public void OnTriggerEnterCustom(RaycastCollision other)
    {
      if(other.other.GetComponent<CharacterController2D>() == null)
        return;

       EventManager.TriggerEvent( EventManager.get().hitFinalCheckpointEvent, new GameConstants.HitFinalCheckpointArgument(gameObject));
    }
}