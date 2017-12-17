using Lean.Touch;
using Photon;
using System;
using System.Collections;
using UnityEngine;

public class CharacterController2D : PunBehaviour
{
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
        HighJump,
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
    public int maxHealth = 10;
    private int health = 10;
    PhotonView photonView;

    public TouchZone touchzone = TouchZone.EntireScreen;

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
        return jumpStrategy == JumpStrat.NormalJump ? 1 : 2;
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

    public float runspeed()
    {
        return _runSpeed;
    }

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = GetComponent<CharacterState>();

        jumpState = new Stack();
        jumpState.Push(jumpRes);

        runDirStack = new Stack();
        runDirStack.Push(runDir);

        jumpWallStack = new Stack();
        jumpWallStack.Push(jumpWall);

        gravityScale = rb.gravityScale;

        gravity = new Stack();
        gravity.Push(gravityScale);
        photonView = GetComponent<PhotonView>();
    }

    public void Start()
    {
        health = maxHealth;
        _runSpeed = runSpeed;
        _actualSpeed = 0;
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
        rb.gravityScale = rb.velocity.y < 0 ? (float)gravity.Peek() : gravityScale; // change gravity only when falling (for now)
    }

    private bool shallNullifySpeed()
    {
        return (_runSpeed > 0 && collidingRight()) || (_runSpeed < 0 && collidingLeft());
    }

    public bool isDead()
    {
        return health <= 0;
    }

    public void respawn(float x, float y)
    {
        if (!isDead())
        {
            Debug.Log("respawning a non dead character");
        }
        reinit();

        health = maxHealth;
        transform.position = new Vector2(x, y);
    }


    public void LateUpdate()
    {
        if (state == null)
        {
            state = GetComponent<CharacterState>();
            if (state == null)
            {
                return;
            }
        }

        if (isDead())
        {
            return;
        }

        state.updateState();
        makeItRunRightOnGround();
        putCorrectGravityScale();
        updateSpeed();

        bool jumped = upSpeed > 0;
        float yVelocity = rb.velocity.y;

        if (jumped)
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

        float xSpeed = shallNullifySpeed() ? 0.0f : _actualSpeed;

        rb.velocity = new Vector2(xSpeed, yVelocity);

        if (_lastSpeed != xSpeed && animator)
        {
            bool isRunning = xSpeed != 0;

            animator.SetBool("isRunning", isRunning);
            _lastSpeed = xSpeed;
        }

        if (grounded() && !jumped)
        {
            consecutiveJumps = 0;
            //Collider2D myCollider = GetComponent<Collider2D>();
            //Vector2 point = new Vector2(myCollider.bounds.center.x, myCollider.bounds.center.y + 1.5f);
            //Debug.DrawLine(myCollider.bounds.center, point, Color.black, 20);
        }
        else if (wallSticking() && !jumped)
        {
            consecutiveJumps = 1;
        }

        updateJumpIn();
        animator.SetBool("isJumping", jumpIn > 0);

    }

    protected virtual void OnEnable()
    {
        // Hook into the events we need
        LeanTouch.OnFingerTap += OnFingerTap;
        LeanTouch.OnFingerSwipe += OnFingerSwipe;

        LeanFingerHeld.OnFingerHeldDown += OnHoldDown;
        LeanFingerHeld.OnFingerHeldUp += OnHoldUp;
    }

    private void updateJumpIn()
    {
        if (jumpIn > 0)
        {
            jumpIn -= Time.deltaTime;
            if (jumpIn < 0)
            {
                jumpIn = 0;

            }
        }
    }

    protected virtual void OnDisable()
    {
        // Unhook the events

        LeanTouch.OnFingerTap -= OnFingerTap;
        LeanTouch.OnFingerSwipe -= OnFingerSwipe;

        LeanFingerHeld.OnFingerHeldDown -= OnHoldDown;
        LeanFingerHeld.OnFingerHeldUp -= OnHoldUp;
    }

    private void updateSpeed()
    {
        float percent = wallSticking() ? 1.0f : Time.deltaTime * accelerationSmooth;
        _actualSpeed = Mathf.Lerp(_actualSpeed, _runSpeed, percent);
    }

    private bool stopped()
    {
        return _runSpeed == 0;
    }

    private bool isItForMe(LeanFinger finger)
    {
        if (!photonView.isMine)
        {
            return false;
        }

        Vector2 originPos = finger.StartScreenPosition;

        switch (touchzone)
        {
            case TouchZone.EntireScreen: return true;
            case TouchZone.LeftHalf: return originPos.x < Screen.width / 2f;
            case TouchZone.RightHalf: return originPos.x > Screen.width / 2f;
        }

        return true;
    }

    public void OnHoldDown(LeanFinger finger)
    {
        if (!isItForMe(finger))
        {
            return;
        }

        int index = grounded() ? (stopped() ? groundedAndStopped : groundedIndex) : airBorn;
        Action action = actions[index].action[(int)Inputs.Hold];
        doAction(action);
    }

    public void OnHoldUp(LeanFinger finger)
    {
        if (!isItForMe(finger))
        {
            return;
        }

        int index = grounded() ? (stopped() ? groundedAndStopped : groundedIndex) : airBorn;
        Action action = actions[index].action[(int)Inputs.HoldUp];
        doAction(action);
    }

    public void OnFingerTap(LeanFinger finger)
    {
        if (!isItForMe(finger))
        {
            return;
        }

        int index = grounded() ? (stopped() ? groundedAndStopped : groundedIndex) : airBorn;

        if (finger.TapCount == 1)
        {
            Action action = actions[index].action[(int)Inputs.Tap];
            doAction(action);
        }
        else if (finger.TapCount > 1)
        {
            Action action = actions[index].action[(int)Inputs.DoubleTap];
            doAction(action);
        }
    }

    public void doAction(Action action)
    {
        if (stopped()) // Stopped
        {
            run();
        }

        switch (action)
        {
            case Action.Jump: jump(jumpMagnitude); break;
            case Action.HighJump: jump(highJumpMagnitude); break;
            case Action.Accelerate: accelerate(); break;
            case Action.Decelerate: decelerate(); break;
            case Action.Dash: dash(); break;
            case Action.Slide: slide(); break;
            case Action.StartSame: run(); break;
            case Action.StartOpposite: run(); changeDirection(); break;
            case Action.Stop: stop(); break;
            default: break;
        }
    }

    public void run()
    {
        if (stopped())
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
    }

    public void slide()
    {
    }

    public void jump(float magnitude)
    {
        if (getMaxJumps() <= consecutiveJumps)
        {
            return;
        }

        if (consecutiveJumps > 0)
        {
            // Second jump, no restriction
        }
        else if (grabingEdge() == false)
        {
            switch ((JumpRestrictions)jumpState.Peek())
            {
                case JumpRestrictions.OnGround:
                    if (!grounded() && !wallSticking())
                    {
                        return;
                    }
                    break;
                case JumpRestrictions.Never: return;
                case JumpRestrictions.Anywhere: break;
            }
        }

        if ((JumpDirectionOnWallOrEdge)jumpWallStack.Peek() == JumpDirectionOnWallOrEdge.Inverse && (grabingEdge() || wallSticking()))
        {
            changeDirection();
        }

        consecutiveJumps++;
        upSpeed = magnitude;
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
        if (!isItForMe(finger))
        {
            return;
        }
        // Store the swipe delta in a temp variable
        var swipe = finger.SwipeScreenDelta;

        int index = grounded() ? (stopped() ? groundedAndStopped : groundedIndex) : airBorn;


        if (swipe.x < -Mathf.Abs(swipe.y)) // Left
        {
            bool goingRight = _runSpeed >= 0;
            if (index == groundedAndStopped)
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
            if (index == groundedAndStopped)
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

        if (stopped())
        {
            run();
        }

        _runSpeed *= -1;
    }

    private void makeItRunRightOnGround()
    {
        if (((RunDirectionOnGround)runDirStack.Peek() == RunDirectionOnGround.AlwaysRight && grounded() && _runSpeed < 0)
        || ((RunDirectionOnGround)runDirStack.Peek() == RunDirectionOnGround.AlwaysLeft && grounded() && _runSpeed > 0))
        {
            changeDirection();
        }
    }

    public void reinit()
    {
        if (transform.localScale.x == -1)
            Flip();

        _runSpeed = runSpeed;
    }

    private bool flipped()
    {
        return transform.localScale.x < 0;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

}