using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base.Mobs.Entity;
using Base.Mobs.FSM;
using Base.Mobs.State;

public partial class BlackKnight : BaseEntity
{
    public int AnimParam_Idle { get; private set; }
    public int AnimParam_Move { get; private set; }
    public int AnimParam_Attack { get; private set; }
    public int AnimParam_Skill { get; private set; }

    private void Awake()
    {
        curState = EState.Idle;
        AnimatorCtrller = GetComponent<Animator>();
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
        StateDict[EState.Idle] = new BlackKnight_IdleState(this);
        StateDict[EState.Move] = new BlackKnight_MoveState(this);
        StateDict[EState.Attack] = new BlackKnight_AttackState(this);
        StateDict[EState.Skill] = new BlackKnight_SkillState(this);
    }

    protected override void AssignAnimationParameters()
    {
        AnimParam_Idle = Animator.StringToHash("idle");
        AnimParam_Move = Animator.StringToHash("move");
        AnimParam_Attack = Animator.StringToHash("canAttack");
        AnimParam_Skill = Animator.StringToHash("skill");
    }
}