using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base.Mobs.Entity;
using Base.Mobs.FSM;
using Base.Mobs.State;

public partial class BlackKnight : BaseEntity
{
    public float life =300;
    public int AnimParam_Walk { get; private set; }
    public int AnimParam_Damaged { get; private set; }
    public int AnimParam_Attack { get; private set; }
    public int AnimParam_Die { get; private set; }
    public int AnimParam_Victory { get; private set; }

    private void Awake()
    {
        curState = EState.Walk;
        StateDict = new Dictionary<EState, IStatable>();
        InitializeStateDict();
        FSM = new FiniteStateMachine(StateDict[curState]);

        AssignAnimationParameters();

        StatData.InitializeStatData();
    }

    private void Start()
    {
        StartCoroutine(UpdateFSM());
    }

    protected override void InitializeStateDict()
    {
        StateDict[EState.Walk] = new BlackKnight_WalkState(this);
        StateDict[EState.Attack] = new BlackKnight_AttackState(this);
        StateDict[EState.Die] = new BlackKnight_DieState(this);
        StateDict[EState.Victory] = new BlackKnight_VictoryState(this);
    }

    protected override void AssignAnimationParameters()
    {
        AnimParam_Attack = Animator.StringToHash("IsTargetInRange");
        AnimParam_Damaged = Animator.StringToHash("");
        AnimParam_Die = Animator.StringToHash("");
        AnimParam_Victory = Animator.StringToHash("");
        AnimParam_Walk = Animator.StringToHash("");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && !PlayableCharacterController.isDash && !PlayableCharacterController.isAttack)
        {
            //���� �߿� �浹�ϸ� �÷��̾�� �������� ��, ���⿡�� �ݶ��̴� �޾ƾ�?
            //collision.gameObject.GetComponent<PlayableCharacterController>().Damage(StatData.ATK);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Sword")
        {
            //�ִ� HP 300���� ���� �� �ǰ� �Ǵ�
            //DamagedEntity((float)other.gameObject.GetComponent<Sword>().Atk);
            life -= other.gameObject.GetComponent<Sword>().Atk;
        }
    }
}