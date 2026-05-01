using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAnimationControls : MonoBehaviour
{
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        EventHandler.MovementEvent += SetAnimation;
    }
    private void OnDisable()
    {
        EventHandler.MovementEvent -= SetAnimation;
    }

    private void SetAnimation(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle, bool isCarrying,
    ToolEffect toolEffect,bool isUsingToolRight, bool isUsingToolLeft, bool isUsingToolUp, bool isUsingToolDown,
    bool isLiftingToolRight, bool isLiftingToolLeft, bool isLiftingToolUp, bool isLiftingToolDown,
    bool isPickingRight, bool isPickingLeft, bool isPickingUp, bool isPickingDown)
    {
        anim.SetFloat(Settings.xInput, xInput);
        anim.SetFloat(Settings.yInput, yInput);
        anim.SetBool(Settings.isWalking, isWalking);
        anim.SetBool(Settings.isRunning, isRunning);

        anim.SetInteger(Settings.toolEffect,(int) toolEffect);

        if (isUsingToolDown)
        {
            anim.SetTrigger(Settings.isUsingToolDown);
        }
        if (isUsingToolRight)
        {
            anim.SetTrigger(Settings.isUsingToolRight);
        }
        if (isUsingToolLeft)
        {
            anim.SetTrigger(Settings.isUsingToolLeft);
        }
        if (isUsingToolUp)
        {
            anim.SetTrigger(Settings.isUsingToolUp);
        }


        if (isLiftingToolLeft)
        {
            anim.SetTrigger(Settings.isLiftingToolLeft);
        }
        if(isLiftingToolRight)
        {
            anim.SetTrigger(Settings.isLiftingToolRight);
        }
        if (isLiftingToolDown)
        {
            anim.SetTrigger(Settings.isLiftingToolDown);
        }
        if (isLiftingToolUp)
        {
            anim.SetTrigger(Settings.isLiftingToolUp);
        }

        if (isPickingRight)
        {
            anim.SetTrigger(Settings.isPickingRight);
        }
        if (isPickingLeft)
        {
            anim.SetTrigger(Settings.isPickingLeft);
        }
        if (isPickingUp)
        {
            anim.SetTrigger(Settings.isPickingUp);
        }
        if (isPickingDown)
        {
            anim.SetTrigger(Settings.isPickingDown);
        }
    }

    private void AnimationEventPlayFootstepSound()
    {

    }
}
