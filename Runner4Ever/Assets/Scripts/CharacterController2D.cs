using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using System;

public class ItsAlmostAStack<T, Index>
{
    private List<T> items = new List<T>();
    private List<Index> indexes = new List<Index>();

    public void Push(T item, Index index)
    {
        items.Add(item);
        indexes.Add(index);
    }

    public T Pop()
    {
        if (items.Count > 0)
        {
            T temp = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            indexes.RemoveAt(indexes.Count - 1);
            return temp;
        }
        else
            return default(T);
    }

    public T Peek()
    {
    	return items[items.Count - 1];
    }

    public void Clear()
    {
    	items.Clear();
    	indexes.Clear();
    }

    public void Remove(Index itemAtPosition)
    {
    	int ind = indexes.IndexOf(itemAtPosition);

    	if(ind == -1)
    	{
    		Debug.LogError("Unknown index");
    	}
    	else
    	{
        	items.RemoveAt(ind);
        	indexes.RemoveAt(ind);
    	}
    }
}

/*
	Central class for controlling a character, handles actions and flow 
*/
public class CharacterController2D : NetworkBehaviour
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

	public enum WallJumpStrategy
	{
		Infinite,
		Normal
	}

	//--------------------------------------- Members ---------------------------------------

	// ------------------------------------------ Jump Related
 	public JumpCharacs jumpDefinition; // Shape, duration, speed of the first jump
 	public JumpCharacs doubleJumpDefinition;  // Shape, duration, speed of the second jump
 	public JumpStrat jumpStrategy = JumpStrat.DoubleJump; // the strategy about jump, can we double jump or simple jump
 	public JumpRestrictions jumpRes = JumpRestrictions.OnGround; // When can we jump
 	public JumpDirectionOnWallOrEdge jumpWall = JumpDirectionOnWallOrEdge.KeepTheSame; // when we are wall jumpin or edge jumping, in which direction do we jump
 	public ItsAlmostAStack<JumpDirectionOnWallOrEdge, GameObject> jumpWallStack; // stack to know which is the current strategy for JumpDirectionOnWallOrEdge
 	public ItsAlmostAStack<JumpRestrictions, GameObject> jumpState; // stack to know which is the current strategy for JumpRestrictions
 	public float timeBetweenJumps = 0.25f; // minimum time between 2 jumps
 	public float jumpBufferTime = 0.2f; // buffer time when we register a failed jump attempt

 	[SyncVar]
	private float lastJumpFailedAttempt = 0.0f; // private variable to register when was the last failed jump

	[SyncVar]
	private float jumpIn; // variable to register when we last jumped
	private JumpCollection jumpCollec; // where we store the jumps, and it will manage the order and the proper end of each jump
	public bool firstJumpInAirEnabled = true;
	private bool wasJumpingLastFrame = false;

	// ---------------------------------------- Health Related
	public int maxHealth = 10;
	private int health = 10;

	// ---------------------------------------- Other Components
	public CharacterState state; // State about if we hit ground/walls
	public InnerState previousFrameState; // State about if we hit ground/walls
	public Animator animator; // Animator for the character
	private Transform characTransform; // characTransform
	private Rigidbody2D rb; // rigidbody
	private Active active;

	//---------------------------------------- Misc
	public RunDirectionOnGround runDir = RunDirectionOnGround.KeepTheAirOne;
	public ItsAlmostAStack<RunDirectionOnGround, GameObject> runDirStack;
	private bool facingRight = true;
	private bool canBeRepelled = true;
	//private float timeBetweenRepelledAgain = 0.1f;

	// RunSpeed Related
	private float totalDistanceRun = 0.0f;
	private bool running = true;
	private float currentVelocity;
	public ItsAlmostAStack<float, GameObject> maxVelocity;
	private float yVelocity;
	public float xSpeedBySecond = 0.1f;
	public float gravityFactor = 0.2f;
	public float momenutmHangWalljumpTime = 0.3f;
	public float timeHanging = 0.0f;
	private float currentGravity;
	private Vector2 outsideForce;

	public float accelerationSmooth = 1.0f;
	public float gravitySmooth = 1.0f;

	public float dashSpeedMul = 2.5f;

	[SyncVar]
	private float upSpeed = 0;
	public ItsAlmostAStack<float, GameObject> gravity;

	private float dashIn = 0.0f;
	public float dashTime = 1.0f;
	public bool nullifyGravityOnDash = false;

	private float xColliderSize = 0.0f;
	private float yColliderSize = 0.0f;

	private Vector2 colliderOffset;
	private int objectsCollected = 0;

	private WallJumpStrategy baseStrategy = WallJumpStrategy.Normal;
	public ItsAlmostAStack<WallJumpStrategy, GameObject> wallJumpStrat;

	public bool networkGame = false;

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
		characTransform = GetComponent<Transform>();
		state = GetComponent<CharacterState>();
		active = GetComponent<Active>();

		if(state == null)
		{
			Debug.LogError("You need a character State");
		}

		xColliderSize = GetComponent<BoxCollider2D>().size.x;
		yColliderSize = GetComponent<BoxCollider2D>().size.y;
		colliderOffset = GetComponent<BoxCollider2D>().offset;
		
	}

	public void Start()
	{
		jumpDefinition.init();
		doubleJumpDefinition.init();

		jumpDefinition.setDebugTransform(characTransform);
		doubleJumpDefinition.setDebugTransform(characTransform);

		jumpDefinition.setName("First Jump");
		doubleJumpDefinition.setName("Double Jump");

		jumpCollec.addJump(jumpDefinition);
		jumpCollec.addJump(doubleJumpDefinition);
		
		jumpState = new ItsAlmostAStack<JumpRestrictions, GameObject>();
		runDirStack = new ItsAlmostAStack<RunDirectionOnGround, GameObject>();
		jumpWallStack = new ItsAlmostAStack<JumpDirectionOnWallOrEdge, GameObject>();
		gravity = new ItsAlmostAStack<float, GameObject>();
		maxVelocity = new ItsAlmostAStack<float, GameObject>();
		wallJumpStrat = new ItsAlmostAStack<WallJumpStrategy, GameObject>();

		networkGame = UnityUtils.isNetworkGame();

		reinit();
	}

	public bool firstWallJumpCollision()
	{
		return canJump() && !jumpCollec.cantJumpReachedMaxJumps() && ((previousFrameState.isWallSticking == false && wallSticking()) || (previousFrameState.isWallSticking && wasJumpingLastFrame && wallSticking() && isJumping() == false));
	}

	public bool amIActive()
	{
		return active.isActive();
	}

	public void Update()
	{
		if(!amIActive() || isDead())
		{
			return;
		}

		canBeRepelledEnable();

		// ------------ Update all states and variables -------------------------------
		state.updateState();
		makeItRunRightOnGround();
		//putCorrectGravityScale();

		updateActionTimer(ref timeHanging);
		updateActionTimer(ref jumpIn);
		updateActionTimer(ref lastJumpFailedAttempt);
		if(updateActionTimer(ref dashIn))
		{
			float upDistance = yColliderSize / 2.0f;
			if(collidingAbove() || state.isThisColliding(Vector2.up, ref upDistance)) // if still colliding at end of slide, keep sliding
			{
				dashIn = dashTime / 2.0f;
			}
			else
			{
				GetComponent<BoxCollider2D>().size = new Vector2(xColliderSize, yColliderSize);
				GetComponent<BoxCollider2D>().offset = new Vector2(colliderOffset.x, colliderOffset.y);
			}
		}

		// ------------------------------- Frame actions -------------------------------
		
		if(firstWallJumpCollision()) // disable gravity for a short time
		{
			timeHanging = momenutmHangWalljumpTime;
		}
		

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

		
		// ----------------------- Jump part ------------------------------
		handleJump(jumped);
		bool isJumpingNow = isJumping();
		Vector3 offset;

		currentVelocity = Mathf.Lerp(currentVelocity, areWeGoingRight() ? maxVelocity.Peek() : -maxVelocity.Peek(), Time.deltaTime * accelerationSmooth);
		currentGravity = Mathf.Lerp(currentGravity, (float)gravity.Peek(), Time.deltaTime * gravitySmooth);

		float xMoveForward = (collidingForward() || !running) ? 0.0f : currentVelocity;

		xMoveForward *= Time.deltaTime; // keep this line or it will be framerate dependant
		float grav = isGravityNullified() ? 0.0f : -currentGravity;

		grav *= Time.deltaTime;

		if(isJumpingNow)
		{
			offset = jumpCollec.getNext();
			offset.x = xMoveForward;

			if(collidingForward()) // if colliding, move only on Y
			{
				offset.x = 0;

				if(offset.y <= 0) 
				{
					offset.y = 0;
					if((WallJumpStrategy)wallJumpStrat.Peek() == WallJumpStrategy.Normal)
					{
						jumpCollec.endJump();
					}
					else
					{
						jumpCollec.reset();
					}
				}
			}

			if (collidingAbove() && offset.y > 0) // colliding forward and going up, let's keep going up
			{
				offset.y = 0;
			}
		}
		else
		{
			offset = new Vector3(xMoveForward, grav, 0.0f);
		}

		applyOutsideForce(ref offset);
		offset = adjustOffsetCheckingCollision(offset);

		totalDistanceRun += Mathf.Abs(offset.x);
		characTransform.position += offset;

		animator.SetBool("isRunning", running);
		animator.SetBool("isJumping", jumpIn > 0);
		animator.SetBool("isSliding", isDashing());

		// Record of this frame for next frame

		previousFrameState = state.deepCopy();	
		wasJumpingLastFrame = isJumpingNow;	
	}
		
	/* ------------------------------------------------------ Functions -------------------------------------------------------*/

	private bool isGravityNullified()
	{
		return (nullifyGravityOnDash && isDashing()) || (grounded() && currentGravity > 0) || (timeHanging > 0 && outsideForce.magnitude == 0);
	}

	public void pushWallStack(JumpDirectionOnWallOrEdge d, GameObject from)
	{
		jumpWallStack.Push(d, from);
	}

	public void popWallStack(GameObject from)
	{
		jumpWallStack.Remove(from);
	}



	private void applyOutsideForce(ref Vector3 offset)
	{
		if(outsideForce.magnitude != 0.0f)
		{
			float yForce = offset.y < 0.0f ? outsideForce.y : outsideForce.y + offset.y;
			offset = new Vector3(offset.x + outsideForce.x, yForce, 0.0f);
			outsideForce = Vector2.zero;
		}
	}

	public void addOutsideForce(Vector2 offset)
	{
		outsideForce = offset;
	}

	public Vector3 adjustOffsetCheckingCollision(Vector3 offset)
	{
		Vector2 off = new Vector2(offset.x, offset.y);

		float magnitude = off.magnitude;

		if(state.isThisColliding(off, ref magnitude))
		{
			offset = Vector3.ClampMagnitude(offset, magnitude);
		}

		return offset;
	}
	
	public void invokeFunctionIn(string functionName, float time)
	{
		Invoke(functionName, time);
	}

	public void doNotRunRight()
	{		
		runDirStack.Push(RunDirectionOnGround.KeepTheAirOne, gameObject);
	}

	public void popGroundRunDirection()
	{
		runDirStack.Pop();
	}

	public void popGroundRunDirectionIn(float time)
	{
		invokeFunctionIn("popGroundRunDirection", time);
	}

	public void handleJump(bool jumpedThisFrame)
	{
		if(jumpedThisFrame) // jumped this frame
		{
			jumpCollec.startJump(characTransform.position);
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
		characTransform.position = new Vector2(x, y); 
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

	public void canBeRepelledEnable()
	{
		canBeRepelled = true;
	}


	public void inverseXVelocity(float magnitude, float max)
	{
		if(canBeRepelled)
		{
			canBeRepelled = false;
			currentVelocity = currentVelocity * -1 * magnitude;
		
			currentVelocity = Mathf.Clamp(currentVelocity, -max, max);

			jumpDefinition.flip();
			doubleJumpDefinition.flip();

			if(isJumping())
			{
				jumpCollec.reset();
			}

			run();
			//invokeFunctionIn("canBeRepelledEnable", timeBetweenRepelledAgain);
		}
	}

	public void inverseYVelocity(float magnitude, float max)
	{
		if(canBeRepelled)
		{
			canBeRepelled = false;
			if(currentGravity == 0)
			{
				currentGravity = 0.01f;
			}
			currentGravity =  currentGravity * -1 * magnitude;

			currentGravity = Mathf.Clamp(currentGravity, -max, max);

			if(isJumping())
			{
				jumpCollec.reset();
			}
	
			run();
			//invokeFunctionIn("canBeRepelledEnable", timeBetweenRepelledAgain);
		}
	}


	public void inverseVelocity(float magnitude, float maxX, float maxY)
	{
		inverseXVelocity(magnitude, maxX);
		inverseYVelocity(magnitude, maxY);
	}

	public void acquireObject()
	{
		objectsCollected++;
	}

	public void loopingJumps(bool b)
	{
		if(b && !isJumping())
		{
			jumpCollec.reset();
		}

		jumpCollec.loopingJumps(b);
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

			if(UnityUtils.isNetworkGame())
			{
				CmdJump();
			}
			else
			{
				actualJump();
			}
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
		currentVelocity *= 2;
	}

	public void decelerate()
	{
		currentVelocity /= 2;
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
		currentVelocity *= dashSpeedMul;
		dashIn = dashTime;
		GetComponent<BoxCollider2D>().size = new Vector2(xColliderSize, yColliderSize / 2);
		GetComponent<BoxCollider2D>().offset = new Vector2(colliderOffset.x, colliderOffset.y - (yColliderSize / 4));
	}

	/* ------------------------------------------------------ Function to inquiry the state -------------------------------------------------------*/

	public int getObjectsAcquiredCount()
	{
		return objectsCollected;
	}

	public float getRunDistance()
	{
		return totalDistanceRun;
	}

	public int getNumberOfJumps()
	{
		return jumpCollec.getNumberOfJumps();
	}

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
		return currentVelocity;
	}

	public bool isJumping()
	{
		return jumpCollec.isJumping();
	}

	public bool isDashing()
	{
		return dashIn > 0;
	}

	public bool collidingAbove()
	{
		return state.isCollidingAbove;
	}

	public bool collidingForward()
	{
		return currentVelocity > 0 && state.isCollidingRight || currentVelocity < 0 && state.isCollidingLeft;
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
		return state.isWallSticking && collidingForward();
	}

	public bool grounded()
	{
		return state.isGrounded;
	}

	public bool areWeGoingRight()
	{
		return currentVelocity >= 0;
	}

	public bool canJump()
	{
		if(collidingAbove())
			return false;

		JumpRestrictions jump = (JumpRestrictions)jumpState.Peek();
		if(jump == JumpRestrictions.Anywhere)
		{
			return true;
		}
		else if(jump == JumpRestrictions.Never)
		{
			return false;
		}

		if(firstJumpInAirEnabled == false && (!grounded() && !wallSticking()) && getCurrentJumpCount() == 0)
			return false;

		if(getMaxJumps() <= getCurrentJumpCount())
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	public bool stopped()
	{
		return !running ;
	}

	public bool isDead()
	{
		return health <= 0;
	}

	public bool amILocalPlayer()
	{
		return networkGame ? isLocalPlayer : true;
	}

	/* ------------------------------------------------------ Function about the direction of the character -------------------------------------------------------*/
	
	public void changeDirection()
	{
		bool runningRight = currentVelocity > 0;

		if(runningRight == facingRight)
		{
			Flip(); // display
		}

		jumpDefinition.flip();
		doubleJumpDefinition.flip();

		if(stopped())
		{
			run();
		}

		currentVelocity *= -1;
	}

	private void makeItRunRightOnGround()
	{
		if(((RunDirectionOnGround)runDirStack.Peek() == RunDirectionOnGround.AlwaysRight && grounded() && currentVelocity < 0 )
		|| ((RunDirectionOnGround)runDirStack.Peek() == RunDirectionOnGround.AlwaysLeft && grounded() && currentVelocity > 0 ))
		{
			changeDirection();
		}
	}

	public void reinit()
	{
		if(flipped())
			Flip();

		state.clean();
		previousFrameState = state.deepCopy();

		jumpCollec.reset();
		jumpCollec.reinit();
		running = true;
		currentVelocity = 0.0f; // start stop
		currentGravity = gravityFactor;
		canBeRepelled = true;

		wallJumpStrat.Clear();
		wallJumpStrat.Push(baseStrategy, gameObject);

		maxVelocity.Clear();
		maxVelocity.Push(xSpeedBySecond, gameObject);

		runDirStack.Clear();
		runDirStack.Push(runDir, gameObject);

		jumpState.Clear();
		jumpState.Push(jumpRes, gameObject);

		jumpWallStack.Clear();
		jumpWallStack.Push(jumpWall, gameObject);

		gravity.Clear();
		gravity.Push(gravityFactor, gameObject);
		health = maxHealth;

		dashIn = 0;
		timeHanging = 0;
		jumpIn = 0;
		lastJumpFailedAttempt = 0;

		GetComponent<BoxCollider2D>().size = new Vector2(xColliderSize, yColliderSize);
		GetComponent<BoxCollider2D>().offset = new Vector2(colliderOffset.x, colliderOffset.y);

		CancelInvoke(); // cancel all invokes
	}

	public bool flipped()
	{
		return characTransform.localScale.x < 0 ;
	}

	private void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = characTransform.localScale;
		theScale.x *= -1;
		characTransform.localScale = theScale;
	}

	

	/* ------------------------------------------------------ Network functions -------------------------------------------------------*/
	public override void OnStartLocalPlayer()
	{
		EventManager.TriggerEvent(EventManager.get().playerSpawnEvent, new GameConstants.PlayerSpawnArgument(gameObject, 
    																									     transform.position.x,
    																									     transform.position.y));
	}

	[ClientRpc]
    void RpcUnpause()
    {
    	EventManager.TriggerEvent(EventManager.get().unPausePlayerEvent, new GameConstants.UnPausePlayerArgument());
    }

    void actualJump()
    {
    	upSpeed = 10.0f;
		jumpIn = timeBetweenJumps;
		lastJumpFailedAttempt = 0.0f;
		timeHanging = 0.0f;
    }

    [Command]
    void CmdJump()
    {
    	actualJump();
    }

    private void unpausePlayers(GameConstants.UnPauseAllPlayerArgument arg)
    {
    	if(isServer)
    	{
    		RpcUnpause();
    	}
    }

     void OnEnable()
    {
        EventManager.StartListening(EventManager.get().unPauseAllPlayerEvent, unpausePlayers);
    }

    void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().unPauseAllPlayerEvent, unpausePlayers);
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