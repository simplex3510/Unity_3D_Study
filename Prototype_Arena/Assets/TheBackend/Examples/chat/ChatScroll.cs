using System.Collections.Generic;
using BackEnd.Tcp;
using UnityEngine;
using UnityEngine.UI;

public class ChatScroll : MonoBehaviour
{
    public GameObject prefab; // This is our prefab object that will be exposed in the inspector

    public RectTransform publicContent;
    public RectTransform guildContent;
    public Color whisperColor;
    private Color32 infoTextColor = new Color32(158, 72, 28, 255);

    internal List<ChatItem> publicChats = new List<ChatItem>();

    private static ChatScroll chatScroll;

    internal string infoText = "안내";
    private static string CHAT_NICK = "[{0}]";


    public static ChatScroll Instance()
    {
        if (!chatScroll)
        {
            chatScroll = FindObjectOfType(typeof(ChatScroll)) as ChatScroll;
            if (!chatScroll)
                Debug.LogWarning("There needs to be one active ChatScroll script on a GameObject in your scene.");
        }

        return chatScroll;
    }

    internal void PopulatePublicList()
    {
        // 존재하던 리스트를 삭제 (reset)
        RemoveAllPublicListViewItem();
        foreach (ChatItem item in publicChats)
        {
            PopulatePublicChat(item);
        }
    }


    // 채팅 아이템 출력
    GameObject newObj;

    internal void PopulatePublicChat(ChatItem item)
    {
        PopulateChat(ChannelType.Public, item);
    }

    internal void PopulateGuildChat(ChatItem item)
    {
        PopulateChat(ChannelType.Guild, item);
    }

    internal void PopulateAll(ChatItem item)
    {
        PopulatePublicChat(item);
        PopulateGuildChat(item);
    }

    internal void PopulateRecentChat(ChannelType type, ChatItem item)
    {

        // Create new instances of our prefab until we've created as many as we specified
        //Debug.Log(publicContent.transform.position.y + newObj.GetComponent<RectTransform>().rect.height);
        if (type == ChannelType.Public)
        {
            newObj = (GameObject)Instantiate(prefab, publicContent.transform);
        }
        else
        {
            newObj = (GameObject)Instantiate(prefab, guildContent.transform);
        }

        Text[] texts = newObj.GetComponentsInChildren<Text>();
        // Nickname
        texts[0].text = string.Format(CHAT_NICK, item.Nickname);
        // Contents
        texts[1].text = item.Contents;
    }
    private void PopulateChat(ChannelType type, ChatItem item)
    {
        switch (type)
        {
            case ChannelType.Public:
                if (publicContent != null && publicContent.transform != null)
                {
                    // Create new instances of our prefab until we've created as many as we specified
                    //Debug.Log(publicContent.transform.position.y + newObj.GetComponent<RectTransform>().rect.height);
                    newObj = (GameObject)Instantiate(prefab, publicContent.transform);
                }
                break;
            case ChannelType.Guild:
                if (guildContent != null && guildContent.transform != null)
                {
                    // Create new instances of our prefab until we've created as many as we specified
                    newObj = (GameObject)Instantiate(prefab, guildContent.transform);
                }
                break;
            default:
                return;
        }

        Text[] texts = newObj.GetComponentsInChildren<Text>();
        // Nickname
        texts[0].text = string.Format(CHAT_NICK, item.Nickname);
        // Contents
        texts[1].text = item.Contents;

        if ((item.session != SessionInfo.None && item.session.IsRemote))
        {
            Button button = newObj.GetComponentInChildren<Button>();
            button.onClick.AddListener(delegate { ParticipantsModal.Instance().participantPanelShow(item.session.NickName, Input.mousePosition); });
        }

        // 안내 메세지 색상
        if (item.Nickname.Equals(infoText))
        {
            foreach (Text text in texts)
            {
                text.color = infoTextColor;
            }
        }
        // 귓속말 색상
        else if (item.isWhisper)
        {
            foreach (Text text in texts)
            {
                text.color = whisperColor;
            }
        }
    }

    internal void RemoveAllPublicListViewItem()
    {
        RemoveAllListViewItem(ChannelType.Public);
    }

    internal void RemoveAllGuildListViewItem()
    {
        RemoveAllListViewItem(ChannelType.Guild);
    }

    private void RemoveAllListViewItem(ChannelType type)
    {
        RectTransform content = null;
        switch (type)
        {
            case ChannelType.Public:
                content = publicContent;
                break;
            case ChannelType.Guild:
                content = guildContent;
                break;
        }

        if (content != null)
        {
            foreach (Transform child in content.transform)
            {
                if (child != null)
                    Destroy(child.gameObject);
            }
        }
    }
}

