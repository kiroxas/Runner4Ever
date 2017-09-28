using UnityEngine;
using System.Collections;
using Lean.Touch;
using System;

/*
	Central class for controlling a character, handles actions and flow 
*/
public class CharacterController2D : MonoBehaviour
{

	//--------------------------------------- Enum declarations ---------------------------------------
	public enum JumpStrat
	{
		NormalJump,
		DoubleJump
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

	//--------------------------------------- Members ---------------------------------------

	// ------------------------------------------ Jump Related
 	public JumpCharacs jumpDefinition; // Shape, duration, speed of the first jump
 	public JumpCharacs doubleJumpDefinition;  // Shape, duration, speed of the second jump
 	public JumpStrat jumpStrategy = JumpStrat.DoubleJump; // the strategy about jump, can we double jump or simple jump
 	public JumpRestrictions jumpRes = JumpRestrictions.OnGround; // When can we jump
 	public JumpDirectionOnWallOrEdge jumpWall = JumpDirectionOnWallOrEdge.KeepTheSame; // when we are wall jumpin or edge jumping, in which direction do we jump
 	public Stack jumpWallStack; // stack to know which is the current strategy for JumpDirectionOnWallOrEdge
 	public Stack jumpState; // stack to know which is the current strategy for JumpRestrictions
 	public float timeBetweenJumps = 0.25f; // minimum time between 2 jumps
 	public float jumpBufferTime = 0.2f; // buffer time when we register a failed jump attempt

	private float lastJumpFailedAttempt = 0.0f; // private variable to register when was the last failed jump
	private float jumpIn; // variable to register when we last jumped
	private JumpCollection jumpCollec; // where we store the jumps, and it will manage the order and the proper end of each jump

	// ---------------------------------------- Health Related
	public int maxHealth = 10;
	private int health = 10;

	// ---------------------------------------- Other Components
	public CharacterState state; // State about if we hit ground/walls
	public Animator animator; // Animator for the character
	private Transform transform; // transform
	private Rigidbody2D rb; // rigidbody

	//---------------------------------------- Misc
	public RunDirectionOnGround runDir = RunDirectionOnGround.KeepTheAirOne;
	public Stack runDirStack;
	private bool facingRight = true;

	// RunSpeed Related
	private bool running = true;
	public float xSpeedPerFrame = 0.1f;
	public float gravityFactor = 0.2f;

	public float runSpeed = 0.1f;
	private float _runSpeedBeforeStop;
	private float _runSpeed, _actualSpeed, _lastSpeed;
	public float accelerationSmooth = 1.0f;

	public float dashSpeedMul = 2.5f;
	private float upSpeed = 0;
	private float gravityScale = 0;
	public Stack gravity;

	private float dashIn = 0.0f;
	public float dashTime = 1.0f;

	private float xColliderSize = 0.0f;
	private float yColliderSize = 0.0f;

	private Vector2 colliderOffset;

	/* ------------------------------------------------------ Monobehaviour Functions -------------------------------------------------------*/

	public void Awake()
	{
		if(jumpDefinition == null)
		{
			jumpDefinition = new JumpCharacs();
		}

		if(doubleJumpDefinition == null)
		{
			doubleJumpDefinition = new JumpCharacs();
		}

		jumpCollec = new JumpCollection();

		rb = GetComponent<Rigidbody2D>();
		transform = GetComponent<Transform>();
		state = GetComponent<CharacterState>();
		if(state == null)
		{
			Debug.LogError("You need a character State");
		}
		
	}

	public void Start()
	{
		jumpDefinition.init();
		doubleJumpDefinition.init();

		jumpDefinition.setDebugTransform(transform);
		doubleJumpDefinition.setDebugTransform(transform);

		jumpDefinition.setName("First Jump");
		doubleJumpDefinition.setName("Double Jump");

		jumpCollec.addJump(jumpDefinition);
		jumpCollec.addJump(doubleJumpDefinition);
		
		gravityScale = rb.gravityScale;
		jumpState = new Stack();
		runDirStack = new Stack();
		jumpWallStack = new Stack();
		gravity = new Stack();

		reinit();

		xColliderSize = GetComponent<BoxCollider2D>().size.x;
		yColliderSize = GetComponent<BoxCollider2D>().size.y;
		colliderOffset = GetComponent<BoxCollider2D>().offset;
		//changeMovementState();
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
		//putCorrectGravityScale();
		//updateSpeed();

		updateActionTimer(ref jumpIn);
		updateActionTimer(ref lastJumpFailedAttempt);
		if(updateActionTimer(ref dashIn))
		{
			GetComponent<BoxCollider2D>().size = new Vector2(xColliderSize, yColliderSize);
			GetComponent<BoxCollider2D>().offset = new Vector2(colliderOffset.x, colliderOffset.y);
		}

		// ------------------------------- Frame actions -------------------------------
		if(lastJumpFailedAttempt > 0.0f) // Time buffer, if we pressed jump not so long ago, and now we can jump, let's jump
		{
			if(canJump())
			{
				jump();
			}
		}

		bool jumped = upSpeed > 0;
		
		if(jumped)
		{
			upSpeed = 0;
		}

		if(wallSticking() && !isJumping())
		{
			jumpCollec.reset();
		}

		float xSpeed = shallNullifySpeed() ? 0.0f : _actualSpeed;
		
		// ----------------------- Jump part ------------------------------
		handleJump(jumped);

		if(isJumping())
		{
			Vector3 offset = jumpCollec.getNext();
			if(collidingForward()) // if colliding, move only on Y
			{
				offset.x = 0;

				if(offset.y <= 0)
				{
					offset.y = 0;
					jumpCollec.reset();
				}
			}

			if (collidingAbove() && offset.y > 0)
			{
				offset.y = 0;
			}

			transform.position += offset;
		}
		else
		{
			float xMoveForward = (collidingForward() || !running) ? 0.0f : areWeGoingRight() ? xSpeedPerFrame : -xSpeedPerFrame;
			float gravity = grounded() ? 0.0f : wallSticking() ? -(gravityFactor / 2.0f) : -gravityFactor;

			Vector3 offset = new Vector3(xMoveForward, gravity, 0.0f);

			transform.position += offset;
		}

		animator.SetBool("isRunning", running);
		animator.SetBool("isJumping", jumpIn > 0);
		animator.SetBool("isSliding", dashIn > 0);
		
	}
		
