﻿using Base.Mobs.State;

public class BlackKnight_AttackState : BaseState
{
    public BlackKnight_AttackState(BlackKnight entity) : base(entity) { }

    public override void OnStateEnter()
    {
        BlackKnight mob = entity as BlackKnight;
        mob.AnimController.SetBool(mob.AnimParam_Attack, true);
    }

    public override void OnStateExit()
    {
        BlackKnight mob = entity as BlackKnight;
        mob.AnimController.SetBool(mob.AnimParam_Attack, false);
    }

    public override void OnStateUpdate()
    {
        BlackKnight mob = entity as BlackKnight;
        mob.AttackTarget();
    }
}
