using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using BackEnd.Tcp;
using LitJson;
public class ChannelListManager : MonoBehaviour
{

    public Toggle publicToggle;
    public Toggle guildToggle;
    public GameObject searchPannel;
    public InputField searchInputField;
    public Button searchButton;

    public GameObject publicChatModal;
    public GameObject guildChatModal;

    public GameObject guildCreatePanel;

    // [5.5.0 재접속 로직] 가장 최근에 접속한 채팅채널 [0] = 일반, [1] = 길드
    public ChannelNodeObject[] reconnectChannel = new ChannelNodeObject[2];

    // [5.5.0 재접속 로직] 채널 요청을 Queue에 담아 동시 호출되지 않게 조정
    Queue<Action> channelJoinManager = new Queue<Action>();

    ChannelType channelType = ChannelType.Public;

    // Start is called before the first frame update
    public GameObject channelScrollView;

    public Text public_channel_alias;
    public Text guild_channel_alias;

    public Text alert;

    private static ChannelListManager channelListManager;
    public static ChannelListManager Instance()
    {
        if (!channelListManager)
        {
            channelListManager = FindObjectOfType(typeof(ChannelListManager)) as ChannelListManager;
            if (!channelListManager)
                Debug.LogWarning("There needs to be one active ChannelListScript script on a GameObject in your scene.");
        }

        return channelListManager;
    }

    private List<ChannelNodeObject> channelList = new List<ChannelNodeObject>();
    internal bool chatStatus = false;
    readonly string CHAT_INACTIVE = "채팅 서비스가 비활성화 상태입니다. 콘솔에서 활성화 시켜주세요.";

    readonly string CHAT_GUILD = "소속된 길드가 존재하지 않습니다.";

    readonly string CHAT_NOT_FOUND = "해당 이름의 그룹이 존재하지 않습니다.";

    private ModalPanel modalPanel;
    ChannelGridScroll populateGrid;

    bool isJoiningChannel = false;

    bool isPublicConnect = false;
    bool isGuildConnect = false;



    // Use this for initialization
    void Start()
    {

        modalPanel = ModalPanel.Instance();

        Screen.SetResolution(Screen.width, Screen.height, true);

        populateGrid = ChannelGridScroll.Instance();

        if (!Backend.IsInitialized)
        {
            Backend.Initialize(true);
        }

        GetChatStatus();

        publicToggle.onValueChanged.AddListener(delegate { getPublicToggle(); });
        guildToggle.onValueChanged.AddListener(delegate { getGuildToggle(); });
        searchButton.onClick.AddListener(delegate { SearchChannel(); });
    }

    public void SetIsJoiningChannel(bool isJoining)
    {
        isJoiningChannel = isJoining;
    }

    public void GetChatStatus()
    {
        BackendReturnObject chatStatusBRO = Backend.Chat.GetChatStatus();
        if (chatStatusBRO.IsSuccess())
        {
            alert.gameObject.SetActive(false);
            channelScrollView.SetActive(true);
            string yn = chatStatusBRO.GetReturnValuetoJSON()["chatServerStatus"]["chatServer"].ToString();
            chatStatus |= yn.Equals("y");
            Debug.Log("chatStatus: " + chatStatus);

            if (chatStatus)
            {
                alert.gameObject.SetActive(false);
                searchPannel.SetActive(true);
            }
            else
            {
                alert.text = CHAT_INACTIVE;
                alert.gameObject.SetActive(true);
            }
        }
        else
        {
            alert.text = chatStatusBRO.GetMessage();
            alert.gameObject.SetActive(true);
            populateGrid.RemoveAllListViewItem();
        }
    }


    void SearchChannel()
    {
        // 길드 생성창 해제(가입 성공 후, 검색 시 길드 생성창이 사라지지 않는 문제 해결용)
        guildCreatePanel.SetActive(false);

        // 그룹 네임 받기
        string groupName = searchInputField.text;

        if (chatStatus)
        {
            BackendReturnObject bro = null;

            bro = Backend.Chat.GetGroupChannelList(groupName);
            Debug.Log(bro);
            if (bro.IsSuccess())
            {
                channelList.Clear();
                JsonData rows = bro.GetReturnValuetoJSON()["rows"];
                ChannelNodeObject channelNode;
                for (int i = 0; i < rows.Count; i++)
                {
                    JsonData data = rows[i];
                    channelNode = new ChannelNodeObject(channelType, groupName, data["inDate"].ToString(), (int)data["joinedUserCount"], (int)data["maxUserCount"],
                                                        data["serverAddress"].ToString(), data["serverPort"].ToString(), data["alias"].ToString());
                    Debug.Log("이름 : " + data["alias"].ToString() + "\n inDate : " + data["inDate"].ToString());

                    channelList.Add(channelNode);

                }
                alert.gameObject.SetActive(false);
                populateGrid.PopulateChannel(channelList);

            }
            // Client Error
            else
            {
                // 채팅을 뒤끝콘솔에서 활성화하지 않은 경우
                if (bro.GetStatusCode() == "412")
                {
                    if (bro.GetMessage().Contains("notGuildMember"))
                    {
                        alert.text = CHAT_GUILD;
                        guildCreatePanel.SetActive(true);
                    }
                    else
                    {
                        alert.text = CHAT_INACTIVE;
                    }
                    alert.gameObject.SetActive(true);

                }
                else if (bro.GetStatusCode() == "404")
                {
                    populateGrid.RemoveAllListViewItem();

                    alert.text = CHAT_NOT_FOUND;
                    alert.gameObject.SetActive(true);
                    Debug.LogError("해당 그룹명이 없습니다");
                }
                else
                {
                    showmodal(bro.GetStatusCode() + "\n" + bro.GetErrorCode() + "\n" + bro.GetMessage());

                }
            }
        }
    }

