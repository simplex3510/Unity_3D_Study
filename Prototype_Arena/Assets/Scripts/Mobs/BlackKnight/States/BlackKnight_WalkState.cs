using Base.Mobs.State;

public class BlackKnight_WalkState : BaseState
{
    public BlackKnight_WalkState(BlackKnight entity) : base(entity) { }

    public override void OnStateEnter()
    {
        
    }

    public override void OnStateExit()
    {

    }

    public override void OnStateUpdate()
    {
        BlackKnight mob = entity as BlackKnight;
        mob.MoveToTarget();
    }
}
