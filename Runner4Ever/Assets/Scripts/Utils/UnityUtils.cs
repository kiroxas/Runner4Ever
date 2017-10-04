using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class UnityUtils 
{
	static public void drawGizmoSquare(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight, Vector3 topLeft, Color color)
	{
		Debug.DrawLine(bottomLeft, topLeft, color);
		Debug.DrawLine(topLeft, topRight, color);
		Debug.DrawLine(topRight, bottomRight, color);
		Debug.DrawLine(bottomLeft, bottomRight, color);
	}
}