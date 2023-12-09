using Base.Mobs.Entity;

namespace Base.Mobs.State
{
    public enum EState : int
    {
        None = 0,
        Walk,
        Attack,
        Die,
        Victory,
        Size
    }

    public interface IStatable
    {
        public void OnStateEnter();
        public void OnStateUpdate();
        public void OnStateExit();
    }

    public abstract class BaseState : IStatable
    {
        protected BaseEntity entity;

        protected BaseState(BaseEntity entity)
        {
            this.entity = entity;
        }

        public abstract void OnStateEnter();
        public abstract void OnStateUpdate();
        public abstract void OnStateExit();
    }
}