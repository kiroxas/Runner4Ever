using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class DeviceUtils  
{
	public enum Device
	{
		Mobile,
		Desktop,
		Console,
		Unknown
	}

	static public Device getDeviceType()
	{
		switch(SystemInfo.deviceType)
		{
			case DeviceType.Handheld : return Device.Mobile;
			case DeviceType.Desktop : return Device.Desktop;
			case DeviceType.Console : return Device.Console;		
			default : return Device.Unknown;
		}
	}

	static public bool isPortrait(ScreenOrientation s)
	{
		return s == ScreenOrientation.Portrait || s == ScreenOrientation.PortraitUpsideDown;
	}

	static public bool isLandscape(ScreenOrientation s)
	{
		return s == ScreenOrientation.LandscapeLeft || s == ScreenOrientation.LandscapeRight;
	}
}