using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동, 점프 담당
/// 충돌처리 기능 포함
/// 기본동작으로 작동
/// </summary>
public class MoveBehaviour : GenericBehaviour
{
    public float walkSpeed = 0.15f;
    public float runSpeed = 1.0f;
    public float sprintSpeed = 2.0f;
    public float speedDampTime = 0.1f;

    public float jumpHeight = 1.5f;
    public float jumpInertiaForce = 10f; //관성
    public float speed, speedSeeker;
    private int jumpBool; //ani
    private int groundedBool; //ani
    private bool jump; //isJumping
    private bool isColliding; //

    private CapsuleCollider capsuleCollider;
    private Transform myTransform;

    private void Start()
    {
        myTransform = transform;
        capsuleCollider = GetComponent<CapsuleCollider>();
        jumpBool = Animator.StringToHash(FC.AnimatorKey.Jump);
        groundedBool = Animator.StringToHash(FC.AnimatorKey.Grounded);
        behaviourController.GetAnimator.SetBool(groundedBool, true);

        behaviourController.SubScribleBehaviour(this);
        behaviourController.RegisterDefaultBehaviour(this.behaviourCode); //가장 기본으로 설정
        speedSeeker = runSpeed;

    }
    Vector3 Rotating(float horizontal, float vertical)
    {
        Vector3 forward = behaviourController.playerCamera.TransformDirection(Vector3.forward);

        forward.y = 0.0f;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x); //직교하는 벡터를 구함
        Vector3 targetDirection = Vector3.zero;
        targetDirection = forward * vertical + right * horizontal;

        if(behaviourController.IsMoving() && targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            Quaternion newRotation = Quaternion.Slerp(behaviourController.GetRigidbody.rotation, targetRotation, behaviourController.turnSmoothing);
            behaviourController.GetRigidbody.MoveRotation(newRotation);
            behaviourController.SetLastDirection(targetDirection);
        }
        if(!(Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f))
        {
            behaviourController.Repositioning();
        }
        return targetDirection;
    }

    private void RemoveVerticalVelocity()
    {
        Vector3 horizontalVelocity = behaviourController.GetRigidbody.velocity;
        horizontalVelocity.y = 0.0f;
        behaviourController.GetRigidbody.velocity = horizontalVelocity;
    }

    void MovementManagement(float horizontal, float vertical) //이동
    {
        if(behaviourController.IsGrounded())
        {
            behaviourController.GetRigidbody.useGravity = true;
        }
        else if(!behaviourController.GetAnimator.GetBool(jumpBool) && behaviourController.GetRigidbody.velocity.y > 0)
        {
            RemoveVerticalVelocity();
        }
        Rotating(horizontal, vertical);
        Vector2 dir = new Vector2(horizontal, vertical);
        speed = Vector2.ClampMagnitude(dir, 1f).magnitude;

        speedSeeker += Input.GetAxis("Mouse ScrollWheel");
        speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
        speed *= speedSeeker;

        if(behaviourController.IsSprinting())
        {
            speed = sprintSpeed;
        }
        behaviourController.GetAnimator.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision) //충돌 체크
    {
        isColliding = true;
        if(behaviourController.IsCurrentBehaviour(GetBehaviourCode) && collision.GetContact(0).normal.y <= 0.1f )
        {
            float vel = behaviourController.GetAnimator.velocity.magnitude;
            Vector3 targentMove = Vector3.ProjectOnPlane(myTransform.forward, collision.GetContact(0).normal).normalized * vel;
            behaviourController.GetRigidbody.AddForce(targentMove, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionExit(Collision collision) //충돌 끝나면
    {
        isColliding = false;
    }

    void JumpManageMent()
    {
        if(jump && !behaviourController.GetAnimator.GetBool(jumpBool) && behaviourController.IsGrounded() )
        {
            behaviourController.LockTempBehaviour(behaviourCode); //점프중에는 이동이 불가
            behaviourController.GetAnimator.SetBool(jumpBool, true);
            if(behaviourController.GetAnimator.GetFloat(speedFloat) > 0.1f)
            {
                capsuleCollider.material.dynamicFriction = 0f;
                capsuleCollider.material.staticFriction = 0f;
                RemoveVerticalVelocity();
                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                behaviourController.GetRigidbody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
            }
        }
        else if(behaviourController.GetAnimator.GetBool(jumpBool))
        {
            if(!behaviourController.IsGrounded() && !isColliding && behaviourController.GetTempLockStatus())
            {
                behaviourController.GetRigidbody.AddForce(myTransform.forward * jumpInertiaForce * Physics.gravity.magnitude * sprintSpeed, ForceMode.Acceleration);
            }
            if (behaviourController.GetRigidbody.velocity.y < 0f&& behaviourController.IsGrounded())
            {
                behaviourController.GetAnimator.SetBool(groundedBool, true);
                capsuleCollider.material.dynamicFriction = 0.6f;
                capsuleCollider.material.staticFriction = 0.6f;
                jump = false;
                behaviourController.GetAnimator.SetBool(jumpBool, false);
                behaviourController.UnLockTempBehaviour(this.behaviourCode);
            }
        }
    }


    private void Update()
    {
        if(!jump && Input.GetButtonDown(ButtonName.Jump) && behaviourController.IsCurrentBehaviour(this.behaviourCode) && !behaviourController.IsOverriding())
        {
            jump = true;
        }
    }

    public override void LocalFixedUpdate()
    {
        MovementManagement(behaviourController.GetH, behaviourController.GetV);
        JumpManageMent();
    }

}
