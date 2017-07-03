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

	public JumpRestrictions jumpRes = JumpRestrictions.OnGround;
	public Animator animator;

	private Rigidbody2D rb;

	public float runSpeed = 0.1f;
	private float _runSpeedBeforeStop;
	private float _runSpeed, _lastSpeed;
	public float jumpMagnitude = 0.1f;
	private float upSpeed = 0;

	public float yRightColDetDelta = 0.02f;

	public float groundedCastDistance = 0.01f;
	private bool isGrounded = false;
	public int groundedRayCasts = 8;
	public LayerMask PlatformMask;

	public void Start()
	{
		_runSpeed = runSpeed;
		rb = GetComponent<Rigidbody2D>();
	}

	public bool updateGrounded()
	{
		/* // Method 1, cast the rigid body
		int colliderHitted = rb.Cast(new Vector2(0, -1), hits, groundedCastDistance);

		isGrounded = colliderHitted > 0;
		
		*/

		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.x / (float)groundedRayCasts;
		isGrounded = false;

		Vector2 rayDirection = Vector2.down;
		for(int i = 0; i < groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + i * step, myCollider.bounds.min.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				isGrounded = true;
				break;
			}
		}

		return isGrounded;
	}

	public void LateUpdate()
	{
		if(_lastSpeed != _runSpeed && animator)
		{
			bool isRunning = _runSpeed > 0;
			
			animator.SetBool("isRunning", isRunning);
			_lastSpeed = _runSpeed;
		}

		if(upSpeed > 0)
		{
			rb.velocity = new Vector2(_runSpeed, upSpeed);
			upSpeed = 0;
		}
		else
		{
			rb.velocity = new Vector2(_runSpeed, rb.velocity.y);
		}

		updateGrounded();

		if(isGrounded)
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
		updateGrounded();
		Action action = isGrounded ? onHoldDownGrounded : onHoldDown;
		doAction(action);
	}

	public void OnHoldUp(LeanFinger finger)
	{
		updateGrounded();
		Action action = isGrounded ? onHoldUpGrounded : onHoldUp;
		doAction(action);
	}

	public void OnFingerTap(LeanFinger finger)
	{
		updateGrounded();
		
		if(finger.TapCount == 1)
		{
			doAction(isGrounded ? onTapGrounded : onTap);
		}
		else if(finger.TapCount == 2)
		{
			doAction(isGrounded ? onDoubleTapGrounded : onDoubleTap);
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
		switch(jumpRes)
		{
			case JumpRestrictions.OnGround :
				updateGrounded();
				if(!isGrounded)
				{
					return;
				}
				break;
			case JumpRestrictions.Never : return;
			case JumpRestrictions.Anywhere : break;
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

	//public void OnTriggerEnter2D(Collider2D other) 
	//{
      // stop();
   // }


    public void OnCollisionEnter2D(Collision2D collision) 
    {
    	if(collision.gameObject.tag == "Platform")
    	{
    		Collider2D myCollider = GetComponent<Collider2D>();
    		Bounds character = myCollider.bounds;
    		Debug.DrawLine(character.center, character.min, Color.blue, 20);
    		Debug.DrawLine(character.center, character.max, Color.grey, 20);

    		foreach (ContactPoint2D contacts in collision.contacts) 
    		{
    			Collider2D otherCollider = collision.otherCollider == myCollider ? collision.collider : collision.otherCollider;
    			Bounds center = otherCollider.bounds;

    			Debug.DrawLine(character.center, center.center, Color.red, 20);	
    			Debug.DrawLine(center.center, center.min, Color.blue, 20);
    			Debug.DrawLine(center.center, center.max, Color.grey, 20);
    		
				bool xCondition = contacts.point.x >= character.max.x;
				bool yCondition = contacts.point.y - yRightColDetDelta> character.min.y && contacts.point.y + yRightColDetDelta< character.max.y;

           		if(xCondition && yCondition)
           	 	{
           	 		updateGrounded();

            		doAction(isGrounded ? onRightCollisionGrounded : onRightCollision);
            		return;
            	}

        	}
    	}
    }


	public void OnFingerSwipe(LeanFinger finger)
	{		
		// Store the swipe delta in a temp variable
		var swipe = finger.SwipeScreenDelta;
		updateGrounded();
			
		if (swipe.x < -Mathf.Abs(swipe.y)) // Left
		{
			doAction(isGrounded ? onSwipeLeftGrounded : onSwipeLeft);
		}
			
		if (swipe.x > Mathf.Abs(swipe.y)) // Rigth
		{
			doAction(isGrounded ? onSwipeRightGrounded :onSwipeRight);
		}
			
		if (swipe.y < -Mathf.Abs(swipe.x)) // Down
		{
			doAction(isGrounded ? onSwipeDownGrounded :onSwipeDown);		
		}
			
		if (swipe.y > Mathf.Abs(swipe.x)) // Up
		{
			doAction(isGrounded ? onSwipeUpGrounded : onSwipeUp);
		}
	}

}