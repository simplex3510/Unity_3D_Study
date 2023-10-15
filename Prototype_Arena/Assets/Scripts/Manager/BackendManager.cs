using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ڳ� SDK namespace �߰�
using BackEnd;

public class BackendManager : MonoBehaviour
{
    void Start()
    {
        var backendReturnObject = Backend.Initialize(true); // �ڳ� �ʱ�ȭ

        // �ڳ� �ʱ�ȭ�� ���� ���䰪
        if (backendReturnObject.IsSuccess())
        {
            Debug.Log("�ʱ�ȭ ���� : " + backendReturnObject); // ������ ��� statusCode 204 Success
        }
        else
        {
            Debug.LogError("�ʱ�ȭ ���� : " + backendReturnObject); // ������ ��� statusCode 400�� ���� �߻�
        }
    }
}