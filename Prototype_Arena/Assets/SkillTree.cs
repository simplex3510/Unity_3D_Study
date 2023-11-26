using TMPro;
using UnityEngine;

public class SkillTree : MonoBehaviour
{

    [SerializeField]
    private int skillPoints;
    [SerializeField]
    private Skill[] skills;
    [SerializeField]
    private TextMeshProUGUI skillPointText;
    [SerializeField]
    private command player;

    public int Point
    {
        get
        {
            return skillPoints;
        }
        set
        {
            skillPoints = value;
            UpdateSkillPoint();
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        skillPoints = 10;
        UpdateSkillPoint();
        skills = GetComponentsInChildren<Skill>();
        ResetSkills();
    }

    public void TryUseSkillPoint(Skill skill) //��ų ������
    {
        if(skillPoints > 0 && skill.Check())
        {
            skillPoints--;
            UpdateSkillPoint();
            skill.UpdateSkillPoint();
            ResetSkills();
        }
    }

    private void ResetSkills() //��ų �ر�
    {
        foreach (Skill skill in skills)
        {
            if(skill.UnLocked)
                skill.UnLock();
            else
                skill.Lock();
        }
    }

    private void UpdateSkillPoint() //���� SP ���
    {
        skillPointText.text = skillPoints.ToString();
    }
}
