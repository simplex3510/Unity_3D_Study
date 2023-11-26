using Mob.Entity;
using Mob.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Mob1
{

}
public class Mob1_IdleState : MobState
{
    public Mob1_IdleState(Mob1 entity) : base(entity) { }

    public override void OnStateEnter()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Idle);
    }

    public override void OnStateExit()
    {
        Mob1 mob = entity as Mob1;

    }

    public override void OnStateUpdate()
    {

    }
}

public class Mob1_MoveState : MobState
{
    public Mob1_MoveState(Mob1 entity) : base(entity) { }
    public override void OnStateEnter()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Move);
    }

    public override void OnStateExit()
    {
        Mob1 mob1 = entity as Mob1;

    }
    public override void OnStateUpdate()
    {
       
    }
}
public class Mob1_AttackState : MobState
{
    public Mob1_AttackState(Mob1 entity) : base(entity) { }

    public override void OnStateEnter()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetBool(mob1.AnimParam_Attack, true);
    }
    public override void OnStateExit()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetBool(mob1 .AnimParam_Attack, false);
    }
    public override void OnStateUpdate()
    {
        
    }
}

public class Mob1_SkillState : MobState
{
    public Mob1_SkillState(Mob1 entity) : base(entity) { }

    public override void OnStateEnter()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Skill);
    }

    public override void OnStateExit()
    {
        Mob1 mob = entity as Mob1;

    }
    public override void OnStateUpdate()
    {
     
    }
}

public class Mob1_UltState : MobState
{
    public Mob1_UltState(Mob1 entity) : base(entity) { }

    public override void OnStateEnter()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Skill);
    }

    public override void OnStateExit()
    {
        Mob1 mob = entity as Mob1;

    }
    public override void OnStateUpdate()
    {
     
    }
}

public class Mob1_GetHitState : MobState
{
    public Mob1_GetHitState(Mob1 entity) : base(entity) { }

    public override void OnStateEnter()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Skill);
    }

    public override void OnStateExit()
    {
        Mob1 mob = entity as Mob1;

    }
    public override void OnStateUpdate()
    {
      
    }
}
public class Mob1_DizzyState : MobState
{
    public Mob1_DizzyState(Mob1 entity) : base(entity) { }

    public override void OnStateEnter()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Skill);
    }

    public override void OnStateExit()
    {
        Mob1 mob = entity as Mob1;

    }
    public override void OnStateUpdate()
    {
       
    }
}

public class Mob1_DieState : MobState
{
    public Mob1_DieState(Mob1 entity) : base(entity) { }

    public override void OnStateEnter()
    {
        Mob1 mob1 = entity as Mob1;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Skill);
    }

    public override void OnStateExit()
    {
        Mob1 mob = entity as Mob1;

    }
    public override void OnStateUpdate()
    {
 
    }
}