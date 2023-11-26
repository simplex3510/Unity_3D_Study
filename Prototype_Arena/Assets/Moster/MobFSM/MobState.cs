using Mob.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mob.State
{
    public enum EState : int
    {
        None = 0,
        Idle,
        Move,
        Attack,
        Skill, //Attack2
        Ult, //Attack3
        GetHit,
        Dizzy,
        Die,
        Size
    }
    public interface MStatable
    {
        public void OnStateEnter();
        public void OnStateUpdate();
        public void OnStateExit();
    }

    public abstract class MobState : MStatable
    {
        protected MobEntity entity;

        protected MobState(MobEntity entity)
        {
            this.entity=entity;
        }

        public abstract void OnStateEnter();
        public abstract void OnStateUpdate();
        public abstract void OnStateExit();

        public T GetEntity<T>() where T : MobEntity => (T)entity;
    }
}