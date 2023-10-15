using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 뒤끝 SDK namespace 추가
using BackEnd;

public class BackendManager : MonoBehaviour
{
    void Start()
    {
        var backendReturnObject = Backend.Initialize(true); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (backendReturnObject.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + backendReturnObject); // 성공일 경우 statusCode 204 Success
        }
        else
        {
            Debug.LogError("초기화 실패 : " + backendReturnObject); // 실패일 경우 statusCode 400대 에러 발생
        }
    }
}