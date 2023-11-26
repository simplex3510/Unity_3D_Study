using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class command : MonoBehaviour
{
    [SerializeField]
    private string commandInput = ""; //���۷� ���
    public string CommandInput
    {
        set
        {
            commandInput = value;
        }
    }
    bool LClick;
    bool RClick;
    bool isDash;

    public Dictionary<string, bool> commandList;

    private void Start()
    {
        commandList = new Dictionary<string, bool>();
    }
    void Update()
    {
        GetInput();
        ListAdd();
        Attack();
    }

    void GetInput() //Ŭ�� �Է� �ޱ�
    {
        LClick = Input.GetMouseButtonDown(0);
        RClick = Input.GetMouseButtonDown(1);
        isDash = Input.GetKeyDown(KeyCode.LeftShift);

        if(commandInput == "" && (LClick || RClick)) //�޺��� ���۵Ǹ�
            StartCoroutine(CommandClear()); //�� �� �� �޺� ���۸� ���
        else if(commandInput != "" && (LClick || RClick)) //�޺��� �̾��
        {
            StopCoroutine(CommandClear()); //�ʱ�ȭ�Ǵ� �ð� �ʱ�ȭ
            StartCoroutine(CommandClear());
        }
    }
    void ListAdd()
    {
        if (LClick)
            commandInput += "L";
        if (RClick)
            commandInput += "R";
        if (isDash)
            commandInput += "S";
    }

    void Attack()
    {
        if (commandInput != "" && (LClick || RClick)) //����ִ� �޺��� ����
        {
            StopCoroutine(CommandClear()); //���۸� ���� �ڷ�ƾ �ߴ�

            foreach (var skill in commandList) //��� ��ų�� ����
            {
                if (commandInput.EndsWith(skill.Key)) //Ŀ�ǵ�� ��ġ�ϴ� ��ų�̰�
                {
                    if (skill.Value) //�ر��� ��ų�̸�
                    {
                        Debug.Log(skill.Key); //��ų ���
                        commandInput = "";
                    }
                    return;
                }
            }
        }
    }

    IEnumerator CommandClear()
    {
        yield return new WaitForSeconds(5f);
        commandInput = "";
    }
}
