using UnityEngine;
using UnityEngine.UI;

public class Checkpoint : MonoBehaviour
{
    public void OnTriggerEnterCustom(RaycastCollision other)
    {
      if(other.other.GetComponent<CharacterController2D>() == null)
        return;

       EventManager.TriggerEvent( EventManager.get().hitCheckpointEvent, new GameConstants.HitCheckpointArgument(gameObject));
    }
}