	/* ------------------------------------------------------ Functions -------------------------------------------------------*/
	
	public void handleJump(bool jumpedThisFrame)
	{
		if(jumpedThisFrame) // jumped this frame
		{
			jumpCollec.startJump(transform.position);
		}
		else if(isJumping() && jumpCollec.jumpEnded()) // in jump mode && our jump has ended
		{
			jumpCollec.reset();
		}

		// reset consecutive jumps
		if(grounded() && !jumpedThisFrame)
		{
			jumpCollec.reset();
		}
	}

	public void doDamage(int damage)
	{
		health -= damage;
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

	private void updateSpeed()
	{
		float percent = wallSticking() ? Time.deltaTime * accelerationSmooth : Time.deltaTime * accelerationSmooth;
		_actualSpeed = Mathf.Lerp(_actualSpeed, _runSpeed, percent);
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
		//consecutiveJumps = 1;
	}


	public void inverseVelocity(float magnitude, float maxX, float maxY)
	{
		inverseXVelocity(magnitude, maxX);
		inverseYVelocity(magnitude, maxY);
	}

	

	/* ------------------------------------------------------ Player's actions -------------------------------------------------------*/

	public void jump()
	{
		if(canJump())
		{		
			if((JumpDirectionOnWallOrEdge)jumpWallStack.Peek() == JumpDirectionOnWallOrEdge.Inverse && (grabingEdge() || wallSticking()))
			{
				changeDirection();
			}

			upSpeed = 10.0f;
			jumpIn = timeBetweenJumps;
			lastJumpFailedAttempt = 0.0f;
		}
		else
		{
			lastJumpFailedAttempt = jumpBufferTime;
		}
	}

	public void slide()
	{
	}

	public void accelerate()
	{
		_runSpeed *= 2;
	}

	public void decelerate()
	{
		_runSpeed /= 2;
	}

	public void run()
	{
		if(stopped())
		{
			running = true;
		}
	}

	public void stop()
	{
		running = false;
		/*_runSpeedBeforeStop = _runSpeed;
		_runSpeed = 0;
		_actualSpeed = 0;*/
	}

	public void dash()
	{	
		_actualSpeed *= dashSpeedMul;
		animator.SetBool("isSliding", true);
		dashIn = dashTime;
		GetComponent<BoxCollider2D>().size = new Vector2(xColliderSize, yColliderSize / 2);
		GetComponent<BoxCollider2D>().offset = new Vector2(colliderOffset.x, colliderOffset.y - (yColliderSize / 4));
	}

	/* ------------------------------------------------------ Function to inquiry the state -------------------------------------------------------*/

	public int getCurrentJumpCount()
	{
		return jumpCollec.getCurrentJumpIndex();
	}

	private int getMaxJumps()
	{
		return  jumpRes == JumpRestrictions.Anywhere ? 9999 : jumpStrategy == JumpStrat.NormalJump ? 1 : 2;
	}

	public float runspeed()
	{
		return _runSpeed;
	}

	public bool isJumping()
	{
		return jumpCollec.isJumping();
		//return movstate == MovementState.Transform;
	}

	public bool collidingAbove()
	{
		return state.isCollidingAbove;
	}

	public bool collidingForward()
	{
		return _runSpeed > 0 && state.isCollidingRight || _runSpeed < 0 && state.isCollidingLeft;
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
		return state.isGrounded;
	}

	public bool areWeGoingRight()
	{
		return _runSpeed >= 0;
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

	public bool stopped()
	{
		return !running ;
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

	/* ------------------------------------------------------ Function about the direction of the character -------------------------------------------------------*/
	
	public void changeDirection()
	{
		Flip(); // display
		jumpDefinition.flip();
		doubleJumpDefinition.flip();

		if(stopped())
		{
			run();
		}

		_runSpeed *= -1;
	}

	private void makeItRunRightOnGround()
	{

		if( 
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
		_actualSpeed = 0;
		jumpCollec.reset();
		running = true;
		//movstate = MovementState.Rigidbody;

		runDirStack.Clear();
		runDirStack.Push(runDir);

		jumpState.Clear();
		jumpState.Push(jumpRes);

		jumpWallStack.Clear();
		jumpWallStack.Push(jumpWall);

		gravity.Clear();
		gravity.Push(gravityScale);
		health = maxHealth;
	}

	public bool flipped()
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

	/* ------------------------------------------------------ Editor functions -------------------------------------------------------*/

	void OnDrawGizmosSelected()
	{
		if(jumpDefinition != null)
		{
			Gizmos.color = Color.blue;
			jumpDefinition.OnDrawGizmosSelected();
			Gizmos.color = Color.green;

			doubleJumpDefinition.OnDrawGizmosSelected(jumpDefinition.getHighestPoint() + jumpDefinition.getDebugPosition());
		}
	}

}