using UnityEngine;
using System.Collections;
using Lean.Touch;

public class CharacterController2D : MonoBehaviour
{
	public float runSpeed = 0.1f;
	public float jumpMagnitude = 0.1f;
	private float upSpeed = 0;

	public void LateUpdate()
	{
		transform.Translate(new Vector2(runSpeed, upSpeed), Space.World);
		upSpeed = 0;
	}

	protected virtual void OnEnable()
	{
			// Hook into the events we need
			LeanTouch.OnFingerTap   += OnFingerTap;
			LeanTouch.OnFingerSwipe += OnFingerSwipe;
	}
		
	protected virtual void OnDisable()
	{
			// Unhook the events
			
			LeanTouch.OnFingerTap   -= OnFingerTap;
			LeanTouch.OnFingerSwipe -= OnFingerSwipe;
	}

	public void OnFingerTap(LeanFinger finger)
	{
		upSpeed = jumpMagnitude;
	}

	public void OnFingerSwipe(LeanFinger finger)
	{		
				// Store the swipe delta in a temp variable
				var swipe = finger.SwipeScreenDelta;
			
				if (swipe.x < -Mathf.Abs(swipe.y)) // Left
				{
					runSpeed /= 2;
				}
			
				if (swipe.x > Mathf.Abs(swipe.y)) // Rigth
				{
					runSpeed *= 2;
				}
			
				if (swipe.y < -Mathf.Abs(swipe.x)) // Down
				{
					
				}
			
				if (swipe.y > Mathf.Abs(swipe.x)) // Up
				{

				}
	}

}