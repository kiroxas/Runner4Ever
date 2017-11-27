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

	void OnEnable ()
    {
        EventManager.StartListening (EventManager.get().hitFinalCheckpointEvent, lastCheckpointHit);
    }

    void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().hitFinalCheckpointEvent, lastCheckpointHit);
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
	}

	public void playEndAnimation()
	{
		animator.SetTrigger(finishHash);
	}
}

