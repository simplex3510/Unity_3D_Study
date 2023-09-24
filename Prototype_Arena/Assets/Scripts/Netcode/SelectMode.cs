using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SelectMode : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        //* �� ��ư�� ������ �߻��� �̺�Ʈ�� ���ٽ����� ����
        serverBtn.onClick.AddListener(() => {
            //* ��Ʈ��ũ �Ŵ����� �̱������� ���ְ� 
            //* StartServer ��ư�� ����� ������
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() => {
            //* StartHost ��ư�� ����� ������
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() => {
            //* StartClient ��ư�� ����� ������
            NetworkManager.Singleton.StartClient();
        });
    }
}