using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using Spine.Unity;

public class HandleSpineAnimation : MonoBehaviour
{
	private Animator animator;

	private CharacterController2D controller;

	private int runHash = Animator.StringToHash("isRunning");
	private int jumpHash = Animator.StringToHash("isJumping");
	private int ascendingHash = Animator.StringToHash("isAscending");
	private int descendingHash = Animator.StringToHash("isDescending");
	private int groundedHash = Animator.StringToHash("isGrounded");
	private int doubleJumpHash = Animator.StringToHash("doubleJump");
	private int wallStickingHash = Animator.StringToHash("isWallSticking");
	private int collidingForwardHash = Animator.StringToHash("isCollidingForward");
	private int slidingHash = Animator.StringToHash("isSliding");
	private int stopSlidingHash = Animator.StringToHash("stopSliding");
	private int finishHash = Animator.StringToHash("finished");
	private int deadHash = Animator.StringToHash("isDead");
	private int initHash = Animator.StringToHash("init");

	void OnEnable ()
    {
        EventManager.StartListening (EventManager.get().hitFinalCheckpointEvent, lastCheckpointHit);
        EventManager.StartListening (EventManager.get().playerDeadEvent, playerIsDead);
        EventManager.StartListening (EventManager.get().initPlayerEvent, playerInit);
        EventManager.StartListening (EventManager.get().stopAnimationsEvent, stopAnim);
        EventManager.StartListening (EventManager.get().playAnimationsEvent, playAnim);
    }

    void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().hitFinalCheckpointEvent, lastCheckpointHit);
        EventManager.StopListening (EventManager.get().playerDeadEvent, playerIsDead);
        EventManager.StopListening (EventManager.get().initPlayerEvent, playerInit);
        EventManager.StopListening (EventManager.get().stopAnimationsEvent, stopAnim);
        EventManager.StopListening (EventManager.get().playAnimationsEvent, playAnim);
    }

	public void Awake()
	{
		controller = GetComponentInParent<CharacterController2D>();
		animator = GetComponent<Animator>();
	}

	public void lastCheckpointHit(GameConstants.HitFinalCheckpointArgument arg)
	{
		playEndAnimation();
	}

	void playerIsDead(GameConstants.PlayerDeadArgument arg)
    {
        playDeadAnimation();
    }

    void playerInit(GameConstants.InitPlayerArgument arg)
    {
        animator.SetBool(initHash, true);
        animator.SetBool(deadHash, false);
    }

    private void stopAnim(GameConstants.StopAnimationsArgument arg)
    {
    	stop();
    }

    private void playAnim(GameConstants.PlayAnimationsArgument arg)
    {
    	play();
    }

    private void stop()
    {
    	animator.speed = 0;
    }

    private void play()
    {
    	animator.speed = 1;
    }

	public void LateUpdate()
	{
		animator.SetBool(runHash, controller.runspeed() > 0.0f);
		animator.SetBool(jumpHash, controller.isJumping());
		animator.SetBool(ascendingHash, controller.getYVelocity() > 0.0f);
		animator.SetBool(descendingHash, controller.getYVelocity() < 0.0f);
		animator.SetBool(groundedHash, controller.grounded());
		animator.SetBool(doubleJumpHash, controller.doubleJumpedThisFrame());
		animator.SetBool(wallStickingHash, controller.wallSticking());
		animator.SetBool(collidingForwardHash, controller.collidingForward());
		animator.SetBool(slidingHash, controller.isSliding());
		animator.SetBool(stopSlidingHash, controller.endSlide());
		animator.SetBool(initHash, false);
	}

	public void playEndAnimation()
	{
		animator.SetTrigger(finishHash);
	}

	public void playDeadAnimation()
	{
		animator.SetBool(deadHash, true);
	}
}

