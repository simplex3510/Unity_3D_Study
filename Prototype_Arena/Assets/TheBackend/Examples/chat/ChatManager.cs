using UnityEngine;
using BackEnd;
using System;
using UnityEngine.SceneManagement;
using BackEnd.Tcp;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    private ChatItem chatItem;
    public GameObject publicChatChannel;
    public GameObject guildChatChannel;

    int sceneIdx;
    bool ChatJoind = false;
    string myNickname = "";

    // Use this for initialization
    void Start()
    {
        Backend.Initialize(true);
        ChatHandlers();
        sceneIdx = SceneManager.GetActiveScene().buildIndex;

    }

    protected void Update()
    {
        Backend.Chat.Poll();
        Backend.AsyncPoll();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ChatJoind)
            {
                if (Backend.Chat.IsChatConnect(ChannelType.Public))
                    Backend.Chat.LeaveChannel(ChannelType.Public);

                if (Backend.Chat.IsChatConnect(ChannelType.Guild))
                    Backend.Chat.LeaveChannel(ChannelType.Guild);

            }
            else
            {
                SceneManager.LoadScene(sceneIdx - 1);
            }
        }
    }

    void ChatHandlers()
    {
        Backend.Chat.SetRepeatedChatBlockMessage("도배하면 안돼요.");

        Backend.Chat.SetTimeoutMessage("챗 안해서 쫒아냅니다.");

        ChatScroll chatScroll = ChatScroll.Instance();
        ChatParticipantsScroll participantsScroll = ChatParticipantsScroll.Instance();

        // 채널에 입장시 해당 채널에 접속해있는 모든 게이머들의 정보이며, 입장시 최초 한번 콜백
        Backend.Chat.OnSessionListInChannel = (args) =>
        {
            Debug.Log("OnSessionListInChannel");
            participantsScroll.public_participants.Clear();

            List<string> nameList = new List<string>();


            // 게이머 정보를 참여자 리스트에 추가
            foreach (SessionInfo session in args.SessionList)
            {
                bool isExist = false;
                foreach (var session2 in participantsScroll.public_participants) // 닉네임으로 검색하여 중복이 있는지 체크(안해도 무관)
                {
                    Debug.Log(session.NickName);
                    if (session2.NickName == session.NickName)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (!isExist)
                {
                    participantsScroll.public_participants.Add(session);
                }
            }
            // 참여자 목록 출력
            participantsScroll.PublicPopulateList();
        };

        // 채널에 입장시 해당 채널에 접속해있는 모든 게이머들의 정보이며, 입장시 최초 한번 콜백
        Backend.Chat.OnSessionListInGuildChannel = (args) =>
        {
            participantsScroll.guild_participants.Clear();
            // 게이머 정보를 참여자 리스트에 추가
            foreach (SessionInfo session in args.SessionList)
            {
                bool isExist = false;
                foreach (var session2 in participantsScroll.guild_participants) // 닉네임으로 검색하여 중복이 있는지 체크(안해도 무관)
                {
                    if (session2.NickName == session.NickName)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (!isExist)
                {
                    participantsScroll.guild_participants.Add(session);
                }
            }
            // 참여자 목록 출력
            participantsScroll.GuildPopulateList();
        };

        // 룸에 입장 혹은 다른 게이머가 룸에 입장하면 콜백
        Backend.Chat.OnJoinChannel = (JoinChannelEventArgs args) =>
        {
            Debug.Log(string.Format("OnJoinChannel {0}", args.ErrInfo));

            if (args.ErrInfo == ErrorInfo.Success)
            {

                // 내가 접속한 경우 
                if (!args.Session.IsRemote)
                {
                    // [5.5.0 재접속 로직] 접속중 bool 값 false로 변환
                    ChannelListManager.Instance().SetIsJoiningChannel(false);

                    ChannelListManager.Instance().PublicIsConnect(true);
                    publicChatChannel.SetActive(true);
                    ChatJoind = true;

                    // 접속할 때 마다 필터링 리스트 확인
                    Backend.Chat.SetFilterUse(ChatScript.Instance().publicFilteringOn.isOn);

                    // [5.5.0 재접속 로직] 남아있는 접속 요청 진행
                    ChannelListManager.Instance().DequeueJoinChannel();

                }

                // 접속한 세션이 참여자에 존재하지 않을 때 -> 추가
                if (!participantsScroll.public_participants.Contains(args.Session))
                {
                    bool isExist = false;
                    foreach (var session in participantsScroll.public_participants) // 참가자 목록중 이름이 이미 있는지 확인
                    {
                        if (session.NickName == args.Session.NickName) // 있으면 true로 변경
                        {
                            isExist = true;
                            break;
                        }
                    }

                    if (!isExist && args.Session.IsRemote) // 입장한 사람이 내가 아니거나, 이름이 중복되있지 않다면 목록에 추가
                    {
                        participantsScroll.public_participants.Add(args.Session);
                        participantsScroll.PublicPopulate(args.Session);
                    }
                    // 입장 안내 메세지 view
                    chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, string.Format("{0}이 입장했습니다.", args.Session.NickName), false);
                    chatScroll.PopulatePublicChat(chatItem);
                }
            }
            else
            {
                // [5.5.0 재접속 로직] 채널의 인원이 0일 때, 30초 이상이 지나면 해당 채널은 폐기가 됩니다.
                // 채널이 폐기될 경우, 채널 리스트를 다시 검색하여 다시 불러온 채널 리스트를 이용하시기 바랍니다.
                if (args.ErrInfo.Reason.Contains("Can't find channel"))
                {
                    ShowModal("채널을 찾을 수 없습니다.\n채널리스트를 다시 불러 접속해주시기 바랍니다.");
                }
                else
                {
                    ShowModal(string.Format("OnJoinChannel: {0}", args.ErrInfo.Reason));
                    Backend.Chat.ResetConnect();
                }
                if (args.ErrInfo.Reason.Contains("Rars"))
                {
                    Backend.Chat.ResetConnect();
                }
                // [5.5.0 재접속 로직] 접속에 실패했을 경우
                ChannelListManager.Instance().SetIsJoiningChannel(false); //접속진행중 false로 변환
                publicChatChannel.SetActive(false); // 재접속 시도중일시 비활성화하지 않음
                ChannelListManager.Instance().DequeueJoinChannel(); // 접속 요청이 남아있을 경우 실행
            }
        };

        Backend.Chat.OnJoinGuildChannel = (JoinChannelEventArgs args) =>
        {
            Debug.Log(string.Format("OnJoinGuildChannel {0}", args.ErrInfo));

            if (args.ErrInfo == ErrorInfo.Success)
            {
                if (!args.Session.IsRemote)
                {
                    // [5.5.0 재접속 로직] 접속중 bool 값 false로 변환
                    ChannelListManager.Instance().SetIsJoiningChannel(false);
                    ChannelListManager.Instance().GuildIsConnect(true);

                    guildChatChannel.SetActive(true);
                    ChatJoind = true;

                    // 접속할 때 마다 필터링 리스트 확인
                    Backend.Chat.SetFilterUse(ChatScript.Instance().publicFilteringOn.isOn);
                    // [5.5.0 재접속 로직] 남아있는 접속 요청 진행
                    ChannelListManager.Instance().DequeueJoinChannel();


                }
                // 접속한 세션이 참여자에 존재하지 않을 때 -> 추가
                if (!participantsScroll.guild_participants.Contains(args.Session))
                {
                    bool isExist = false;
                    foreach (var session in participantsScroll.guild_participants) // 사용자중에 이름이 중복된 유저가 있는지 확인
                    {
                        if (session.NickName == args.Session.NickName) // 있으면 true로 변경
                        {
                            isExist = true;
                            break;
                        }
                    }

                    if (!isExist && args.Session.IsRemote) // 추가할 사람이 자신이 아니면서 목록에 없으면 추가
                    {
                        participantsScroll.guild_participants.Add(args.Session);
                        participantsScroll.GuildPopulate(args.Session);
                    }
                    // 입장 안내 메세지 view
                    chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, string.Format("{0}이 입장했습니다.", args.Session.NickName), false);
                    chatScroll.PopulateGuildChat(chatItem);
                }
            }
            else
            {
                if (args.ErrInfo.Reason.Contains("Can't find channel"))
                {
                    ShowModal("채널을 찾을 수 없습니다.\n채널리스트를 다시 불러 접속해주시기 바랍니다.");
                }
                else
                {
                    ShowModal(string.Format("OnJoinGuildChannel: {0}", args.ErrInfo.Reason));
                }
                if (args.ErrInfo.Reason.Contains("Rars"))
                {
                    Backend.Chat.ResetConnect();
                }
                ChannelListManager.Instance().SetIsJoiningChannel(false); //접속진행중 false로 변환
                guildChatChannel.SetActive(false); // 재접속 시도중일시 비활성화하지 않음
                ChannelListManager.Instance().DequeueJoinChannel(); // 접속 요청이 남아있을 경우 실행
            }
        };

        // 채널에서 퇴장 혹은 다른 게이머가 채널에서 퇴장하면 콜백
        Backend.Chat.OnLeaveChannel = (LeaveChannelEventArgs args) =>
        {
            Debug.Log(string.Format("OnLeaveChannel {0}", args.ErrInfo));
            // 퇴장한 사람을 참여자 목록에서 삭제
            participantsScroll.PublicDePopulate(args.Session);
            // 내가 채널에서 퇴장한 경우
            if (!args.Session.IsRemote)
            {
                try
                {
                    ChannelListManager.Instance().PublicIsConnect(false);

                    // 활성상태인 모달이 존재하는 경우, 모두 닫기 
                    ParticipantsModal.Instance().CloseAll();

                    // [5.5.0 재접속 로직] 일시적으로 연결이 끊겨서 채널에서 나가졌을 경우(기존 OnSessionOfflineChannel)
                    // [5.5.0 재접속 로직] 재접속 로직(재접속을 원하지 않을경우 추가 X)
                    if (args.ErrInfo.Detail == ErrorCode.DisconnectFromRemote)
                    {
                        Debug.Log("일반 채널이 일시적으로 나가졌습니다. 재접속을 시도합니다.");
                        ChannelListManager.Instance().ReJoinChannel(ChannelType.Public);
                        // [5.5.0 재접속 로직] 다시 접속 요청을 하는 것이기 때문에 RecentLog가 재호출되면서 로그가 중첩으로 쌓이거나 이전 기록과 차이가 발생할 수 있음.
                        chatScroll.RemoveAllPublicListViewItem();

                    }
                    // [5.5.0 재접속 로직] 재접속이 아닌 일반 종료일 경우 혹은 재접속 외에 다른 오류로 인해 나가질 경우
                    else
                    {
                        publicChatChannel.SetActive(false);
                        chatScroll.RemoveAllPublicListViewItem();
                    }
                }
                finally
                {
                    ChannelListManager.Instance().SetIsJoiningChannel(false);
                    ChatJoind = false;
                }

            }
            // 다른사람이 채널에서 퇴장한 경우
            else
            {
                chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, string.Format("{0}이 퇴장했습니다.", args.Session.NickName), false);
                chatScroll.PopulatePublicChat(chatItem);
                participantsScroll.PublicDePopulate(args.Session);
            }
        };

        Backend.Chat.OnLeaveGuildChannel = (LeaveChannelEventArgs args) =>
        {
            Debug.Log(string.Format("OnLeaveGuildChannel {0}", args.ErrInfo));
            // 퇴장한 사람을 참여자 목록에서 삭제
            participantsScroll.GuildDePopulate(args.Session);
            // 내가 채널에서 퇴장한 경우
            if (!args.Session.IsRemote)
            {
                try
                {
                    ChannelListManager.Instance().GuildIsConnect(false);
                    // 활성상태인 모달이 존재하는 경우, 모두 닫기 
                    ParticipantsModal.Instance().CloseAll();

                    // [5.5.0 재접속 로직] 일시적으로 연결이 끊겨서 채널에서 나가졌을 경우(OnSessionOfflineGuildChannel)
                    // [5.5.0 재접속 로직] 재접속 로직(재접속을 원하지 않을경우 추가 X)
                    if (args.ErrInfo.Detail == ErrorCode.DisconnectFromRemote)
                    {
                        Debug.Log("길드 채널이 일시적으로 끊어졌습니다. 재접속을 시도합니다.");

                        // [5.5.0 재접속 로직] 길드의 재접속은 일반채널의 재접속이 끝난 뒤에 이루어지도록 하기 위해 여기서는 재접속해야된다는 bool만 true로 바뀐다
                        ChannelListManager.Instance().ReJoinChannel(ChannelType.Guild);

                        // [5.5.0 재접속 로직] 다시 접속 요청을 하는 것이기 때문에 RecentLog가 재호출되면서 로그가 중첩으로 쌓이거나 이전 기록과 차이가 발생할 수 있음.
                        chatScroll.RemoveAllGuildListViewItem();
                    }
                    // [5.5.0 재접속 로직] 재접속이 아닌 일반 종료일 경우 혹은 재접속 외에 다른 오류로 인해 나가질 경우
                    else
                    {
                        guildChatChannel.SetActive(false);
                        chatScroll.RemoveAllGuildListViewItem();
                    }
                }
                finally
                {
                    ChatJoind = false;
                }
            }
            // 다른사람이 채널에서 퇴장한 경우
            else
            {
                chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, string.Format("{0}이 퇴장했습니다.", args.Session.NickName), false);
                chatScroll.PopulateGuildChat(chatItem);
            }
        };

        /*
                //[5.5.0 deprecated] 재접속 로직의 경우, OnJoinChannel과 OnLeaveChannel 핸들러를 참고해주세요
                //클라이언트의 네트워크 상황이 좋지않거나, 어떠한 이유로 인해 Poll 함수가 주기적으로 호출되지 못한 경우 서버에서 해당 유저를 오프라인하고 이 때 콜백되는 함수
                Backend.Chat.OnSessionOfflineChannel = (SessionOfflineEventArgs args) =>
                {
                    Debug.Log(string.Format("OnSessionOfflineChannel {0}", args.ErrInfo));

                    // 내가 채널에서 끊겼을 경우
                    if (!args.Session.IsRemote)
                    {
                        chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, "OnSessionOfflineChannel : 일시적으로 접속이 끊겼습니다", false);
                        chatScroll.PopulatePublicChat(chatItem);
                        Backend.Chat.LeaveChannel(ChannelType.Guild);

                    }
                    // 다른사람이 채널에서 끊겼을 경우
                    else
                    {
                        chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, string.Format("{0}이 OnSessionOfflineChannel로 퇴장했습니다.", args.Session.NickName), false);
                        chatScroll.PopulatePublicChat(chatItem);
                    }
                };

                //오프라인 상태에서 클라이언트의 네트워크 상황이 다시 좋아지거나, Poll 함수가 다시 주기적으로 호출되는 경우 서버에서 해당 유저를 온라인 처리하고 콜백되는 함수
                Backend.Chat.OnSessionOnlineChannel = (SessionOnlineEventArgs args) =>
                {
                    Debug.Log(string.Format("OnSessionOnlineChannel {0}", args.ErrInfo));
                    // 내가 채널에 다시 접속될 경우
                    if (!args.Session.IsRemote)
                    {

                        chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, "OnSessionOnlineChannel로 접속되었습니다", false);
                        chatScroll.PopulatePublicChat(chatItem);


                    }
                    // 다른사람이 채널에 다시 접속될 경우
                    else
                    {
                        chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, string.Format("{0}이 OnSessionOnlineChannel로 접속되었습니다.", args.Session.NickName), false);
                        chatScroll.PopulatePublicChat(chatItem);
                    }
                };
        */


        // 채팅 왔을 때
        Backend.Chat.OnChat = (ChatEventArgs args) =>
        {
            Debug.Log(string.Format("OnChat {0}, {1}", DateTime.Now.TimeOfDay, args.Message));
            if (args.ErrInfo == ErrorInfo.Success)
            {
                chatItem = new ChatItem(args.From, args.From.NickName, args.Message, args.From.IsRemote);
                chatScroll.PopulatePublicChat(chatItem);
            }
            else if (args.ErrInfo.Category == ErrorCode.BannedChat)
            {
                // 도배방지 메세지 
                if (args.ErrInfo.Detail == ErrorCode.BannedChat)
                {
                    chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, args.ErrInfo.Reason, false);
                    chatScroll.PopulatePublicChat(chatItem);
                }
            }

        };

        Backend.Chat.OnGuildChat = (ChatEventArgs args) =>
        {
            //Debug.Log(string.Format("OnChat {0}", args.ErrInfo));
            if (args.ErrInfo == ErrorInfo.Success)
            {
                chatItem = new ChatItem(args.From, args.From.NickName, args.Message, args.From.IsRemote);
                chatScroll.PopulateGuildChat(chatItem);
            }
            else if (args.ErrInfo.Category == ErrorCode.BannedChat)
            {
                // 도배방지 메세지 
                if (args.ErrInfo.Detail == ErrorCode.BannedChat)
                {
                    chatItem = new ChatItem(args.From, chatScroll.infoText, args.ErrInfo.Reason, false);
                    chatScroll.PopulateGuildChat(chatItem);
                }
            }

        };

        string NotificationNick;
        Backend.Chat.OnNotification = (NotificationEventArgs args) =>
        {

            NotificationNick = string.Format("[Notification] {0}", args.Subject);
            chatItem = new ChatItem(SessionInfo.None, NotificationNick, args.Message, false, false, true);
            chatScroll.PopulateAll(chatItem);

        };

        string GlobalChatNick;
        Backend.Chat.OnGlobalChat = (GlobalChatEventArgs args) =>
        {
            if (args.ErrInfo == ErrorInfo.Success)
            {
                GlobalChatNick = string.Format("[User Notification] {0}", args.From.NickName);
            }
            else
            {
                GlobalChatNick = string.Format("[Error] {0}", "운영자가 아닙니다");
            }

            chatItem = new ChatItem(SessionInfo.None, GlobalChatNick, args.Message, false, false, true);
            chatScroll.PopulatePublicChat(chatItem);

        };

        string whisperNick;
        Backend.Chat.OnWhisper = (WhisperEventArgs args) =>
        {
            Debug.Log(string.Format("OnWhisper {0}", args.ErrInfo));

            if (args.ErrInfo == ErrorInfo.Success)
            {
                Debug.Log(string.Format("OnWhisper: from {0} to {1} : message {2}", args.From.NickName, args.To.NickName, args.Message));

                // 내가 보낸 귓속말인 경우
                if (!args.From.IsRemote)
                {
                    whisperNick = string.Format("to {0}", args.To.NickName);
                    chatItem = new ChatItem(SessionInfo.None, whisperNick, args.Message, true, true);
                }
                // 내가 받은 귓속말인 경우
                else
                {
                    whisperNick = string.Format("from {0}", args.From.NickName);
                    chatItem = new ChatItem(args.From, whisperNick, args.Message, true, false);
                }

                chatScroll.PopulateAll(chatItem);
            }
            else if (args.ErrInfo.Category == ErrorCode.BannedChat)
            {
                // 도배방지 메세지 
                if (args.ErrInfo.Detail == ErrorCode.BannedChat)
                {
                    chatItem = new ChatItem(SessionInfo.None, chatScroll.infoText, args.ErrInfo.Reason, false);
                    chatScroll.PopulateAll(chatItem);
                }
            }
        };

        Backend.Chat.OnRecentChatLogs = (RecentChatLogsEventArgs args) =>
        {
            Debug.Log("OnRecentChatLogs : " + args.LogInfos.Count);
            if (args.ErrInfo != ErrorInfo.Success)
            {
                Debug.LogError(args.ErrInfo);
                Debug.LogError(args.Reason);
            }
            string str = string.Empty;
            str += "OnRecentChatLogs\n";
            str += string.Format("ChannelType : {0}\n", args.channelType);
            str += string.Format("message Count : {0}\n\n", args.LogInfos.Count);

            //  역순으로 출력
            for (int i = args.LogInfos.Count - 1; i >= 0; i--)
            {
                str += string.Format("NickName : [ {0} ] : \"{1}\"\n", args.LogInfos[i].NickName, args.LogInfos[i].Message);

                chatItem = new ChatItem(args.LogInfos[i].NickName, args.LogInfos[i].Message);
                chatScroll.PopulateRecentChat(args.channelType, chatItem);

            }
            Debug.Log(str);
        };

        // Exception 발생 시
        Backend.Chat.OnException = (Exception e) =>
        {
            ShowModal(e.GetBaseException().ToString());
            Debug.LogError(e.StackTrace);
        };


    }

    private void ShowModal(string message)
    {
        ChannelListManager.Instance().showmodal(message);
    }
    void OnApplicationQuit()
    {
        if (Backend.Chat.IsChatConnect(ChannelType.Public)) // 일반 채널에 올라가지 않으면
        {
            Backend.Chat.LeaveChannel(ChannelType.Public);
        }
        if (Backend.Chat.IsChatConnect(ChannelType.Guild))
        {
            Backend.Chat.LeaveChannel(ChannelType.Guild);
        }
    }
}

internal class ChatItem
{
    internal SessionInfo session { get; set; }
    internal bool IsRemote { get; set; }
    internal bool isWhisper { get; set; }
    internal bool isNotification { get; set; } // 운영자채팅도 포함
    internal string Nickname;
    internal string Contents;

    internal ChatItem(SessionInfo session, string nick, string cont, bool isWhisper, bool IsRemote)
    {
        this.session = session;
        Nickname = nick;
        Contents = cont;
        this.isWhisper = isWhisper;
        this.IsRemote = IsRemote;
    }
    internal ChatItem(SessionInfo session, string nick, string cont, bool isWhisper, bool IsRemote, bool isNotification)
    {
        this.session = session;
        Nickname = nick;
        Contents = cont;
        this.isWhisper = isWhisper;
        this.IsRemote = IsRemote;
        this.isNotification = isNotification;
    }

    internal ChatItem(SessionInfo session, string nick, string cont, bool IsRemote)
    {
        this.session = session;
        Nickname = nick;
        Contents = cont;
        isWhisper = false;
        this.IsRemote = IsRemote;
    }
    internal ChatItem(string nick, string cont)
    {
        this.session = session;
        Nickname = nick;
        Contents = cont;
        isWhisper = false;
        this.IsRemote = false;
    }
}