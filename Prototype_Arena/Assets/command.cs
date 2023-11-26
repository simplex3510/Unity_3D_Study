using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class command : MonoBehaviour
{
    [SerializeField]
    private string commandInput = ""; //버퍼로 사용
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

    void GetInput() //클릭 입력 받기
    {
        LClick = Input.GetMouseButtonDown(0);
        RClick = Input.GetMouseButtonDown(1);
        isDash = Input.GetKeyDown(KeyCode.LeftShift);

        if(commandInput == "" && (LClick || RClick)) //콤보가 시작되면
            StartCoroutine(CommandClear()); //몇 초 후 콤보 버퍼를 비움
        else if(commandInput != "" && (LClick || RClick)) //콤보를 이어가면
        {
            StopCoroutine(CommandClear()); //초기화되는 시간 초기화
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
        if (commandInput != "" && (LClick || RClick)) //비어있는 콤보는 무시
        {
            StopCoroutine(CommandClear()); //버퍼를 비우면 코루틴 중단

            foreach (var skill in commandList) //모든 스킬에 대해
            {
                if (commandInput.EndsWith(skill.Key)) //커맨드와 일치하는 스킬이고
                {
                    if (skill.Value) //해금한 스킬이면
                    {
                        Debug.Log(skill.Key); //스킬 사용
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
