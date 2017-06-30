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

	public Action onTap;
	public Action onSwipeLeft;
	public Action onSwipeRight;
	public Action onSwipeDown;
	public Action onSwipeUp;
	public Action onDoubleTap;
	public Action onHoldDown;
	public Action onHoldUp;

	public Action onRightCollision;
	public Animator animator;

	private Rigidbody2D rb;

	public float runSpeed = 0.1f;
	private float _runSpeedBeforeStop;
	private float _runSpeed, _lastSpeed;
	public float jumpMagnitude = 0.1f;
	private float upSpeed = 0;

	public float yRightColDetDelta = 0.02f;

	public void Start()
	{
		_runSpeed = runSpeed;
		rb = GetComponent<Rigidbody2D>();
	}

	public void LateUpdate()
	{
		if(_lastSpeed != _runSpeed && animator)
		{
			bool isRunning = _runSpeed > 0;
			
			animator.SetBool("isRunning", isRunning);
			_lastSpeed = _runSpeed;
		}

		float step = 0.01f;

		if(upSpeed > 0)
		{
			rb.velocity = new Vector2(_runSpeed, upSpeed);
			upSpeed = 0;
		}
		else
		{
			rb.velocity = new Vector2(_runSpeed, rb.velocity.y);
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
		doAction(onHoldDown);
	}

	public void OnHoldUp(LeanFinger finger)
	{
		doAction(onHoldUp);
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
            		doAction(onRightCollision);
            		return;
            	}

        	}
    	}
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