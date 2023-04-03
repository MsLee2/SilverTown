using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

/// <summary>
/// ���콺 ������ ��ư���� ����
/// �ٸ� ���ۺ��� ���� �켱
/// �ٸ� ���� ��ü
/// </summary>


public class AimBehaviour : GenericBehaviour
{
    public Texture2D crossHair; //ũ�ν����
    public float aimTurnSmoothing; //���ؽ� ȸ�� �ӵ�

    //ī�޶� ������
    private Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0.0f);
    private Vector3 aimCamOffset = new Vector3(0.0f, 0.4f, -0.7f);

    //�ִϸ����� �Ķ����
    private int aimBool;
    private bool aim; //���� ������ üũ
    private int cornerBool; //�ڳ� �ִϸ��̼� ����
    private bool peekCorner; //�÷��̾� �ڳ� üũ
    private Vector3 initalRootRotation; // Root Bone ȸ����.
    private Vector3 initalHipRotation; // Hip Bone
    private Vector3 initalSpineRotation; // Spine Bone

    private Transform myTransform;

    private void Start()
    {
        //ĳ��
        myTransform = transform;

        //setup
        aimBool = Animator.StringToHash(AnimatorKey.Aim);
        cornerBool = Animator.StringToHash(AnimatorKey.Corner);

        //value
        Transform hips = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        initalRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initalHipRotation = hips.localEulerAngles;
        initalSpineRotation = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine).localEulerAngles;
    }
    // ī�޶� ���� �÷��̾ �ùٸ� �������� ȸ��
    void Rotating()
    {
        Vector3 forward = behaviourController.playerCamera.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;

        Quaternion targetRotation = Quaternion.Euler(0f, behaviourController.GetCamScript.GetH, 0.0f);

        float minSpeed = Quaternion.Angle(transform.rotation, targetRotation) * aimTurnSmoothing;

        //�𼭸�
        if(peekCorner)
        {
            //�������ϋ� ��ü�� ��¦ ����̱�
            myTransform.rotation = Quaternion.LookRotation(behaviourController.GetLastDirection());
            targetRotation *= Quaternion.Euler(initalRootRotation);
            targetRotation *= Quaternion.Euler(initalHipRotation);
            targetRotation *= Quaternion.Euler(initalSpineRotation);

            Transform spine = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
            spine.rotation = targetRotation;
        }
        else
        {
            behaviourController.SetLastDirection(forward);
            myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, minSpeed * Time.deltaTime);

        }
    }

    //�������϶��� ���� �Լ�
    void AimManagement()
    {
        Rotating();
    }
    private IEnumerator ToggleAimOn()
    {
        yield return new WaitForSeconds(0.05f);

        //���� �Ұ� �����϶��� ���� ����ó��
        if (behaviourController.GetTempLockStatus(this.behaviourCode) || behaviourController.IsOverriding(this))
        {
            yield return false;
        }
        else //���� ����
        {
            aim = true;
            int signal = 1;
            if (peekCorner)
            {
                signal = (int)Mathf.Sign(behaviourController.GetH);
            }

            #region not used Offset Change
            //aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
            //aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;
            #endregion

            yield return new WaitForSeconds(0.1f);
            behaviourController.GetAnimator.SetFloat(speedFloat, 0.0f);

            behaviourController.OverrideWithBehaviour(this); //�������̵�
        }
    }

    private IEnumerator ToggleAimOff()
    {
        aim = false;

        yield return new WaitForSeconds(0.3f);
        behaviourController.GetCamScript.ResetTargetOffests();
        behaviourController.GetCamScript.ResetMaxVerticalAngle();
        yield return new WaitForSeconds(0.1f);

        behaviourController.RevokeOverrideingBehaviour(this); //�������̵� ����
    }

    public override void LocalFixedUpdate()
    {
        #region not used ChangeTargetOffset
        //if(aim)
        //{
        //    behaviourController.GetCamScript.SetTargetOffset(aimPivotOffset, aimCamOffset);
        //}
        #endregion
    }

    public override void LocalLateUpdate()
    {
        AimManagement();
    }

    private void Update()
    {
        //���� �Է�
        peekCorner = behaviourController.GetAnimator.GetBool(cornerBool);

        if(Input.GetAxisRaw(ButtonName.Aim) != 0 && !aim)
        {
            StartCoroutine(ToggleAimOn()); //�ڷ�ƾ ����
        }
        else if(aim && Input.GetAxisRaw(ButtonName.Aim) == 0)
        {
            StartCoroutine(ToggleAimOff());
        }

        canSprint = !aim; //�������϶� �޸��� ��Ȱ��ȭ ���� ���� �����

        if(aim && Input.GetButton(ButtonName.Shoulder) && !peekCorner)
        {
            aimCamOffset.x = aimCamOffset.x * (-1);
            aimPivotOffset.x = aimPivotOffset.x * (-1);
        }
        behaviourController.GetAnimator.SetBool(aimBool, aim);
    }

    private void OnGUI()
    {
        if(crossHair != null)
        {
            float length = behaviourController.GetCamScript.GetCurrentPivotMagnitude(aimPivotOffset);

            if(length < 0.05f)
            {
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - (crossHair.width * 0.5f), Screen.height * 0.5f - (crossHair.height * 0.5f), crossHair.width, crossHair.height), crossHair);
            }
        }
    }

}
