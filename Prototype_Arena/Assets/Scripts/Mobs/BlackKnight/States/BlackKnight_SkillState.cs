using Base.Mobs.State;

public class BlackKnight_SkillState : BaseState
{
    public BlackKnight_SkillState(BlackKnight entity) : base(entity) { }

    public override void OnStateEnter()
    {
        BlackKnight mob1 = entity as BlackKnight;
        mob1.AnimatorCtrller.SetTrigger(mob1.AnimParam_Skill);
    }

    public override void OnStateExit()
    {
        BlackKnight mob1 = entity as BlackKnight;
        mob1.CheckNearestEnemy();
    }

    public override void OnStateUpdate()
    {

    }
}