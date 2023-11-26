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

    public void TryUseSkillPoint(Skill skill) //스킬 레벨업
    {
        if(skillPoints > 0 && skill.Check())
        {
            skillPoints--;
            UpdateSkillPoint();
            skill.UpdateSkillPoint();
            ResetSkills();
        }
    }

    private void ResetSkills() //스킬 해금
    {
        foreach (Skill skill in skills)
        {
            if(skill.UnLocked)
                skill.UnLock();
            else
                skill.Lock();
        }
    }

    private void UpdateSkillPoint() //남은 SP 출력
    {
        skillPointText.text = skillPoints.ToString();
    }
}
