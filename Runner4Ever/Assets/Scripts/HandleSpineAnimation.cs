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

	public void Awake()
	{
		controller = GetComponentInParent<CharacterController2D>();
		animator = GetComponent<Animator>();
	}

	public void LateUpdate()
	{
		animator.SetBool("isRunning", controller.runspeed() > 0.0f);
		animator.SetBool("isJumping", controller.isJumping());
		animator.SetBool("isAscending", controller.getYVelocity() > 0.0f);
		animator.SetBool("isDescending", controller.getYVelocity() < 0.0f);
	}

	public void playEndAnimation()
	{
		animator.SetBool("finished", true);
	}
}

