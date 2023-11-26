using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mob.Entity;
using Mob.State;

public partial class Mob1 : MobEntity
{
    public int AnimParam_Idle { get; private set; }
    public int AnimParam_Move { get; private set; }
    public int AnimParam_Attack { get; private set; }
    public int AnimParam_Skill { get; private set; }
    public int AnimParam_Ult { get; private set; }
    public int AnimParam_GetHit { get; private set; }
    public int AnimParam_Dizzy { get; private set; }
    public int AnimParam_Die { get; private set; }

    private void Awake()
    {
        curState = EState.Idle;
        AnimatorCtrller = GetComponent<Animator>();
        StateDict = new Dictionary<EState, MStatable>();
        InitializeStatDict();
        fsm = new FiniteStateMachine(StateDict[curState]);
        AssignAnimationParameters();
        StatData.InitializeStaData();
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateFSM());
    }

    protected override void InitializeStatDict()
    {
        StateDict[EState.Idle] = new Mob1_IdleState(this);
        StateDict[EState.Move] = new Mob1_MoveState(this);
        StateDict[EState.Attack] = new Mob1_AttackState(this);
        StateDict[EState.Skill] = new Mob1_SkillState(this);
        StateDict[EState.Ult] = new Mob1_UltState(this);
        StateDict[EState.GetHit] = new Mob1_GetHitState(this);
        StateDict[EState.Dizzy] = new Mob1_DizzyState(this);
        StateDict[EState.Die] = new Mob1_DieState(this);

    }
    protected override void AssignAnimationParameters()
    {
        AnimParam_Idle = Animator.StringToHash("idle");
        AnimParam_Move = Animator.StringToHash("move");
        AnimParam_Attack = Animator.StringToHash("attack");
        AnimParam_Skill = Animator.StringToHash("skill");
        AnimParam_Ult = Animator.StringToHash("ultimate");
        AnimParam_GetHit = Animator.StringToHash("gethit");
        AnimParam_Dizzy = Animator.StringToHash("dizzy");
        AnimParam_Die = Animator.StringToHash("die");
    }

}
