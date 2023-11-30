using Base.Mobs.Entity;
using Base.Mobs.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BlackKnight
{
    #region public State Method
    public void CheckNearestEnemy()
    {

        return;
    }

    public void MoveToTarget()
    {
        if (target == null)
        {
            Debug.LogError("Can not Move to Target That is null");
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, StatData.SPD * Time.deltaTime);
        //transform.Translate(StatData.SPD * Time.deltaTime * target.transform.position);
    }

    public override void AttackTarget()
    {
        if (target.StatData.DEAD == true)
        {
            CheckNearestEnemy();
        }
        else
        {
            target.DamagedCharacter(StatData.ATK);
        }
    }

    public override void DamagedCharacter(float damage)
    {

    }
    #endregion

    #region protected Method
    protected override IEnumerator UpdateFSM()
    {
        Debug.Log("Wait UpdateFSM");
        yield return new WaitForSeconds(3f);
        Debug.Log("Start UpdateFSM");

        Collider2D detectedTarget;

        while (true)
        {
            switch (curState)
            {
                case EState.Idle:
                    break;

                case EState.Move:
                    break;

                case EState.Attack:
                    break;

                case EState.Skill:
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
            case EState.Idle:
                FSM.ChangeState(StateDict[EState.Idle]);
                break;
            case EState.Move:
                FSM.ChangeState(StateDict[EState.Move]);
                break;
            case EState.Attack:
                FSM.ChangeState(StateDict[EState.Attack]);
                break;
            case EState.Skill:
                FSM.ChangeState(StateDict[EState.Skill]);
                break;
            default:
                Debug.LogError("ChangeState Error");
                break;
        }
    }
    #endregion
}
