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
    	EventManager.StartListening (GameConstants.orientationChangedEvent, changeImage);
    }

    void OnDisable ()
    {
        EventManager.StopListening (GameConstants.orientationChangedEvent, changeImage);
    }

    void changeImage()
    {
        if(DeviceUtils.getDeviceType() != DeviceUtils.Device.Mobile)
            return;
            
    	Image image = GetComponent<Image> ();
        bool isPortrait = DeviceUtils.isPortrait(GameFlow.get().getOrientation());

        image.sprite = isPortrait ? portrait : landscape;
    }

    void Start()
    {
        changeImage();
    }
}