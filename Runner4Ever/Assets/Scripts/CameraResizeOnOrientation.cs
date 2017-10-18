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

    void OnEnable()
    {
    	EventManager.StartListening (GameConstants.orientationChangedEvent, changeSize);
    }

    void OnDisable ()
    {
        EventManager.StopListening (GameConstants.orientationChangedEvent, changeSize);
    }

    void changeSize()
    {
        if(DeviceUtils.getDeviceType() != DeviceUtils.Device.Mobile)
            return;
            
    	Camera cam = GetComponent<Camera> ();
        bool isPortrait = DeviceUtils.isPortrait(GameFlow.get().getOrientation());

       aimedSize = isPortrait ? portraitSize : landscapeSize;
    }

    void Start()
    {
        currentSize = GetComponent<Camera> ().orthographicSize;
        changeSize();
    }

    void Update()
    {
        if(aimedSize != currentSize)
        {
            currentSize = Mathf.Lerp(currentSize, aimedSize, Time.deltaTime * smoothing);
            GetComponent<Camera>().orthographicSize = currentSize;
        }
    }
}