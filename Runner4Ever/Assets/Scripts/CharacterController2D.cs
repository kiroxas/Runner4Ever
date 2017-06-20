using UnityEngine;
using System.Collections;
using Lean.Touch;

public class CharacterController2D : MonoBehaviour
{
	public enum Action
	{
		None,
		Jump,
		Accelerate,
		Decelerate,
		Dash,
		Slide
	}

	public Action onTap;
	public Action onSwipeLeft;
	public Action onSwipeRight;
	public Action onSwipeDown;
	public Action onSwipeUp;
	public Action onDoubleTap;


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
		if(finger.TapCount == 1)
			doAction(onTap);
		else if(finger.TapCount == 2)
			doAction(onDoubleTap);
	}

	public void doAction(Action action)
	{
		switch(action)
		{
			case Action.Jump : jump(); return;
			case Action.Accelerate : accelerate(); return;
			case Action.Decelerate : decelerate(); return;
			case Action.Dash : dash(); return;
			case Action.Slide : slide(); return;
			default : return;
		}
	}

	public void dash()
	{

	}

	public void slide()
	{

	}

	public void jump()
	{
		upSpeed = jumpMagnitude;
	}

	public void accelerate()
	{
		runSpeed *= 2;
	}

	public void decelerate()
	{
		runSpeed /= 2;
	}

	public void OnFingerSwipe(LeanFinger finger)
	{		
		// Store the swipe delta in a temp variable
		var swipe = finger.SwipeScreenDelta;
			
		if (swipe.x < -Mathf.Abs(swipe.y)) // Left
		{
			doAction(onSwipeLeft);
		}
			
		if (swipe.x > Mathf.Abs(swipe.y)) // Rigth
		{
			doAction(onSwipeRight);
		}
			
		if (swipe.y < -Mathf.Abs(swipe.x)) // Down
		{
			doAction(onSwipeDown);		
		}
			
		if (swipe.y > Mathf.Abs(swipe.x)) // Up
		{
			doAction(onSwipeUp);
		}
	}

}