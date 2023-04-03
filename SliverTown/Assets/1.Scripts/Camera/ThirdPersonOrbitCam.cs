using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라 중요 속성
/// 오프셋 벡터, 피벗오프셋 백터
/// 위치 오프셋 벡터는 충돌 처리용으로 사용 
/// 피벗오프셋 벡터는 시선 이동에 사용
/// 충돌체크 : 이중 충돌 체크
/// </summary>

[RequireComponent(typeof(Camera))]
public class ThirdPersonOrbitCam : MonoBehaviour
{
    public Transform player; //Player 위치
    public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
    public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

    public float smooth = 10f; //카메라 반응속도
    public float horizontalAimingSpeed = 6.0f; //수평 회전 속도
    public float camRotation = -45f; //카메라 각도


    //사용 미정
    private float verticalAimingSpeed = 6.0f; //수직 회전 속도, 근데 안쓸듯?
    private float maxVerticalAngle = 30.0f; //수직 최대 기울기
    private float minVerticalAngle = -60.0f; // 수직 최소 기울기
    public float recoilAngleBound = 5.0f; //수직 반동
    private float angleV = 0.0f; //마우스 이동에 따른 카메라 수직이동 수치
    private float targetMaxVerticalAngle; //카메라 수직 최대 각도
    private float recoilAngle = 0f; //반동 각도
    
    private float angleH = 0.0f; //마우스 이동에 따른 카메라 수평이동 수치
    private Transform cameraTransform; //카메라 위치 캐싱용
    private Camera myCamera; //FOV 조절용으로 쓸듯?
    private Vector3 relCameraPos; //플레이어로부터 카메라까지의 벡터
    private float relCameraPosMag; //플레이어로부터 카메라까지의 거리
    private Vector3 smoothPivotOffset; //카메라 피벗 보간용 벡터
    private Vector3 smoothCamOffset; //카메라 위치 보간용 벡터
    private Vector3 targetPivotOffset; //카메라 피벗 보간용 벡터
    private Vector3 targetCamOffset; //카메라 위치 보간용 벡터

    private float defaultFOV; //기본 시야값
    private float targetFOV; // 타겟 시야값

    public float GetH
    {
        get => angleH;
    }

    private void Awake()
    {
        //캐싱
        cameraTransform = transform;
        myCamera = cameraTransform.GetComponent<Camera>();
        //카메라 기본 포지션 세팅  
        cameraTransform.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
        //cameraTransform.rotation = Quaternion.identity;
        cameraTransform.rotation = Quaternion.Euler(camRotation, 0f, 0f);

        //카메라와 플레이어간의 상대 벡터, 충돌체크 사용하기 위해
        relCameraPos = cameraTransform.position - player.position;
        relCameraPosMag = relCameraPos.magnitude - 0.5f; //플레이어 제외값

        //기본세팅
        smoothPivotOffset = pivotOffset;
        smoothCamOffset = camOffset;
        //myCamera.fieldOfView;
        defaultFOV = myCamera.fieldOfView;
        Debug.Log(myCamera.fieldOfView);
        angleH = player.eulerAngles.y;

        ResetTargetOffests();
        ResetFOV();
        ResetMaxVerticalAngle();

    }

    public void ResetTargetOffests()
    {
        targetPivotOffset = pivotOffset;
        targetCamOffset = camOffset;
    }
    public void ResetFOV()
    {
        this.targetFOV = defaultFOV;   
    }

    public void ResetMaxVerticalAngle()
    {
        targetMaxVerticalAngle = maxVerticalAngle;
    }

    public void BounceVertical(float degree) //반동
    {
        recoilAngle = degree;
    }

    public void SetTargetOffset(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetCamOffset = newCamOffset;
    }

    public void SetFOV(float customFOV)
    {
        this.targetFOV = customFOV;
    }

    #region not used
    //bool ViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight)
    //{
    //    Vector3 target = player.position + (Vector3.up * deltaPlayerHeight);
    //    if(Physics.SphereCast(checkPos, 0.2f, target - checkPos, out RaycastHit hit, relCameraPosMag))
    //    {
    //        if(hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}

    //bool ReverseViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight, float maxDistance)
    //{
    //    Vector3 origin = player.position + (Vector3.up * deltaPlayerHeight);
    //    if(Physics.SphereCast(origin, 0.2f, checkPos - origin, out RaycastHit hit, maxDistance))
    //    {
    //        if(hit.transform != player && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}

    //bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
    //{
    //    float playerFocusHeight = player.GetComponent<CapsuleCollider>().height * 0.75f;
    //    return ViewingPosCheck(checkPos, playerFocusHeight) && ReverseViewingPosCheck(checkPos, playerFocusHeight, offset);
    //}
    #endregion


    private void Update()
    {
        //마우스 이동 값
        angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
        //angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f) * verticalAimingSpeed;

        #region not used
        ////수직 이동 제한
        //angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);
        ////수직 카메라 바운스
        //angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10f * Time.deltaTime);
        #endregion

        //카메라 회전
        Quaternion camYRotation = Quaternion.Euler(camRotation, angleH, 0.0f);
        Quaternion aimRotation = Quaternion.Euler(camRotation, angleH, 0.0f);
        cameraTransform.rotation = aimRotation;

        //set Fov
        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, targetFOV, Time.deltaTime);

        Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
        Vector3 noCollisionOffset = targetCamOffset; //조준할 때 카메라의 오프셋 값

        #region not used
        //for(float zOffset = targetCamOffset.z; zOffset <= 0f; zOffset += 0.5f) //카메라 충돌 체크
        //{
        //    noCollisionOffset.z = zOffset;
        //    if(DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset, Mathf.Abs(zOffset)) || zOffset == 0f)
        //    {
        //        break;
        //    }
        //}
        #endregion

        smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, smooth * Time.deltaTime);
        smoothCamOffset = Vector3.Lerp(smoothCamOffset, noCollisionOffset, smooth * Time.deltaTime);

        cameraTransform.position = player.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;

        if(recoilAngle > 0.0f)
        {
            recoilAngle -= recoilAngleBound * Time.deltaTime;
        }
        else if(recoilAngle < 0.0f)
        {
            recoilAngle += recoilAngleBound * Time.deltaTime;
        }

    }

    public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
    }
}
