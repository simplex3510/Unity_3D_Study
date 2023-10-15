using UnityEngine;
using System.Threading.Tasks;

using BackEnd;

public class BackendManager : MonoBehaviour
{
    void Start()
    {
        var bro = Backend.Initialize(true); // �ڳ� �ʱ�ȭ

        // �ڳ� �ʱ�ȭ�� ���� ���䰪
        if (bro.IsSuccess())
        {
            Debug.Log("�ʱ�ȭ ���� : " + bro); // ������ ��� statusCode 204 Success
        }
        else
        {
            Debug.LogError("�ʱ�ȭ ���� : " + bro); // ������ ��� statusCode 400�� ���� �߻� 
        }

        Test();
    }

    // ���� �Լ��� �񵿱⿡�� ȣ���ϰ� ���ִ� �Լ�(����Ƽ UI ���� �Ұ�)
    async void Test()
    {
        await Task.Run(() => {
            BackendLogin.Instance.CustomLogin("admin", "1234"); // �ڳ� �α���

            BackendGameData.Instance.GameDataGet(); //[�߰�] ������ �ҷ����� �Լ�

            Debug.Log("�׽�Ʈ�� �����մϴ�.");
        });
    }
}