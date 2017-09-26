using UnityEngine;
using System.Collections;
using Lean.Touch;
using System;

public class CharacterController2D : MonoBehaviour
{
	public enum MovementState
	{
		Rigidbody,
		Transform
	}	

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

	public enum JumpStrat
	{
		NormalJump,
		DoubleJump
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

	public enum JumpDirectionOnWallOrEdge
	{
		KeepTheSame,
		Inverse
	}

	public enum JumpRestrictions
	{
		Anywhere,
		OnGround,
		Never
	}

	public enum RunDirectionOnGround
	{
		AlwaysRight,
		AlwaysLeft,
		KeepTheAirOne
	}

	[Serializable]
 	public class Row
 	{
     	public Action[] action = new Action[System.Enum.GetNames(typeof(Inputs)).Length];
 	}

 	public JumpCharacs charc;

	static public int states = 3;
	public int airBorn = 0;
	public int groundedIndex = 1;
	public int groundedAndStopped = 2;

	public Row[] actions = new Row[states];

	private CharacterState state;
	public JumpStrat jumpStrategy = JumpStrat.DoubleJump;
	private int consecutiveJumps = 0;

	public JumpRestrictions jumpRes = JumpRestrictions.OnGround;
	
	public RunDirectionOnGround runDir = RunDirectionOnGround.KeepTheAirOne;
	public JumpDirectionOnWallOrEdge jumpWall = JumpDirectionOnWallOrEdge.KeepTheSame;

	public Stack jumpWallStack;
	public Stack runDirStack;
	public Stack jumpState;

	public Animator animator;

	private Transform transform;
	private Rigidbody2D rb;
	private bool facingRight = true;

	// RunSpeed Related
	public float runSpeed = 0.1f;
	private float _runSpeedBeforeStop;
	private float _runSpeed, _actualSpeed, _lastSpeed;
	public float accelerationSmooth = 1.0f;

	public float speedBonusOnJump = 0.0f;
	public float dashSpeedMul = 2.5f;
	public float jumpMagnitude = 0.1f;
	public float highJumpMagnitude = 0.2f;
	private float upSpeed = 0;
	private float gravityScale = 0;
	public Stack gravity;
	public float timeBetweenJumps = 0.25f;
	private float jumpIn = 0.0f;
	private float lastJumpFailedAttempt = 0.0f;
	public float jumpBufferTime = 0.2f;
	public int maxHealth = 10;
	private int health = 10;

	private float dashIn = 0.0f;
	public float dashTime = 1.0f;

	private float xColliderSize = 0.0f;
	private float yColliderSize = 0.0f;

	private Vector2 colliderOffset;

	public TouchZone touchzone = TouchZone.EntireScreen;

	public MovementState movstate;

	bool isJumping()
	{
		return movstate == MovementState.Transform;
	}

	void changeMovementState()
	{
		if(movstate == MovementState.Rigidbody)
		{
			rb.velocity = new Vector2(0.0f, 0.0f); // moving it manually
			charc.startJump(transform.position);
			movstate = MovementState.Transform;
		}
		else
		{
			charc.endJump();
			movstate = MovementState.Rigidbody;
		}
	}

	public int getCurrentJumpCount()
	{
		return consecutiveJumps;
	}

	public void doDamage(int damage)
	{
		health -= damage;
	}

	private int getMaxJumps()
	{
		return  jumpRes == JumpRestrictions.Anywhere ? 9999 : jumpStrategy == JumpStrat.NormalJump ? 1 : 2;
	}

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
		return state.isGrounded || wallSticking();
	}

	public float runspeed()
	{
		return _runSpeed;
	}

