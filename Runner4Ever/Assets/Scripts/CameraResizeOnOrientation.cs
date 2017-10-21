using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraResizeOnOrientation : MonoBehaviour 
{
    public float portraitSize;
    public float landscapeSize;
    public float smoothing = 3.0f;

    private float currentSize;
    private float aimedSize;

    //public UnityAction<GameConstants.OrientationChangedEvent> myCallback;

    void OnEnable()
    {
    	EventManager.StartListening( EventManager.get().orientationChangedEvent, changeSize);
    }

    void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().orientationChangedEvent, changeSize);
    }

    void changeSize(GameConstants.OrientationChangedArgument arg)
    {
        if(DeviceUtils.getDeviceType() != DeviceUtils.Device.Mobile)
            return;
            
        bool isPortrait = DeviceUtils.isPortrait(arg.orientation);

        aimedSize = isPortrait ? portraitSize : landscapeSize;
    }

    void Start()
    {
        if(DeviceUtils.getDeviceType() == DeviceUtils.Device.Mobile)
        {
            currentSize = GetComponent<Camera> ().orthographicSize;
            changeSize(new GameConstants.OrientationChangedArgument(GameFlow.get().getOrientation()));
        }
    }

    void Update()
    {
        if(DeviceUtils.getDeviceType() == DeviceUtils.Device.Mobile && aimedSize != currentSize)
        {
            currentSize = Mathf.Lerp(currentSize, aimedSize, Time.deltaTime * smoothing);
            GetComponent<Camera>().orthographicSize = currentSize;
        }
    }
}