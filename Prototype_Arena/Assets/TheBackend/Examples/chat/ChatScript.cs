using BackEnd;
using BackEnd.Tcp;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChatScript : MonoBehaviour
{

    ModalPanel modalPanel;

    public GameObject guild_chat_alert_panel;
    public GameObject guild_chat_panel;

    public Toggle publicFilteringOn;
    //public Toggle guildFilteringOn;

    public InputField publicMessage;
    public InputField guildMessage;

    int maxLength = 225;

    static string BlockFailMsg = "존재하지 않는 닉네임입니다.";
    static string BlockSuccessMsg = "{0}님의 채팅을 차단합니다.";
    static string UnBlockFailMsg = "차단 목록에 존재하지 않는 닉네임입니다.";
    static string UnBlockSuccessMsg = "{0}님을 차단해제합니다.";

    static string CmdErrMsg = "잘못된 명령어 입니다.";
    static string NullMsg = "대상을 입력해주세요.";
    static string WhisperNicknameNullMsg = "귓속말 대상을 입력해주세요.";
    static string MessageNullMsg = "메세지를 입력해주세요.";
    static string MessageLengthExceedMsg = "입력 제한 글자수를 초과하였습니다.";
    static string WhisperNicknameErrorMsg = "자기자신에게 귓속말을 할 수 없습니다.";
    static string GlobalChatErrorMsg = "운영자가 아닙니다.";
    string mNickname;

    private static ChatScript chatScript;

    public static ChatScript Instance()
    {
        if (!chatScript)
        {
            chatScript = FindObjectOfType(typeof(ChatScript)) as ChatScript;
            if (!chatScript)
                Debug.LogWarning("There needs to be one active chatScript script on a GameObject in your scene.");
        }

        return chatScript;
    }
    // Use this for initialization
    void Start()
    {
        modalPanel = ModalPanel.Instance();

        Backend.Chat.SetFilterReplacementChar('뿅');

        publicFilteringOn.onValueChanged.AddListener(value =>
        {
            Debug.Log(value);
            Backend.Chat.SetFilterUse(value);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("LeaveChannel");
            LeaveChannel();
        }
    }

    public void ResetConnect()
    {
        Backend.Chat.ResetConnect();
    }
    public void PublicChat()
    {
        Chat(ChannelType.Public, publicMessage.text);
    }

    public void GuildChat()
    {
        Chat(ChannelType.Guild, guildMessage.text);
    }

    // public / guild chat
    private void Chat(ChannelType type, string message)
    {
        Chat(type, message, string.Empty, false);
    }

    private void Chat(ChannelType type, string message, string toNickname, bool IsWhisper)
    {
        if (Backend.IsInitialized)
        {
            // 글자수가 최대 글자수 넘는지 확인
            if (GetStringByte(message) > maxLength)
            {
                // 오류 모달
                ShowModal(MessageLengthExceedMsg);
            }
            // 글자수가 0 이상인 경우 채팅을 보냄
            else if (message.Length > 0)
            {

                if (type == ChannelType.Public)
                    publicMessage.text = string.Empty;
                else
                    guildMessage.text = string.Empty;

                // command 명령어인 경우
                if (message.StartsWith("/"))
                {
                    string[] messageSplit = message.Split(' ');

                    // public / guild 채널에서 명령어로 귓속말을 입력한 경우
                    if (IsWhisperCmd(messageSplit[0]))
                    {
                        //string[] messageSplit = message.Split(' ');
                        if (messageSplit.Length > 2)
                        {
                            string nickname = messageSplit[1];
                            if (nickname.Equals(mNickname))
                            {
                                ChatItem chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, WhisperNicknameErrorMsg, false);
                                ChatScroll.Instance().PopulateAll(chatItem);
                                return;
                            }
                            else
                            {
                                var wMesssageStart = messageSplit[0].Length + messageSplit[1].Length + 2;
                                if (wMesssageStart < message.Length)
                                {
                                    string wMessage = message.Substring(wMesssageStart);
                                    Backend.Chat.Whisper(nickname, wMessage);
                                    return;
                                }
                            }
                        }
                        CmdError(NullMsg);
                        return;
                    }
                    //글로벌쳇 보내기
                    if (IsGlobalChatCmd(messageSplit[0]))
                    {
                        if (messageSplit.Length > 1)
                        {
                            string nickname = messageSplit[1];
                            try
                            {
                                Debug.Log("할말 :" + messageSplit[1]);
                                Backend.Chat.ChatToGlobal(messageSplit[1]); // 운영자가 아니면 OnGlobalChat에서 에러로 리턴됩니다
                            }
                            catch (Exception e)
                            {
                                Debug.Log("에러 : " + e);
                            }
                            return;
                        }
                    }


                    // 명령어로 차단해제한 경우 
                    if (IsUnblockCmd(messageSplit[0]))
                    {
                        //string[] messageSplit = message.Split(' ');
                        if (messageSplit.Length > 1)
                        {
                            string nickname = messageSplit[1];
                            if (!string.IsNullOrEmpty(nickname))
                            {
                                UnBlockUser(nickname);
                                return;
                            }
                        }
                        CmdError(NullMsg);
                        return;
                    }

                    // 명령어로 차단한 경우 
                    if (IsBlockCmd(messageSplit[0]))
                    {
                        //string[] messageSplit = message.Split(' ');
                        if (messageSplit.Length > 1)
                        {
                            string nickname = messageSplit[1];
                            if (!string.IsNullOrEmpty(nickname))
                            {
                                BlockUser(nickname);
                                return;
                            }
                        }
                        CmdError(NullMsg);
                        return;
                    }

                    // 명령어로 신고한 경우
                    if (IsReportCmd(messageSplit[0]))
                    {
                        if (messageSplit.Length > 1)
                        {
                            string nickname = messageSplit[1];
                            if (!string.IsNullOrEmpty(nickname))
                            {
                                ReportUser(nickname);
                                return;
                            }
                        }
                        CmdError(NullMsg);
                        return;
                    }

                    // 존재하지 않는 명령어를 입력한 경우
                    CmdError(CmdErrMsg);
                    return;
                }

                // whisper pannel 에서 입력한 경우 
                if (!string.IsNullOrEmpty(toNickname))
                {
                    Backend.Chat.Whisper(toNickname, message);
                    return;
                }

                // public / guild pannel 에서 일반 메세지를 입력한 경우
                switch (type)
                {
                    case ChannelType.Public:
                        Backend.Chat.ChatToChannel(ChannelType.Public, message);
                        break;
                    case ChannelType.Guild:
                        Backend.Chat.ChatToChannel(ChannelType.Guild, message);
                        break;
                }
            }
            // 메세지를 입력하지 않은 경우 
            else
            {
                // 오류 모달
                ShowModal(MessageNullMsg);
            }
        }
    }

    private bool IsWhisperCmd(string message)
    {
        return message.Equals("/ㄱ")
                    || message.Equals("/w")
                    || message.Equals("/귓");
    }

    private bool IsBlockCmd(string message)
    {
        return message.Equals("/b")
                    || message.Equals("/차단");
    }

    private bool IsUnblockCmd(string message)
    {
        return message.Equals("/ub")
                      || message.Equals("/차단해제");
    }

    private bool IsReportCmd(string message)
    {
        return message.Equals("/신고");
    }
    private bool IsGlobalChatCmd(string message)
    {
        return message.Equals("/all");
    }

    private void CmdError(string message)
    {
        ChatItem chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, message, false);
        ChatScroll.Instance().PopulateAll(chatItem);
    }


    private void ShowModal(string message)
    {
        modalPanel.AlertShow(message);
    }

    private int GetStringByte(string message)
    {
        return System.Text.Encoding.Unicode.GetByteCount(message);
    }

    // 채널 나가기 
    public void LeaveChannel()
    {

        Backend.Chat.LeaveChannel(ChannelType.Public);
        publicMessage.text = string.Empty;
    }
    // 채널 나가기 
    public void LeaveGuildChannel()
    {
        Backend.Chat.LeaveChannel(ChannelType.Guild);

        guildMessage.text = string.Empty;
    }

    internal void BlockUser(string nickname)
    {
        ChatItem chatItem;
        Backend.Chat.BlockUser(nickname, blockCallback =>
        {
            // 성공
            if (blockCallback)
            {
                chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, string.Format(BlockSuccessMsg, nickname), false);
            }
            // 실패
            else
            {
                chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, BlockFailMsg, false);
            }

            ChatScroll.Instance().PopulateAll(chatItem);
        });
    }

    internal void UnBlockUser(string nickname)
    {
        ChatItem chatItem;

        // 성공
        if (Backend.Chat.UnblockUser(nickname))
        {
            chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, string.Format(UnBlockSuccessMsg, nickname), false);
        }
        // 실패
        else
        {
            chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, UnBlockFailMsg, false);
        }
        ChatScroll.Instance().PopulateAll(chatItem);
    }

    internal void ReportUser(string nickname)
    {
        ChatItem chatItem;
        Backend.Social.GetUserInfoByNickName(nickname, callback =>
        {
            if (callback.IsSuccess())
            {
                LitJson.JsonData rows = callback.GetReturnValuetoJSON()["rows"];
                if (rows.Count > 0)
                {
                    ParticipantsModal.Instance().SetSelectedNickname(nickname);
                    ParticipantsModal.Instance().OnReport();
                }
                else
                {
                    chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, BlockFailMsg, false);
                    ChatScroll.Instance().PopulateAll(chatItem);
                }
            }
            else
            {
                if (callback.GetStatusCode() == "404")
                {
                    chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, BlockFailMsg, false);
                    ChatScroll.Instance().PopulateAll(chatItem);
                }
                else
                {
                    Debug.Log("닉네임 검색 도중 오류가 발생하였습니다 : " + callback);
                    chatItem = new ChatItem(SessionInfo.None, ChatScroll.Instance().infoText, callback.ToString(), false);
                    ChatScroll.Instance().PopulateAll(chatItem);
                }

            }
        });
    }
}

