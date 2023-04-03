using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ī�޶� �߿� �Ӽ�
/// ������ ����, �ǹ������� ����
/// ��ġ ������ ���ʹ� �浹 ó�������� ��� 
/// �ǹ������� ���ʹ� �ü� �̵��� ���
/// �浹üũ : ���� �浹 üũ
/// </summary>

[RequireComponent(typeof(Camera))]
public class ThirdPersonOrbitCam : MonoBehaviour
{
    public Transform player; //Player ��ġ
    public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
    public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

    public float smooth = 10f; //ī�޶� �����ӵ�
    public float horizontalAimingSpeed = 6.0f; //���� ȸ�� �ӵ�
    public float camRotation = -45f; //ī�޶� ����


    //��� ����
    private float verticalAimingSpeed = 6.0f; //���� ȸ�� �ӵ�, �ٵ� �Ⱦ���?
    private float maxVerticalAngle = 30.0f; //���� �ִ� ����
    private float minVerticalAngle = -60.0f; // ���� �ּ� ����
    public float recoilAngleBound = 5.0f; //���� �ݵ�
    private float angleV = 0.0f; //���콺 �̵��� ���� ī�޶� �����̵� ��ġ
    private float targetMaxVerticalAngle; //ī�޶� ���� �ִ� ����
    private float recoilAngle = 0f; //�ݵ� ����
    
    private float angleH = 0.0f; //���콺 �̵��� ���� ī�޶� �����̵� ��ġ
    private Transform cameraTransform; //ī�޶� ��ġ ĳ�̿�
    private Camera myCamera; //FOV ���������� ����?
    private Vector3 relCameraPos; //�÷��̾�κ��� ī�޶������ ����
    private float relCameraPosMag; //�÷��̾�κ��� ī�޶������ �Ÿ�
    private Vector3 smoothPivotOffset; //ī�޶� �ǹ� ������ ����
    private Vector3 smoothCamOffset; //ī�޶� ��ġ ������ ����
    private Vector3 targetPivotOffset; //ī�޶� �ǹ� ������ ����
    private Vector3 targetCamOffset; //ī�޶� ��ġ ������ ����

    private float defaultFOV; //�⺻ �þ߰�
    private float targetFOV; // Ÿ�� �þ߰�

    public float GetH
    {
        get => angleH;
    }

    private void Awake()
    {
        //ĳ��
        cameraTransform = transform;
        myCamera = cameraTransform.GetComponent<Camera>();
        //ī�޶� �⺻ ������ ����  
        cameraTransform.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
        //cameraTransform.rotation = Quaternion.identity;
        cameraTransform.rotation = Quaternion.Euler(camRotation, 0f, 0f);

        //ī�޶�� �÷��̾�� ��� ����, �浹üũ ����ϱ� ����
        relCameraPos = cameraTransform.position - player.position;
        relCameraPosMag = relCameraPos.magnitude - 0.5f; //�÷��̾� ���ܰ�

        //�⺻����
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

    public void BounceVertical(float degree) //�ݵ�
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
        //���콺 �̵� ��
        angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
        //angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f) * verticalAimingSpeed;

        #region not used
        ////���� �̵� ����
        //angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);
        ////���� ī�޶� �ٿ
        //angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10f * Time.deltaTime);
        #endregion

        //ī�޶� ȸ��
        Quaternion camYRotation = Quaternion.Euler(camRotation, angleH, 0.0f);
        Quaternion aimRotation = Quaternion.Euler(camRotation, angleH, 0.0f);
        cameraTransform.rotation = aimRotation;

        //set Fov
        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, targetFOV, Time.deltaTime);

        Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
        Vector3 noCollisionOffset = targetCamOffset; //������ �� ī�޶��� ������ ��

        #region not used
        //for(float zOffset = targetCamOffset.z; zOffset <= 0f; zOffset += 0.5f) //ī�޶� �浹 üũ
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
