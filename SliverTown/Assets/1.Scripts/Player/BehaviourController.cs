using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 현재 동작, 기본 동작, 오버라이딩 동작, 잠긴 동작, 마우스 이동 값
/// 땅에 서있는지, GenericBehaviour를 상속받은 동작들을 업데이트
/// </summary>
public class BehaviourController : MonoBehaviour
{
    private List<GenericBehaviour> behaviours; //기본적인 동작
    private List<GenericBehaviour> overrideBehaviours; //우선 실행되는 동작
    private int currentBehaviour; //현재 동작 해시코드
    private int defaultBehaviour; //기본 동작 해시코드
    private int behaviourLocked; //잠긴 동작 해시코드

    //캐싱할 목록
    public Transform playerCamera;
    private Animator myAnimator;
    private Rigidbody myRigidbody;
    private ThirdPersonOrbitCam camScript;
    private Transform myTransfom;

    //
    private float h; //Horizontal 값
    private float v; //Vertical 값
    public float turnSmoothing = 0.06f; //카메라를 향하도록 움직일 때 회전속도
    private bool changedFOV; //달리기 동작이 카메라 시야각이 변경되었을 떄 저장했는지
    private float sprintFOV = 60; //달리기 시야각
    private Vector3 lastDirection; //마지막 향했던 방향
    private bool sprint; //달리는 중인지?

    private int hFloat; //애니메이터 가로 축 파라미터
    private int vFloat; //애니메이터 세로 축 파라미터
    private int groundedBool; //애니메이터 지상 체크
    private Vector3 colExtents; //땅과의 충돌 체크


    //Get 할 것들 모음~
    public float GetH 
    {
        get => h;
    }
    public float GetV
    {
        get => v;
    }
    public ThirdPersonOrbitCam GetCamScript
    {
        get => camScript;
    }
    public Rigidbody GetRigidbody
    {
        get => myRigidbody;
    }
    public Animator GetAnimator
    {
        get => myAnimator;
    }
    public int GetDefaultBehaviour
    {
        get => defaultBehaviour;
    }

    private void Awake()
    {
        behaviours = new List<GenericBehaviour>();
        overrideBehaviours = new List<GenericBehaviour>();
        myAnimator = GetComponent<Animator>();
        hFloat = Animator.StringToHash(FC.AnimatorKey.Horizontal);
        vFloat = Animator.StringToHash(FC.AnimatorKey.Vertical);
        camScript = playerCamera.GetComponent<ThirdPersonOrbitCam>();
        myRigidbody = GetComponent<Rigidbody>();
        myTransfom = transform;

        //Is Ground?
        groundedBool = Animator.StringToHash(FC.AnimatorKey.Grounded);
        colExtents = GetComponent<Collider>().bounds.extents;
    }

