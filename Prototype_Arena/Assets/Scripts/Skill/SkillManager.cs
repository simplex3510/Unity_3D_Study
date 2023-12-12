using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;
    public GameObject player;
    private PlayableCharacterController playableCharacterController;
    public GameObject[] choices = new GameObject[3];
    public List<int> LevelUpSkill;
    public GameObject LevelBase;
    public Text CommandText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        playableCharacterController = player.GetComponent<PlayableCharacterController>();
    }

    private void Update()
    {
        CommandText.text = "Command: " + player.GetComponent<PlayableCharacterController>().SkillCammand;
    }

    public void LevelUp()
    {
        playableCharacterController.SkillCammand = "";
        int choice = 0;
        LevelUpSkill = new List<int>();
        while (choice < 3)
        { 
            int rand = Random.Range(0, playableCharacterController.UnlockSkills.Count);
            if(LevelUpSkill.Contains(rand))
            {
                continue;
            }
            else
            {
                LevelUpSkill.Add(rand);
                choice++;
            }
        }

        SetUI();
    }

    void SetUI()
    {
        for(int i = 0; i < choices.Length; i++)
        {
            choices[i].transform.GetChild(0).GetComponent<Image>().sprite = playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillImage;

            choices[i].transform.GetChild(1).GetComponent<Text>().text =
                playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillName.ToString() + '(' + playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillCommand.ToString() + ')' + " Lv." + playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillLevel;

            if (playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillLevel == 0)
                choices[i].transform.GetChild(2).GetComponent<Text>().text = playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillDescription.ToString();
            else if (playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillLevel < playableCharacterController.UnlockSkills[LevelUpSkill[i]].maxLevel)
                choices[i].transform.GetChild(2).GetComponent<Text>().text = "데미지가 " + (playableCharacterController.UnlockSkills[LevelUpSkill[i]].baseDamage + playableCharacterController.UnlockSkills[LevelUpSkill[i]].ad * playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillLevel) + "에서 " + (playableCharacterController.UnlockSkills[LevelUpSkill[i]].baseDamage + playableCharacterController.UnlockSkills[LevelUpSkill[i]].ad * (playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillLevel + 1)) + "로 증가합니다.";
            else
                choices[i].transform.GetChild(2).GetComponent<Text>().text = "이 스킬은 마스터하셨습니다.";

            if (playableCharacterController.UnlockSkills[LevelUpSkill[i]].skillLevel < playableCharacterController.UnlockSkills[LevelUpSkill[i]].maxLevel)
            {
                choices[i].transform.GetChild(3).GetComponentInChildren<Text>().text = "Level Up";
            }
            else
            {
                choices[i].transform.GetChild(3).GetComponentInChildren<Text>().text = "Close";
            }
        }

        LevelBase.SetActive(true);
    }

    public void choiceLevelUpSkill(int num)
    {
        if(playableCharacterController.UnlockSkills[LevelUpSkill[num]].skillLevel < playableCharacterController.UnlockSkills[LevelUpSkill[num]].maxLevel)
            playableCharacterController.UnlockSkills[LevelUpSkill[num]].skillLevel++;

        for(int i = 0; i < playableCharacterController.UnlockSkills[LevelUpSkill[num]].childSkill.Length; i++)
        {
            if (!playableCharacterController.UnlockSkills.Contains(playableCharacterController.UnlockSkills[LevelUpSkill[num]].childSkill[i]))
            {
                    playableCharacterController.UnlockSkills.Add(playableCharacterController.UnlockSkills[LevelUpSkill[num]].childSkill[i]);
            }

        }

        LevelBase.SetActive(false);
    }
}