	public void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		state = GetComponent<CharacterState>();
		transform = GetComponent<Transform>();
		charc = GetComponent<JumpCharacs>();
	}

	public void Start()
	{
		movstate = MovementState.Rigidbody;
		health = maxHealth;
		_runSpeed = runSpeed;
		_actualSpeed = 0;
		
		gravityScale = rb.gravityScale;
		jumpState = new Stack();
		jumpState.Push(jumpRes);

		runDirStack = new Stack();
		runDirStack.Push(runDir);

		jumpWallStack = new Stack();
		jumpWallStack.Push(jumpWall);

		gravity = new Stack();
		gravity.Push(gravityScale);

		xColliderSize = GetComponent<BoxCollider2D>().size.x;
		yColliderSize = GetComponent<BoxCollider2D>().size.y;
		colliderOffset = GetComponent<BoxCollider2D>().offset;
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
		//rb.gravityScale = rb.velocity.y < 0 ? (float)gravity.Peek() : gravityScale; // change gravity only when falling (for now)
		rb.gravityScale = (float)gravity.Peek();
	}

	private bool shallNullifySpeed()
	{
		bool answer =  (_runSpeed > 0 && _actualSpeed > 0 && collidingRight()) || (_runSpeed < 0 && _actualSpeed < 0 && collidingLeft());

		return answer;
	}

	public bool isDead()
	{
		return health <= 0;
	}

	public void respawn(float x, float y)
	{
		if(!isDead())
		{
			Debug.Log("respawning a non dead character");
		}
		reinit();

		health = maxHealth;
		transform.position = new Vector2(x, y); 
	}


	public void LateUpdate()
	{
		if(isDead())
		{
			return;
		}

		// ------------ Update all states and variables -------------------------------
		state.updateState();
		makeItRunRightOnGround();
		putCorrectGravityScale();
		updateSpeed();

		updateActionTimer(ref jumpIn);
		updateActionTimer(ref lastJumpFailedAttempt);
		if(updateActionTimer(ref dashIn))
		{
			GetComponent<BoxCollider2D>().size = new Vector2(xColliderSize, yColliderSize);
			GetComponent<BoxCollider2D>().offset = new Vector2(colliderOffset.x, colliderOffset.y);
		}

		// ------------------------------- Frame actions -------------------------------
		bool jumped = upSpeed > 0;
		float yVelocity = rb.velocity.y;

		if(jumped)
		{
			yVelocity = upSpeed;
			_actualSpeed += _actualSpeed < 0 ? -speedBonusOnJump : speedBonusOnJump;
			
			upSpeed = 0;
			unlockYPosition();
		}
		else if (grabingEdge())
		{
			yVelocity = 0;
			lockYPosition();
		}
		else if(lastJumpFailedAttempt > 0.0f) // Time buffer, if we pressed jump not so long ago, and now we can jump, let's jump
		{
			if(canJump())
			{
				jump(selectJumpMagnitude());
			}
		}

		float xSpeed = shallNullifySpeed() ? 0.0f : _actualSpeed;
		
		if(jumped && !isJumping())
		{
			/*if(charc.jumpEnded() == false)
			{
				changeMovementState();	
			}*/

			changeMovementState();
		}

		if(isJumping() && charc.jumpEnded() == false && grounded())
		{
			changeMovementState();
		}
		else if(isJumping() && charc.jumpEnded())
		{
			changeMovementState();
		}

		if(isJumping())
		{
			transform.position += charc.getNext();
		}
		else
		{
			rb.velocity = new Vector2(xSpeed, yVelocity);
		}

		if(_lastSpeed != xSpeed && animator)
		{
			bool isRunning = xSpeed != 0;
			
			animator.SetBool("isRunning", isRunning);
			_lastSpeed = xSpeed;
		}

		if(grounded() && !jumped)
		{
			consecutiveJumps = 0;
		}


		animator.SetBool("isJumping", jumpIn > 0);
		animator.SetBool("isSliding", dashIn > 0);
		
	}



	protected virtual void OnEnable()
	{
			// Hook into the events we need
			LeanTouch.OnFingerTap   += OnFingerTap;
			LeanTouch.OnFingerSwipe += OnFingerSwipe;

			LeanFingerHeld.OnFingerHeldDown += OnHoldDown;
			LeanFingerHeld.OnFingerHeldUp += OnHoldUp;
	}

	private bool updateActionTimer(ref float variable)
	{
		if(variable > 0)
		{
			variable -= Time.deltaTime;
			if(variable < 0)
			{
				variable = 0;
				return true;
			}
		}

		return false;
	}
		
	protected virtual void OnDisable()
	{
			// Unhook the events
			
			LeanTouch.OnFingerTap   -= OnFingerTap;
			LeanTouch.OnFingerSwipe -= OnFingerSwipe;

			LeanFingerHeld.OnFingerHeldDown -= OnHoldDown;
			LeanFingerHeld.OnFingerHeldUp -= OnHoldUp;
	}

	private void updateSpeed()
	{
		float percent = wallSticking() ? Time.deltaTime * accelerationSmooth : Time.deltaTime * accelerationSmooth;
		_actualSpeed = Mathf.Lerp(_actualSpeed, _runSpeed, percent);
	}

	private bool stopped()
	{
		return _runSpeed == 0 ;
	}

	private bool isItForMe(LeanFinger finger)
	{
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

		int index = grounded() ? (stopped() ? groundedAndStopped : groundedIndex ) : airBorn;
		Action action = actions[index].action[(int)Inputs.Hold];
		doAction(action);
	}

	public void OnHoldUp(LeanFinger finger)
	{
		if(!isItForMe(finger))
		{
			return;
		}

		int index = grounded() ? (stopped() ? groundedAndStopped : groundedIndex ) : airBorn;
		Action action = actions[index].action[(int)Inputs.HoldUp];
		doAction(action);
	}

	public void OnFingerTap(LeanFinger finger)
	{
		if(!isItForMe(finger))
		{
			return;
		}

		int index = grounded() ? (stopped() ? groundedAndStopped : groundedIndex ) : airBorn;
		
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

	float selectJumpMagnitude()
	{
		return getCurrentJumpCount() == 0 ? jumpMagnitude : highJumpMagnitude;
	}

	public void doAction(Action action)
	{
		if(stopped()) // Stopped
		{
			run();
		}

		switch(action)
		{
			case Action.Jump : jump(selectJumpMagnitude()); break;
			case Action.Accelerate : accelerate(); break;
			case Action.Decelerate : decelerate(); break;
			case Action.Dash : dash(); break;
			case Action.Slide : slide(); break;
			case Action.StartSame : run(); break;
			case Action.StartOpposite : run(); changeDirection(); break;
			case Action.Stop : stop(); break;
			default : break;
		}
	}

	public void run()
	{
		if(stopped())
		{
			_runSpeed = _runSpeedBeforeStop;
		}
	}

	public void stop()
	{
		_runSpeedBeforeStop = _runSpeed;
		_runSpeed = 0;
		_actualSpeed = 0;
	}

	public void dash()
	{	
		_actualSpeed *= dashSpeedMul;
		animator.SetBool("isSliding", true);
		dashIn = dashTime;
		GetComponent<BoxCollider2D>().size = new Vector2(xColliderSize, yColliderSize / 2);
		GetComponent<BoxCollider2D>().offset = new Vector2(colliderOffset.x, colliderOffset.y - (yColliderSize / 4));
	}

	public void inverseXVelocity(float magnitude, float max)
	{
		float speed = _actualSpeed == 0 ? _runSpeedBeforeStop : _actualSpeed;
		_actualSpeed = speed * -1 * magnitude;
		
		_actualSpeed = Mathf.Clamp(_actualSpeed, -max, max);

		rb.velocity = new Vector2(_actualSpeed, rb.velocity.y);
		run();
	}

	public void inverseYVelocity(float magnitude, float max)
	{
		float ySpeed =  rb.velocity.y * -1 * magnitude;

		ySpeed = Mathf.Clamp(ySpeed, -max, max);

		rb.velocity = new Vector2(rb.velocity.x, ySpeed);
		run();
		consecutiveJumps = 1;
	}


	public void inverseVelocity(float magnitude, float maxX, float maxY)
	{
		inverseXVelocity(magnitude, maxX);
		inverseYVelocity(magnitude, maxY);
	}

	public void slide()
	{
	}

	public bool canJump()
	{
		JumpRestrictions jump = (JumpRestrictions)jumpState.Peek();
		if(jump == JumpRestrictions.Anywhere)
		{
			return true;
		}
		else if(jump == JumpRestrictions.Never)
		{
			return false;
		}


		if(getMaxJumps() <= getCurrentJumpCount())
		{
			return false;
		}
		else
		{
			return true;
		}

		return true;
	}

	public void jump(float magnitude)
	{
		if(canJump())
		{		
			if((JumpDirectionOnWallOrEdge)jumpWallStack.Peek() == JumpDirectionOnWallOrEdge.Inverse && (grabingEdge() || wallSticking()))
			{
				changeDirection();
			}

			consecutiveJumps++;
			upSpeed = magnitude;
			jumpIn = timeBetweenJumps;
			lastJumpFailedAttempt = 0.0f;
		}
		else
		{
			lastJumpFailedAttempt = jumpBufferTime;
		}
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
		if(!isItForMe(finger))
		{
			return;
		}
		// Store the swipe delta in a temp variable
		var swipe = finger.SwipeScreenDelta;

		int index = grounded() ? (stopped() ? groundedAndStopped : groundedIndex ) : airBorn;
		
			
		if (swipe.x < -Mathf.Abs(swipe.y)) // Left
		{
			bool goingRight = _runSpeed >= 0;
			if(index == groundedAndStopped)
			{
				doAction(flipped() ? Action.StartSame : Action.StartOpposite);
			}
			else
			{
				Action action = actions[index].action[goingRight ? (int)Inputs.SwipeOppDir : (int)Inputs.SwipeSameDir];
				doAction(action);
			}
		}
			
		if (swipe.x > Mathf.Abs(swipe.y)) // Rigth
		{
			bool goingRight = _runSpeed >= 0;
			if(index == groundedAndStopped)
			{
				doAction(flipped() ? Action.StartOpposite : Action.StartSame);
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

	private void changeDirection()
	{
		Flip(); // display

		if(stopped())
		{
			run();
		}

		_runSpeed *= -1;
	}

	private void makeItRunRightOnGround()
	{

		if( !wallSticking() &&
			((RunDirectionOnGround)runDirStack.Peek() == RunDirectionOnGround.AlwaysRight && grounded() && _runSpeed < 0 )
		|| ((RunDirectionOnGround)runDirStack.Peek() == RunDirectionOnGround.AlwaysLeft && grounded() && _runSpeed > 0 ))
		{
			changeDirection();
		}
	}

	public void reinit()
	{
		if(transform.localScale.x == -1)
			Flip();

		_runSpeed = runSpeed;
	}

	private bool flipped()
	{
		return transform.localScale.x < 0 ;
	}

	private void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

}