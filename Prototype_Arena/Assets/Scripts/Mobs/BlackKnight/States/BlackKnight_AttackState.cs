using Base.Mobs.State;

public class BlackKnight_AttackState : BaseState
{
    public BlackKnight_AttackState(BlackKnight entity) : base(entity) { }

    public override void OnStateEnter()
    {
        BlackKnight mob1 = entity as BlackKnight;
        mob1.AnimatorCtrller.SetBool(mob1.AnimParam_Attack, true);
    }

    public override void OnStateExit()
    {
        BlackKnight mob1 = entity as BlackKnight;
        mob1.CheckNearestEnemy();
        mob1.AnimatorCtrller.SetBool(mob1.AnimParam_Attack, false);
    }

    public override void OnStateUpdate()
    {

    }
}
