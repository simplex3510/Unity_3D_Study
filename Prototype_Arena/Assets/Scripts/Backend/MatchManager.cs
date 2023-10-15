using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using BackEnd.Tcp;
using Manager;

public class MatchManager : MonoSingleton<MatchManager>
{
    ErrorInfo errorInfo;

    private void Start()
    {
        Backend.Match.JoinMatchMakingServer(out errorInfo);
        Debug.Log("��ġ ���� ���� ����: " + errorInfo.Detail.ToString());

        Backend.Match.OnJoinMatchMakingServer += (JoinChannelEventArgs args) =>
        {
            Debug.Log("��ġ ���� ����: " + args.ErrInfo.ToString());
        };

        Backend.Match.CreateMatchRoom();
        Backend.Match.OnMatchMakingRoomCreate = (MatchMakingInteractionEventArgs args) =>
        {
            Debug.Log("��ġ �� ����: " + args.ErrInfo.ToString());
        };
    }

    private void Update()
    {
        Backend.Match.Poll();
    }

    public void OnClickMatch()
    {
        Backend.Match.RequestMatchMaking(MatchType.Random, MatchModeType.OneOnOne, Backend.Match.GetMatchList().GetInDate());
    }
}