    public bool IsMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon; //이동 감지
    }
    public bool IsHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }
    public bool CanSprint()
    {
        foreach(GenericBehaviour behaviour in behaviours)
        {
            if(!behaviour.AllowSprint)
            {
                return false;
            }
        }
        foreach(GenericBehaviour genricBehaviour in overrideBehaviours)
        {
            if(!genricBehaviour.AllowSprint)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsSprinting()
    {
        return sprint && IsMoving() && CanSprint();
    }
    public bool IsGrounded()
    {
        Ray ray = new Ray(myTransfom.position + Vector3.up * 2 * colExtents.x, Vector3.down);
        return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
    }

    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        myAnimator.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
        myAnimator.SetFloat(vFloat, v, 0.1f, Time.deltaTime);

        sprint = Input.GetButton(ButtonName.Sprint); //쉬프트버튼

        //if(!IsSprinting()) //달리는 중에 FOV 변경
        //{
        //    changedFOV = true;
        //    camScript.SetFOV(sprintFOV);
        //}
        //else if(changedFOV) //끝나면 원상태로
        //{
        //    camScript.ResetFOV();
        //    changedFOV = false;
        //}

        myAnimator.SetBool(groundedBool, IsGrounded());
    }

    public void Repositioning() //캐릭터 틀어짐 방지
    {
        if(lastDirection != Vector3.zero)
        {
            lastDirection.y = 0f; //3인칭 캐릭터 Y이동은 없앰
            Quaternion targetRotation = Quaternion.LookRotation(lastDirection);
            Quaternion newRotation = Quaternion.Slerp(myRigidbody.rotation, targetRotation, turnSmoothing);
            myRigidbody.MoveRotation(newRotation);
        }
    }

    private void FixedUpdate()
    {
        bool isAnyBehaviourActive = false;
        if(behaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            foreach(GenericBehaviour behaviour in behaviours)
            {
                if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                {
                    isAnyBehaviourActive = true;
                    behaviour.LocalFixedUpdate();
                }
            }
        }
        else
        {
            foreach(GenericBehaviour behaviour in overrideBehaviours)
            {
                behaviour.LocalFixedUpdate();
            }
        }
        if(!isAnyBehaviourActive && overrideBehaviours.Count == 0)
        {
            myRigidbody.useGravity = true;
            Repositioning();
        }
    }

    private void LateUpdate()
    {
        if(behaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            foreach(GenericBehaviour behaviour in behaviours)
            {
                if(behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                {
                    behaviour.LocalLateUpdate();
                }
            }
        }
        else
        {
            foreach(GenericBehaviour behaviour in overrideBehaviours)
            {
                behaviour.LocalLateUpdate();
            }
        }
    }

    public void SubScribleBehaviour(GenericBehaviour behaviour) //구독하면 업데이트에 추가
    {
        behaviours.Add(behaviour);
    }
    public void RegisterDefaultBehaviour(int behaviourCode) //코드에 해당하는 애들만 업데이트할 때 기본
    {
        defaultBehaviour = behaviourCode;
        currentBehaviour = behaviourCode;
    }
    public void RegisterBehaviour(int behaviourCode) //코드만 바꾸면 등록
    {
        if(currentBehaviour == defaultBehaviour)
        {
            currentBehaviour = behaviourCode;
        }
    }
    public void UnRegisterBehaviour(int behaviourCode) //등록 해제
    {
        if(currentBehaviour == behaviourCode)
        {
            currentBehaviour = defaultBehaviour;
        }
    }

    public bool OverrideWithBehaviour(GenericBehaviour behaviour)
    {
        if(!overrideBehaviours.Contains(behaviour))
        {
            if(overrideBehaviours.Count == 0)
            {
                foreach(GenericBehaviour behaviour1 in behaviours)
                {
                    if(behaviour1.isActiveAndEnabled && currentBehaviour == behaviour1.GetBehaviourCode)
                    {
                        behaviour1.OnOverride();
                        break;
                    }
                }
            }
            overrideBehaviours.Add(behaviour);
            return true;
        }
        return false;
    }
    public bool RevokeOverrideingBehaviour(GenericBehaviour behaviour)
    {
        if(overrideBehaviours.Contains(behaviour))
        {
            overrideBehaviours.Remove(behaviour);
            return true;
        }
        return false;
    }

    public bool IsOverriding(GenericBehaviour behaviour = null) //오버라이딩중인지 찾기
    {
        if(behaviour == null)
        {
            return overrideBehaviours.Count > 0;
        }
        return overrideBehaviours.Contains(behaviour);
    }
    public bool IsCurrentBehaviour(int behaviourCode)
    {
        return this.currentBehaviour == behaviourCode;
    }

    public bool GetTempLockStatus(int behaviourCode = 0) //잠긴지 안잠긴지 체크
    {
        return (behaviourLocked != 0 && behaviourLocked != behaviourCode);
    }

    public void LockTempBehaviour(int behaviourCode) //동작 락
    {
        if(behaviourLocked == 0)
        {
            behaviourLocked = behaviourCode;
        }
    }

    public void UnLockTempBehaviour(int behaviourCode) //잠금 해제
    {
        if(behaviourLocked == behaviourCode)
        {
            behaviourLocked = 0;
        }
    }

    public Vector3 GetLastDirection()
    {
        return lastDirection;
    }

    public void SetLastDirection(Vector3 direction)
    {
        lastDirection = direction;
    }

}

public abstract class GenericBehaviour : MonoBehaviour
{
    protected int speedFloat;
    protected BehaviourController behaviourController;
    protected int behaviourCode;
    protected bool canSprint;

    private void Awake()
    {
        this.behaviourController = GetComponent<BehaviourController>();
        speedFloat = Animator.StringToHash(FC.AnimatorKey.Speed);
        canSprint = true;
        //동작 타입을 해시코드로 가지고 있다가 추후에 구별용으로 사용.
        behaviourCode = this.GetType().GetHashCode();
    }

    public int GetBehaviourCode
    {
        get => behaviourCode;
    }

    public bool AllowSprint
    {
        get => canSprint;
    }

    public virtual void LocalLateUpdate()
    {

    }

    public virtual void LocalFixedUpdate()
    {

    }

    public virtual void OnOverride() //특정 동작을 우선시 하기 위해
    {

    }

}

