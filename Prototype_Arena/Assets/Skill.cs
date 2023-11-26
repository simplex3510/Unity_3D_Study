using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    private Image sprite;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private Image levelImage;
    [SerializeField]
    private int maxLevel = 5;
    [SerializeField]
    private int nowLevel = 0;
    [SerializeField]
    public bool unlocked;
    [SerializeField]
    private Skill childTalent;
    [SerializeField]
    private string command;
    [SerializeField]
    private command player;
    public int NowLevel
    {
        get
        {
            return nowLevel;
        }
        set
        {
            nowLevel = value;
        }
    }

    public bool UnLocked
    {
        get
        {
            return unlocked;
        }
        set
        {
            unlocked = value;
        }
    }
    void Start()
    {
        sprite = GetComponent<Image>();
        
        if (unlocked)
        {
            UnLock();
        }
    }
    
    public bool Check() //�������� ������ ��ų���� �Ǵ��ϰ� ���� ��ų�� �ر�
    {
        if (nowLevel < maxLevel && unlocked)
        {
            nowLevel++;
            player.CommandInput = "";
            if(nowLevel == 1)
            {
                player.commandList.Add(command, true);
            }

            if(0 < nowLevel)
            {
                if(childTalent != null)
                {
                    childTalent.UnLocked = true;
                }
            }

            return true;
        }
        else
            return false;
    }
    public void UpdateSkillPoint() //��ų�� ���� ���
    {
        levelText.text = nowLevel.ToString() + "/" + maxLevel.ToString();
    }
    public void Lock() //��ų�� ��� ����
    {
        sprite.enabled = false;
        levelImage.enabled = false;
        levelText.enabled = false;
    }
    public void UnLock() // ��ų �ر�
    {
        sprite.enabled = true;
        levelImage.enabled = true;
        levelText.enabled = true;
    }
}