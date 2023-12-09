using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public MeshCollider meshCollider;

    public void Use(float attackTime)
    {
        StartCoroutine(Attack(attackTime));
    }

    IEnumerator Attack(float attackTime)
    {
        meshCollider.enabled = true;
        yield return new WaitForSeconds(attackTime);
        meshCollider.enabled = false;
    }
}
