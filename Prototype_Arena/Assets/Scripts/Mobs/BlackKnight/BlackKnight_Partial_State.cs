using Base.Mobs.Entity;
using Base.Mobs.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BlackKnight
{
    private void OnEnable()
    {
        // animController.SetTrigger(AnimParam_Walk);
    }

    #region public State Method
    public bool CheckTargetInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2.0f, EnemyLayerMask);
        if (colliders.Length != 0)
        {
            return true;
        }

        return false;
    }
    public void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetChar.transform.position, StatData.SPD * Time.deltaTime);
        transform.LookAt(targetChar.transform);
    }
    public override void AttackTarget()
    {
        if (targetIDam == null)
        {
            targetIDam = targetChar.GetComponent<IDamagable>();
        }

        // targetIDam.DamagedEntity(10);
    }
    public override void DamagedEntity(float damage)
    {
        StatData.CurHP -= damage;
        if (StatData.DEAD)
        {
            AnimController.SetTrigger(AnimParam_Die);
            Invoke("DieAndDestroy", 3f);
        }
        else
        {
            AnimController.SetTrigger(AnimParam_Damaged);
        }
    }

    public void DieAndDestroy()
    {
        Destroy(gameObject);
    }
    #endregion

    #region protected Method
    protected override IEnumerator UpdateFSM()
    {
        while (true)
        {
            if (StatData.DEAD == true)
            {
                AnimController.SetTrigger(AnimParam_Die);
                yield break;
            }

            if (targetChar.activeSelf == false)
            {
                animController.SetTrigger(AnimParam_Victory);
                yield break;
            }

            switch (curState)
            {
                case EState.Walk:
                    if (CheckTargetInRange() == true)
                    {
                        ChangeState(EState.Attack);
                    }
                    break;

                case EState.Attack:
                    if (CheckTargetInRange() == false)
                    {
                        ChangeState(EState.Walk);
                    }
                    break;

                default:
                    Debug.LogError("UpdateFSM Error");
                    break;
            }

            FSM.UpdateState();

            yield return null;
        }
    }

    protected override void ChangeState(EState nextState)
    {
        curState = nextState;

        switch (curState)
        {
            case EState.Walk:
                FSM.ChangeState(StateDict[EState.Walk]);
                break;
            case EState.Attack:
                FSM.ChangeState(StateDict[EState.Attack]);
                break;
            case EState.Die:
                FSM.ChangeState(StateDict[EState.Die]);
                break;
            case EState.Victory:
                FSM.ChangeState(StateDict[EState.Victory]);
                break;
            default:
                Debug.LogError("ChangeState Error");
                break;
        }
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Sword")
        {
            DamagedEntity(collision.gameObject.GetComponent<Sword>().Atk);
        }
    }
}
