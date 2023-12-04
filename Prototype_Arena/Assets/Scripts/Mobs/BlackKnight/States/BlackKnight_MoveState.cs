using Base.Mobs.State;

public class BlackKnight_MoveState : BaseState
{
    public BlackKnight_MoveState(BlackKnight entity) : base(entity) { }

    public override void OnStateEnter()
    {
        BlackKnight mob1 = entity as BlackKnight;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Move);
    }

    public override void OnStateExit()
    {
        BlackKnight mob1 = entity as BlackKnight;
        mob1.CheckNearestEnemy();
    }

    public override void OnStateUpdate()
    {
        BlackKnight mob1 = entity as BlackKnight;
        mob1.MoveToTarget();
    }
}