    void getPublicToggle()
    {

        if (publicToggle.isOn)
        {
            channelType = ChannelType.Public;
            Refresh();
            // 이미 서버에 연결된 채널이 있을 경우
            if (isPublicConnect)
            {
                publicChatModal.SetActive(true);
                guildChatModal.SetActive(false);

            }
            else
            {
                publicChatModal.SetActive(false);
                guildChatModal.SetActive(false);
            }
        }
    }

    void getGuildToggle()
    {
        if (guildToggle.isOn)
        {
            channelType = ChannelType.Guild;
            Refresh();

            // 이미 서버에 연결된 채널이 있을 경우
            if (isGuildConnect)
            {
                publicChatModal.SetActive(false);
                guildChatModal.SetActive(true);
            }
            else
            {
                publicChatModal.SetActive(false);
                guildChatModal.SetActive(false);
            }
        }
    }

    void Refresh()
    {
        alert.gameObject.SetActive(false);
        guildCreatePanel.SetActive(false);


        populateGrid.RemoveAllListViewItem();
        searchInputField.text = string.Empty;
    }

    public void GuildIsConnect(bool isConnect)
    {
        isGuildConnect = isConnect;

    }

    public void PublicIsConnect(bool isConnect)
    {
        isPublicConnect = isConnect;

    }

    void AlreadyFull()
    {
        showmodal("인원이 꽉찬 방입니다.");
    }

    internal void showmodal(string message)
    {
        modalPanel.AlertShow(message);
    }

    // [5.5.0 재접속 로직] Queue에 삽입하고 진행중인 접속이 없다면 바로 실행
    public void JoinChannel(ChannelNodeObject channelNode)
    {
        if (isJoiningChannel)
        {
            Debug.Log("이미 접속이 시도중입니다.");
            return;
        }
        EnqueueJoinChannel(channelNode);
        DequeueJoinChannel();
    }

    // [5.5.0 재접속 로직] 기존에 바로 실행하던 JoinChannel 함수 Queue를 통해 실행하도록 변경
    internal void JoinChannelInternal(ChannelNodeObject channelNode)
    {
        // 인원수 체크 
        if (channelNode.joinedUserCount >= channelNode.maxUserCount)
        {
            AlreadyFull();
        }
        else
        {
            ErrorInfo info;
            Debug.Log(string.Format("입장 : {0} / {1} / {2} / {3} / {4}", channelNode.type, channelNode.host, channelNode.port, channelNode.groupName, channelNode.inDate));

            Backend.Chat.JoinChannel(channelNode.type, channelNode.host, channelNode.port, channelNode.groupName, channelNode.inDate, out info);
            switch (channelNode.type)
            {
                case ChannelType.Public:
                    public_channel_alias.text = channelNode.alias;
                    reconnectChannel[0] = channelNode; // [5.4.5 재접속 로직] 가장 최근에 접속한 일반[0]채널로 저장
                    break;
                case ChannelType.Guild:
                    guild_channel_alias.text = channelNode.alias;
                    reconnectChannel[1] = channelNode; // [5.4.5 재접속 로직] 가장 최근에 접속한 길드[1]채널로 저장
                    break;
            }
            if (info.Category != ErrorCode.Success)
            {
                isJoiningChannel = false;
                Debug.Log(info);
            }
        }
    }

    // [5.5.0 재접속 로직] Queue에 채널 접속 함수 삽입
    public void EnqueueJoinChannel(ChannelNodeObject channelNode)
    {
        channelJoinManager.Enqueue(() =>
        {
            JoinChannelInternal(channelNode);
        }
        );
    }

    // [5.5.0 재접속 로직] Queue에 저장된 채널 접속 함수 호출 및 제거
    public void DequeueJoinChannel()
    {
        if (isJoiningChannel)
        {
            Debug.LogError("이미 접속 시도중입니다!");
            return;
        }
        if (channelJoinManager.Count > 0)
        {
            isJoiningChannel = true;
            var action = channelJoinManager.Dequeue();
            action();
        }

    }

    // [5.5.0 재접속 로직] 이전에 접속한 후 저장해놓았던 채팅 채널을 이용하여 다시 접속
    public void ReJoinChannel(ChannelType channelType)
    {
        switch (channelType)
        {
            case ChannelType.Public:
                EnqueueJoinChannel(reconnectChannel[0]);
                break;
            case ChannelType.Guild:
                EnqueueJoinChannel(reconnectChannel[1]);
                break;
        }
        if (isJoiningChannel)
        {
            Debug.Log("이미 접속 시도중입니다. 채널 접속이 끝난 후 다시 시도합니다.");
            return;
        }
        DequeueJoinChannel();
    }
}

public class ChannelNodeObject
{
    public ChannelType type;
    public string groupName;
    public string inDate;
    public string participants;
    public int joinedUserCount;
    public int maxUserCount;
    public string host;
    public ushort port;
    public string alias;

    public ChannelNodeObject(ChannelType type, string groupName, string inDate, int joinedUser, int maxUser, string host, string port, string alias)
    {
        this.type = type;
        this.groupName = groupName;
        this.inDate = inDate;
        this.joinedUserCount = joinedUser;
        this.maxUserCount = maxUser;
        this.participants = joinedUser + "/" + maxUser;
        this.host = host;
        this.port = ushort.Parse(port);
        this.alias = alias;
    }
}