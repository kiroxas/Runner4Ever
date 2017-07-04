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
		Slide,
		Stop,
		Start
	}

	public enum JumpRestrictions
	{
		Anywhere,
		OnGround,
		Never
	}

	public Action onTap;
	public Action onSwipeLeft;
	public Action onSwipeRight;
	public Action onSwipeDown;
	public Action onSwipeUp;
	public Action onDoubleTap;
	public Action onHoldDown;
	public Action onHoldUp;
	public Action onRightCollision;

	public Action onTapGrounded;
	public Action onSwipeLeftGrounded;
	public Action onSwipeRightGrounded;
	public Action onSwipeDownGrounded;
	public Action onSwipeUpGrounded;
	public Action onDoubleTapGrounded;
	public Action onHoldDownGrounded;
	public Action onHoldUpGrounded;
	public Action onRightCollisionGrounded;

	public CharacterState state;

	public JumpRestrictions jumpRes = JumpRestrictions.OnGround;
	
	public Animator animator;

	private Rigidbody2D rb;

	public float runSpeed = 0.1f;
	private float _runSpeedBeforeStop;
	private float _runSpeed, _lastSpeed;
	public float jumpMagnitude = 0.1f;
	private float upSpeed = 0;

	public bool collidingRight()
	{
		return state.isCollidingRight;
	}

	public bool grabingEdge()
	{
		return state.isGrabingEdge;
	}

	public bool grounded()
	{
		return state.isGrounded;
	}

	public float runspeed()
	{
		return _runSpeed;
	}

	public void Start()
	{
		_runSpeed = runSpeed;
		rb = GetComponent<Rigidbody2D>();
	}

	private void lockYPosition()
	{
		rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
	}

	private void unlockYPosition()
	{
		rb.constraints = RigidbodyConstraints2D.FreezeRotation;
	}


	public void LateUpdate()
	{
		state.updateState();

		/*if(isCollidingRight)
        {
        	doAction(isGrounded ? onRightCollisionGrounded : onRightCollision); 
        }*/

		float xSpeed = collidingRight() ? 0.0f : _runSpeed;
		float yVelocity = rb.velocity.y;

        //rb.constraints = RigidbodyConstraints2D.FreezeRotation;

		if(upSpeed > 0)
		{
			yVelocity = upSpeed;
			upSpeed = 0;
			unlockYPosition();
		}
		else if (grabingEdge())
		{
			yVelocity = 0;
			lockYPosition();
		}

		rb.velocity = new Vector2(xSpeed, yVelocity);

		if(_lastSpeed != xSpeed && animator)
		{
			bool isRunning = xSpeed > 0;
			
			animator.SetBool("isRunning", isRunning);
			_lastSpeed = xSpeed;
		}

		if(grounded())
		{
			Collider2D myCollider = GetComponent<Collider2D>();
			Vector2 point = new Vector2(myCollider.bounds.center.x, myCollider.bounds.center.y + 2);
			Debug.DrawLine(myCollider.bounds.center, point, Color.black, 20);
		}
		
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

	public void OnHoldDown(LeanFinger finger)
	{
		state.updateGrounded();
		Action action = grounded() ? onHoldDownGrounded : onHoldDown;
		doAction(action);
	}

	public void OnHoldUp(LeanFinger finger)
	{
		state.updateGrounded();
		Action action = grounded() ? onHoldUpGrounded : onHoldUp;
		doAction(action);
	}

	public void OnFingerTap(LeanFinger finger)
	{
		state.updateGrounded();
		
		if(finger.TapCount == 1)
		{
			doAction(grounded() ? onTapGrounded : onTap);
		}
		else if(finger.TapCount == 2)
		{
			doAction(grounded() ? onDoubleTapGrounded : onDoubleTap);
		}
	}

	public void doAction(Action action)
	{
		if(_runSpeed == 0) // Stopped
		{
			run();
		}

		switch(action)
		{
			case Action.Jump : jump(); return;
			case Action.Accelerate : accelerate(); return;
			case Action.Decelerate : decelerate(); return;
			case Action.Dash : dash(); return;
			case Action.Slide : slide(); return;
			case Action.Start : run(); return;
			case Action.Stop : stop(); return;
			default : return;
		}
	}

	public void run()
	{
		_runSpeed = _runSpeedBeforeStop;
	}

	public void stop()
	{
		_runSpeedBeforeStop = _runSpeed;
		_runSpeed = 0;
	}

	public void dash()
	{
	}

	public void slide()
	{
	}

	public void jump()
	{
		if(grabingEdge() == false)
		{
		switch(jumpRes)
		{
			case JumpRestrictions.OnGround :
				state.updateGrounded();
				if(!grounded())
				{
					return;
				}
				break;
			case JumpRestrictions.Never : return;
			case JumpRestrictions.Anywhere : break;
		}
		}

		upSpeed = jumpMagnitude;
	}

	public void accelerate()
	{
		_runSpeed *= 2;
	}

	public void decelerate()
	{
		_runSpeed /= 2;
	}


	public void OnFingerSwipe(LeanFinger finger)
	{		
		// Store the swipe delta in a temp variable
		var swipe = finger.SwipeScreenDelta;
		state.updateGrounded();
			
		if (swipe.x < -Mathf.Abs(swipe.y)) // Left
		{
			doAction(grounded() ? onSwipeLeftGrounded : onSwipeLeft);
		}
			
		if (swipe.x > Mathf.Abs(swipe.y)) // Rigth
		{
			doAction(grounded() ? onSwipeRightGrounded :onSwipeRight);
		}
			
		if (swipe.y < -Mathf.Abs(swipe.x)) // Down
		{
			doAction(grounded() ? onSwipeDownGrounded :onSwipeDown);		
		}
			
		if (swipe.y > Mathf.Abs(swipe.x)) // Up
		{
			doAction(grounded() ? onSwipeUpGrounded : onSwipeUp);
		}
	}

}