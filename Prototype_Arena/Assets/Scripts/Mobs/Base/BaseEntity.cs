using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base.Mobs.State;
using Base.Mobs.FSM;
using Base.Mobs.StatData;

namespace Base.Mobs.Entity
{
    public interface IDamagable
    {
        public void AttackTarget();
        public void DamagedEntity(float damage);
    }

    public abstract class BaseEntity : MonoBehaviour, IDamagable
    {
        [SerializeField]
        protected Animator animController;
        public Animator AnimController { get { return animController; } }

        protected EState curState;
        protected Dictionary<EState, IStatable> StateDict { get; set; }
        protected FiniteStateMachine FSM { get; set; }

        [SerializeField]
        protected BaseStatData statData;
        public BaseStatData StatData { get { return statData; } }

        [SerializeField]
        protected GameObject targetChar;
        protected IDamagable targetIDam;
        [SerializeField]
        protected LayerMask EnemyLayerMask;
        [SerializeField]
        protected int Life;

        protected abstract void InitializeStateDict();
        protected abstract void AssignAnimationParameters();
        protected abstract IEnumerator UpdateFSM();
        protected abstract void ChangeState(EState eState);

        public abstract void AttackTarget();
        public abstract void DamagedEntity(float damage);
    }
}