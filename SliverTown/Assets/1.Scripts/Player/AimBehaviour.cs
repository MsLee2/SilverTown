using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

/// <summary>
/// 마우스 오른쪽 버튼으로 조준
/// 다른 동작보다 조준 우선
/// 다른 동작 대체
/// </summary>


public class AimBehaviour : GenericBehaviour
{
    public Texture2D crossHair; //크로스헤어
    public float aimTurnSmoothing; //조준시 회전 속도

    //카메라 오프셋
    private Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0.0f);
    private Vector3 aimCamOffset = new Vector3(0.0f, 0.4f, -0.7f);

    //애니메이터 파라메터
    private int aimBool;
    private bool aim; //조준 중인지 체크
    private int cornerBool; //코너 애니메이션 관련
    private bool peekCorner; //플레이어 코너 체크
    private Vector3 initalRootRotation; // Root Bone 회전값.
    private Vector3 initalHipRotation; // Hip Bone
    private Vector3 initalSpineRotation; // Spine Bone

    private Transform myTransform;

    private void Start()
    {
        //캐싱
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
    // 카메라에 따라 플레이어를 올바른 방향으로 회전
    void Rotating()
    {
        Vector3 forward = behaviourController.playerCamera.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;

        Quaternion targetRotation = Quaternion.Euler(0f, behaviourController.GetCamScript.GetH, 0.0f);

        float minSpeed = Quaternion.Angle(transform.rotation, targetRotation) * aimTurnSmoothing;

        //모서리
        if(peekCorner)
        {
            //조준중일떄 상체만 살짝 기울이기
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

    //조준중일때를 관리 함수
    void AimManagement()
    {
        Rotating();
    }
    private IEnumerator ToggleAimOn()
    {
        yield return new WaitForSeconds(0.05f);

        //조준 불가 상태일때에 대한 예외처리
        if (behaviourController.GetTempLockStatus(this.behaviourCode) || behaviourController.IsOverriding(this))
        {
            yield return false;
        }
        else //조준 시작
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

            behaviourController.OverrideWithBehaviour(this); //오버라이딩
        }
    }

    private IEnumerator ToggleAimOff()
    {
        aim = false;

        yield return new WaitForSeconds(0.3f);
        behaviourController.GetCamScript.ResetTargetOffests();
        behaviourController.GetCamScript.ResetMaxVerticalAngle();
        yield return new WaitForSeconds(0.1f);

        behaviourController.RevokeOverrideingBehaviour(this); //오버라이드 종료
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
        //동작 입력
        peekCorner = behaviourController.GetAnimator.GetBool(cornerBool);

        if(Input.GetAxisRaw(ButtonName.Aim) != 0 && !aim)
        {
            StartCoroutine(ToggleAimOn()); //코루틴 시작
        }
        else if(aim && Input.GetAxisRaw(ButtonName.Aim) == 0)
        {
            StartCoroutine(ToggleAimOff());
        }

        canSprint = !aim; //조준중일때 달리기 비활성화 추후 수정 맘대로

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
