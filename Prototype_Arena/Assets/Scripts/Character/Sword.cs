using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public BoxCollider meshCollider;

    public void Use(float attackTime)
    {
        StartCoroutine(Attack(attackTime));
    }

    IEnumerator Attack(float attackTime)
    {
        yield return new WaitForSeconds(0.5f);
        meshCollider.enabled = true;
        yield return new WaitForSeconds(attackTime - 0.5f);
        meshCollider.enabled = false;
    }
}