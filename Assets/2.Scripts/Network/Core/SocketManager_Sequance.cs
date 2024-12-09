using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SocketManager : TCPSocketManagerBase<SocketManager>
{
    //플레이어 순서 정하기

    public void DiceGameResponse(GamePacket packet)
    {
        var response = packet.DiceGameResponse;

        if(response.Success)
        {
            var result = response.Result;

            int i = 0;
            var playerDart = SelectOrderManager.Instance.DartOrder;
            foreach(var dart in playerDart)
            {
                dart.MyDistance = result[i].Distance;
                dart.MyRank = result[i].Rank;
                i++;
            }
        }
        else
        {
            Debug.LogError($"FailCode : {response.FailCode.ToString()}");
        }
    }

    public void DiceGameNotification(GamePacket packet)
    {
        var notification = packet.DiceGameNotification;
        var result = notification.Result;

        List<SelectOrderDart> darts = SelectOrderManager.Instance.DartOrder;
        foreach(var dart in darts)
        {
            result.Add(dart.DiceGameData);
        }
    }

    public void DiceGameRequest(GamePacket packet)
    {
        var request = packet.DiceGameRequest;
    }
}
