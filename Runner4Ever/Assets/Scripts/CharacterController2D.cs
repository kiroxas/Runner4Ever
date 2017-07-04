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

	public enum JumpDirectionOnWallOrEdge
	{
		KeepTheSame,
		Inverse
	}

	public enum RunDirectionOnGround
	{
		AlwaysRight,
		AlwaysLeft,
		KeepTheAirOne
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

	private CharacterState state;

	public JumpRestrictions jumpRes = JumpRestrictions.OnGround;
	public JumpDirectionOnWallOrEdge jumpWall = JumpDirectionOnWallOrEdge.KeepTheSame;
	public RunDirectionOnGround runDir = RunDirectionOnGround.KeepTheAirOne;

	public Stack jumpState;

	public Animator animator;

	private Rigidbody2D rb;
	private bool facingRight = true;

	public float runSpeed = 0.1f;
	private float _runSpeedBeforeStop;
	private float _runSpeed, _lastSpeed;
	public float jumpMagnitude = 0.1f;
	private float upSpeed = 0;
	private float gravityScale = 0;
	public float timeBetweenJumps = 0.25f;
	private float jumpIn = 0.0f;

	public bool collidingRight()
	{
		return state.isCollidingRight;
	}

	public bool collidingLeft()
	{
		return state.isCollidingLeft;
	}

	public bool collidingSide()
	{
		return state.isCollidingSide;
	}

	public bool grabingEdge()
	{
		return state.isGrabingEdge;
	}

	public bool wallSticking()
	{
		return state.isWallSticking;
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
		state = GetComponent<CharacterState>();
		gravityScale = rb.gravityScale;
		jumpState = new Stack();
		jumpState.Push(jumpRes);
	}

	private void lockYPosition()
	{
		rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
	}

	private void unlockYPosition()
	{
		rb.constraints = RigidbodyConstraints2D.FreezeRotation;
	}

	private void putCorrectGravityScale()
	{
		bool conditions = rb.velocity.y < 0 && wallSticking();
		rb.gravityScale = conditions ? 0.25f : gravityScale;
	}

	private bool shallNullifySpeed()
	{
		return (_runSpeed > 0 && collidingRight()) || (_runSpeed < 0 && collidingLeft());
	}


	public void LateUpdate()
	{
		state.updateState();
		makeItRunRightOnGround();
		putCorrectGravityScale();

		float xSpeed = shallNullifySpeed() ? 0.0f : _runSpeed;
		float yVelocity = rb.velocity.y;

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
			bool isRunning = xSpeed != 0;
			
			animator.SetBool("isRunning", isRunning);
			_lastSpeed = xSpeed;
		}

		if(grounded())
		{
			Collider2D myCollider = GetComponent<Collider2D>();
			Vector2 point = new Vector2(myCollider.bounds.center.x, myCollider.bounds.center.y + 2);
			Debug.DrawLine(myCollider.bounds.center, point, Color.black, 20);
		}

		updateJumpIn();
		
	}

	protected virtual void OnEnable()
	{
			// Hook into the events we need
			LeanTouch.OnFingerTap   += OnFingerTap;
			LeanTouch.OnFingerSwipe += OnFingerSwipe;

			LeanFingerHeld.OnFingerHeldDown += OnHoldDown;
			LeanFingerHeld.OnFingerHeldUp += OnHoldUp;
	}

	private void updateJumpIn()
	{
		if(jumpIn > 0)
		{
			jumpIn -= Time.deltaTime;
			if(jumpIn < 0)
			{
				jumpIn = 0;
			}
		}
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
		if(jumpIn > 0)
		{
			return;
		}

		if(grabingEdge() == false)
		{
		switch((JumpRestrictions)jumpState.Peek())
		{
			case JumpRestrictions.OnGround :
				state.updateState();
				if(!grounded() && !wallSticking())
				{
					return;
				}
				break;
			case JumpRestrictions.Never : return;
			case JumpRestrictions.Anywhere : break;
		}
		}

		if(jumpWall == JumpDirectionOnWallOrEdge.Inverse && (grabingEdge() || wallSticking()))
		{
			changeDirection();
		}

		upSpeed = jumpMagnitude;
		jumpIn = timeBetweenJumps;
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

	private void changeDirection()
	{
		Flip(); // display

		_runSpeed *= -1;
	}

	private void makeItRunRightOnGround()
	{
		if((runDir == RunDirectionOnGround.AlwaysRight && grounded() && _runSpeed < 0)
		|| (runDir == RunDirectionOnGround.AlwaysLeft && grounded() && _runSpeed > 0))
		{
			changeDirection();
		}
	}

	private void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

}