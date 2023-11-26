using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mob.State;

namespace Mob.Entity
{
    public interface MDamagable
    {
        public void AttackTarget();
        public void DamagedCharacter(float damage);
    }
    public abstract class MobEntity : MonoBehaviour, MDamagable
    {
        public Animator AnimatorCtrller { get; protected set; }
        public EState curState;
        protected Dictionary<EState, MStatable> StateDict { get; set; }
        protected FiniteStateMachine fsm { get; set; }

        [SerializeField]
        protected BaseStatData statData;
        public BaseStatData StatData { get { return statData; } }

        [SerializeField] 
        protected MobEntity target;
        [SerializeField]
        protected LayerMask EnenmyLayerMask;

        protected abstract void InitializeStatDict();
        protected abstract void AssignAnimationParameters();
        protected abstract IEnumerator UpdateFSM();
        protected abstract void ChangeState(EState eState);
        public abstract void AttackTarget();
        public abstract void DamagedCharacter(float damage);
    }
}

