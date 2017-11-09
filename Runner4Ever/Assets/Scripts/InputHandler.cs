using UnityEngine;
using System.Collections;
using Lean.Touch;
using System;

/*
	Class that handles input, and triggers actions
*/
public class InputHandler : MonoBehaviour
{
	// --------------------------------- Enum definitions ---------------------------------
	public enum TouchZone
	{
		EntireScreen,
		LeftHalf,
		RightHalf
	}

	public enum Action
	{
		None,
		Jump,
		Accelerate,
		Decelerate,
		Dash,
		Slide,
		Stop,
		StartSame,
		StartOpposite
	}

	public enum Inputs
	{
		Tap,
		DoubleTap,
		SwipeSameDir,
		SwipeOppDir,
		SwipeUp,
		SwipeDown,
		Hold,
		HoldUp
	}

	// --------------------------------- Helper class ---------------------------------
	[Serializable]
 	public class Row
 	{
     	public Action[] action = new Action[System.Enum.GetNames(typeof(Inputs)).Length];
 	}

 	// --------------------------------- Members ---------------------------------
	static public int states = 3; // Number of states in actions
	public int airBorn = 0; // index for airborn
	public int groundedIndex = 1; // index for grounded
	public int groundedAndStopped = 2; // index for grounded and stopped
	public TouchZone touchzone = TouchZone.EntireScreen; // zone where we handle the input
 	public Row[] actions = new Row[states]; // all the actions for every state and input

 	private CharacterController2D character; // reference to the character

 	// --------------------------------- Functions ---------------------------------

 	public void Awake()
	{
		character = GetComponent<CharacterController2D>();
	}

 	protected virtual void OnEnable()
	{
		// Hook into the events we need
		LeanTouch.OnFingerTap   += OnFingerTap;
		LeanTouch.OnFingerSwipe += OnFingerSwipe;

		LeanFingerHeld.OnFingerHeldDown += OnHoldDown;
		LeanFingerHeld.OnFingerHeldUp += OnHoldUp;
	}

	protected virtual void OnDisable()
	{
		// Unhook the events
			
		LeanTouch.OnFingerTap   -= OnFingerTap;
		LeanTouch.OnFingerSwipe -= OnFingerSwipe;

		LeanFingerHeld.OnFingerHeldDown -= OnHoldDown;
		LeanFingerHeld.OnFingerHeldUp -= OnHoldUp;
	}

	private bool isItForMe(LeanFinger finger)
	{
		if(character.amILocalPlayer() == false)
		{
			return false;
		}
		
		Vector2 originPos = finger.StartScreenPosition;

		switch(touchzone)
		{
			case TouchZone.EntireScreen : return true;
			case TouchZone.LeftHalf : return originPos.x < Screen.width / 2f;
			case TouchZone.RightHalf : return originPos.x > Screen.width / 2f;
		}

		return true;
	}

	public void OnHoldDown(LeanFinger finger)
	{
		if(!isItForMe(finger))
		{
			return;
		}

		int index = character.grounded() ? (character.stopped() ? groundedAndStopped : groundedIndex ) : airBorn;
		Action action = actions[index].action[(int)Inputs.Hold];
		doAction(action);
	}

	public void OnHoldUp(LeanFinger finger)
	{
		if(!isItForMe(finger))
		{
			return;
		}

		int index = character.grounded() ? (character.stopped() ? groundedAndStopped : groundedIndex ) : airBorn;
		Action action = actions[index].action[(int)Inputs.HoldUp];
		doAction(action);
	}

	public void OnFingerTap(LeanFinger finger)
	{
		if(!isItForMe(finger))
		{
			return;
		}

		int index = character.grounded() ? (character.stopped() ? groundedAndStopped : groundedIndex ) : airBorn;
		
		if(finger.TapCount == 1)
		{
			Action action = actions[index].action[(int)Inputs.Tap];
			doAction(action);
		}
		else if(finger.TapCount > 1)
		{
			Action action = actions[index].action[(int)Inputs.DoubleTap];
			doAction(action);
		}
	}

	public void OnFingerSwipe(LeanFinger finger)
	{	
		if(!isItForMe(finger))
		{
			return;
		}
		// Store the swipe delta in a temp variable
		var swipe = finger.SwipeScreenDelta;

		int index = character.grounded() ? (character.stopped() ? groundedAndStopped : groundedIndex ) : airBorn;
			
		if (swipe.x < -Mathf.Abs(swipe.y)) // Left
		{
			bool goingRight = character.areWeGoingRight();
			if(index == groundedAndStopped)
			{
				doAction(character.flipped() ? Action.StartSame : Action.StartOpposite);
			}
			else
			{
				Action action = actions[index].action[goingRight ? (int)Inputs.SwipeOppDir : (int)Inputs.SwipeSameDir];
				doAction(action);
			}
		}
			
		if (swipe.x > Mathf.Abs(swipe.y)) // Rigth
		{
			bool goingRight = character.areWeGoingRight();
			if(index == groundedAndStopped)
			{
				doAction(character.flipped() ? Action.StartOpposite : Action.StartSame);
			}
			else
			{
				Action action = actions[index].action[goingRight ? (int)Inputs.SwipeSameDir : (int)Inputs.SwipeOppDir];
				doAction(action);
			}
		}
			
		if (swipe.y < -Mathf.Abs(swipe.x)) // Down
		{
			Action action = actions[index].action[(int)Inputs.SwipeDown];
			doAction(action);		
		}
			
		if (swipe.y > Mathf.Abs(swipe.x)) // Up
		{
			Action action = actions[index].action[(int)Inputs.SwipeUp];
			doAction(action);
		}
	}


	public void doAction(Action action)
	{
		if(character.stopped()) // Stopped
		{
			character.run();
		}

		switch(action)
		{
			case Action.Jump : character.jump(); break;
			case Action.Accelerate : character.accelerate(); break;
			case Action.Decelerate : character.decelerate(); break;
			case Action.Dash : character.dash(); break;
			case Action.Slide : character.slide(); break;
			case Action.StartSame : character.run(); break;
			case Action.StartOpposite : character.run(); character.changeDirection(); break;
			case Action.Stop : character.stop(); break;
			default : break;
		}
	}
}