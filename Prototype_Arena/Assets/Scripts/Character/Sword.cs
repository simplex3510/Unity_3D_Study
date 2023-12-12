using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public BoxCollider meshCollider;
    [SerializeField]
    private float _atk;
    public float Atk
    {
        get { return _atk; }
    }

    public void Use(float attackTime, float Atk)
    {
        StartCoroutine(Attack(attackTime, Atk));
    }

    IEnumerator Attack(float attackTime, float Atk)
    {
        _atk = Atk;
        yield return new WaitForSeconds(0.5f);
        meshCollider.enabled = true;
        yield return new WaitForSeconds(attackTime - 0.5f);
        meshCollider.enabled = false;
    }
}