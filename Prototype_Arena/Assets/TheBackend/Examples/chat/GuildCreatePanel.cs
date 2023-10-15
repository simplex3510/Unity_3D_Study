using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using UnityEngine.UI;

public class GuildCreatePanel : MonoBehaviour
{
    public InputField guildNameInput;
    public Text guild_alert;

    readonly string CREATE_SUCCESS = "가입 성공했습니다.";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateGuild()
    {
        //입력된 길드이름과 5개의 굿즈로생성
        Backend.Guild.CreateGuildV3(guildNameInput.text, 5, callback =>
        {
            Debug.Log("CreateGuildV3 : " + callback);

            guild_alert.gameObject.SetActive(true);
            if (callback.IsSuccess())
            {
                guild_alert.text = CREATE_SUCCESS;
            }
            else
            {
                guild_alert.text = callback.GetMessage();
            }
        });
    }

    void OnDisable()
    {
        guild_alert.gameObject.SetActive(false);
        guildNameInput.text = string.Empty;

    }


}
