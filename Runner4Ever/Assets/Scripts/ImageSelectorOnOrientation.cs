using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSelectorOnOrientation : MonoBehaviour 
{
    public Sprite portrait;
    public Sprite landscape;

    void OnEnable()
    {
    	EventManager.StartListening (EventManager.get().orientationChangedEvent, changeImage);
    }

    void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().orientationChangedEvent, changeImage);
    }

    void changeImage(GameConstants.OrientationChangedArgument arg)
    {
        if(DeviceUtils.getDeviceType() != DeviceUtils.Device.Mobile)
            return;
            
    	Image image = GetComponent<Image> ();
        bool isPortrait = DeviceUtils.isPortrait(arg.orientation);

        image.sprite = isPortrait ? portrait : landscape;
    }

    void Start()
    {
        changeImage(new GameConstants.OrientationChangedArgument(GameFlow.get().getOrientation()));
    }
}