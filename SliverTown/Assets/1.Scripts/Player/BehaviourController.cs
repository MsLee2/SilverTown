using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ���� ����, �⺻ ����, �������̵� ����, ��� ����, ���콺 �̵� ��
/// ���� ���ִ���, GenericBehaviour�� ��ӹ��� ���۵��� ������Ʈ
/// </summary>
public class BehaviourController : MonoBehaviour
{
    private List<GenericBehaviour> behaviours; //�⺻���� ����
    private List<GenericBehaviour> overrideBehaviours; //�켱 ����Ǵ� ����
    private int currentBehaviour; //���� ���� �ؽ��ڵ�
    private int defaultBehaviour; //�⺻ ���� �ؽ��ڵ�
    private int behaviourLocked; //��� ���� �ؽ��ڵ�

    //ĳ���� ���
    public Transform playerCamera;
    private Animator myAnimator;
    private Rigidbody myRigidbody;
    private ThirdPersonOrbitCam camScript;
    private Transform myTransfom;

    //
    private float h; //Horizontal ��
    private float v; //Vertical ��
    public float turnSmoothing = 0.06f; //ī�޶� ���ϵ��� ������ �� ȸ���ӵ�
    private bool changedFOV; //�޸��� ������ ī�޶� �þ߰��� ����Ǿ��� �� �����ߴ���
    private float sprintFOV = 60; //�޸��� �þ߰�
    private Vector3 lastDirection; //������ ���ߴ� ����
    private bool sprint; //�޸��� ������?

    private int hFloat; //�ִϸ����� ���� �� �Ķ����
    private int vFloat; //�ִϸ����� ���� �� �Ķ����
    private int groundedBool; //�ִϸ����� ���� üũ
    private Vector3 colExtents; //������ �浹 üũ


    //Get �� �͵� ����~
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
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon; //�̵� ����
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

        sprint = Input.GetButton(ButtonName.Sprint); //����Ʈ��ư

        //if(!IsSprinting()) //�޸��� �߿� FOV ����
        //{
        //    changedFOV = true;
        //    camScript.SetFOV(sprintFOV);
        //}
        //else if(changedFOV) //������ �����·�
        //{
        //    camScript.ResetFOV();
        //    changedFOV = false;
        //}

        myAnimator.SetBool(groundedBool, IsGrounded());
    }

    public void Repositioning() //ĳ���� Ʋ���� ����
    {
        if(lastDirection != Vector3.zero)
        {
            lastDirection.y = 0f; //3��Ī ĳ���� Y�̵��� ����
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

    public void SubScribleBehaviour(GenericBehaviour behaviour) //�����ϸ� ������Ʈ�� �߰�
    {
        behaviours.Add(behaviour);
    }
    public void RegisterDefaultBehaviour(int behaviourCode) //�ڵ忡 �ش��ϴ� �ֵ鸸 ������Ʈ�� �� �⺻
    {
        defaultBehaviour = behaviourCode;
        currentBehaviour = behaviourCode;
    }
    public void RegisterBehaviour(int behaviourCode) //�ڵ常 �ٲٸ� ���
    {
        if(currentBehaviour == defaultBehaviour)
        {
            currentBehaviour = behaviourCode;
        }
    }
    public void UnRegisterBehaviour(int behaviourCode) //��� ����
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

    public bool IsOverriding(GenericBehaviour behaviour = null) //�������̵������� ã��
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

    public bool GetTempLockStatus(int behaviourCode = 0) //����� ������� üũ
    {
        return (behaviourLocked != 0 && behaviourLocked != behaviourCode);
    }

    public void LockTempBehaviour(int behaviourCode) //���� ��
    {
        if(behaviourLocked == 0)
        {
            behaviourLocked = behaviourCode;
        }
    }

    public void UnLockTempBehaviour(int behaviourCode) //��� ����
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
        //���� Ÿ���� �ؽ��ڵ�� ������ �ִٰ� ���Ŀ� ���������� ���.
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

    public virtual void OnOverride() //Ư�� ������ �켱�� �ϱ� ����
    {

    }

}

