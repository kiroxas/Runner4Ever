using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class UnityUtils 
{
	public enum CollisionDirection
	{
		Above,
		Below,
		Left,
		Right,
		Nowhere
	}

	static public void drawGizmoSquare(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight, Vector3 topLeft, Color color)
	{
		Debug.DrawLine(bottomLeft, topLeft, color);
		Debug.DrawLine(topLeft, topRight, color);
		Debug.DrawLine(topRight, bottomRight, color);
		Debug.DrawLine(bottomLeft, bottomRight, color);
	}

	static public int getFPS()
	{
		return (int)(1.0f / Time.deltaTime);
	}

	static public CollisionDirection getCollisionDirection(Bounds rectangleHitbox, Vector2 hitpoint)
	{
		if(Mathf.Approximately(hitpoint.y, rectangleHitbox.max.y))  
        {
        	return CollisionDirection.Above;
        }
        else if(Mathf.Approximately(hitpoint.y, rectangleHitbox.min.y))  // below
        {
        	return CollisionDirection.Below;
        }
        else if(Mathf.Approximately(hitpoint.x, rectangleHitbox.min.x)) // left
        {
        	return CollisionDirection.Left;
        }
        else if(Mathf.Approximately(hitpoint.x, rectangleHitbox.max.x))  // right
        {
        	return CollisionDirection.Right;
        }
        else
        {
        	return CollisionDirection.Nowhere;
        }
	}
}