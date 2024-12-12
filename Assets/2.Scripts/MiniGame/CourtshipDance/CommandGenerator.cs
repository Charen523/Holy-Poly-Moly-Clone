using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class CommandGenerator
{
    private CourtshipDanceData gameData;

    private int[] rotations = { 0, 90, 180, 270 };
    private Random random = new();

    private Dictionary<string, UserInfo> SessionDic = GameManager.Instance.SessionDic;

    private Dictionary<string, Queue<Queue<BubbleInfo>>> playerPoolDic = new();
    private Dictionary<int, Queue<Queue<BubbleInfo>>> teamPoolDic;

    public CommandGenerator()
    {
        gameData = new CourtshipDanceData();
        gameData.Init();
    }

    public void InitFFA(List<PlayerInfo> players)
    {
        var originPool = GenerateBoardInfoPool(gameData.individualBoardAmount);

        for (int i = 0; i < players.Count; i++)
        {
            Queue<Queue<BubbleInfo>> playerPool = DeepCopyPool(originPool);   // 깊은 복사
            SetBoardPoolColor(playerPool, players[i]);
            playerPoolDic.Add(players[i].SessionId, playerPool);
        }
    }

    public void InitTeamGame(Dictionary<int, List<PlayerInfo>> teamDic)
    {
        teamPoolDic = new();
        var originPool = GenerateBoardInfoPool(gameData.boardAmount);

        int boardCount = teamDic.Keys.Max();
        for (int i = 0; i < boardCount; ++i)
        {
            Queue<Queue<BubbleInfo>> dancePool = DeepCopyPool(originPool);
            SetBoardPoolColor(dancePool, teamDic[i+1]);
            teamPoolDic.Add(i + 1, dancePool);
        }
    }

    private Queue<Queue<BubbleInfo>> DeepCopyPool(Queue<Queue<BubbleInfo>> original)
    {
        return new Queue<Queue<BubbleInfo>>(
              original.Select(innerQueue => new Queue<BubbleInfo>(
                      innerQueue.Select(bubble => bubble.Clone())
                   ))
              );
    }

    public Dictionary<string, Queue<Queue<BubbleInfo>>> GetPlayerPoolDic()
    {
        return playerPoolDic;
    }
    public Dictionary<int, Queue<Queue<BubbleInfo>>> GetTeamPoolDic() 
    {
        return teamPoolDic;
    }

    public Queue<Queue<BubbleInfo>> GenerateBoardInfoPool(int poolCount)
    {
        Queue<Queue<BubbleInfo>> pool = new();
        for (int i = 0; i < poolCount; ++i)
        {
            int bubbleCount = Math.Clamp(i, gameData.minBubbleCount, gameData.maxBubbleCount);
            pool.Enqueue(GenerateBoardInfo(bubbleCount));
        }
        return pool;
    }

    private Queue<BubbleInfo> GenerateBoardInfo(int bubbleCount)
    {
        Queue<BubbleInfo> queue = new();
        for (int i = 0; i < bubbleCount; ++i)
        {
            queue.Enqueue(new BubbleInfo(GetRandomRotation()));
        }
        return queue;
    }

    public void SetBoardPoolColor(Queue<Queue<BubbleInfo>> pool, List<PlayerInfo> players)
    {
        foreach (var queue in pool)
        {
            SetBoardColor(queue, players);
        }
    }

    public void SetBoardPoolColor(Queue<Queue<BubbleInfo>> pool, PlayerInfo player)
    {
        foreach (var queue in pool)
        {
            SetBoardColor(queue, player);
        }
    }

    private void SetBoardColor(Queue<BubbleInfo> queue, List<PlayerInfo> teamPlayers)
    {
        List<int> colors = new();
        List<string> sessionIds = new();
        foreach (var player in teamPlayers)
        {
            if (SessionDic.TryGetValue(player.SessionId, out UserInfo userInfo))
            {
                colors.Add(userInfo.Color);
                sessionIds.Add(player.SessionId);
            }
        }
        if (colors.Count == 0) return;

        int index = 0;
        foreach (BubbleInfo b in queue)
        {
            b.SetColor(colors[index]);
            b.SetSessionId(sessionIds[index]);
            index = (index + 1) % colors.Count;
        }
    }

    private void SetBoardColor(Queue<BubbleInfo> queue, PlayerInfo player)
    {
        int color = -1;

        if (SessionDic.TryGetValue(player.SessionId, out UserInfo userInfo))
        {
            color = userInfo.Color;
        }
        else
        {
            // 게임매니저 세션딕에 찾는 세션Id가 없음.
        }

        foreach (BubbleInfo b in queue)
        {
            b.SetColor(color);
            b.SetSessionId(player.SessionId);
        }
    }

    private float GetRandomRotation()
    {
        return rotations[random.Next(rotations.Length)];
    }

    #region Convert
    public static RepeatedField<DancePool> ConvertToDancePools(Dictionary<string, Queue<Queue<BubbleInfo>>> playerPoolDic)
    {
        RepeatedField<DancePool> dancePools = new RepeatedField<DancePool>();

        foreach (var pool in playerPoolDic)
        {
            DancePool obj = new()
            {
                SessionId = pool.Key,
                DanceTables = { }
            };
            foreach (var queue in pool.Value)
            {
                DanceTable danceTable = new()
                {
                    Commands = { }
                };
                foreach (var bubbleInfo in queue)
                {
                    DanceCommand command = new DanceCommand
                    {
                        Direction = (Direction)bubbleInfo.Rotation,
                        TargetSessionId = bubbleInfo.sessionId
                    };
                    danceTable.Commands.Add(command);
                }
                obj.DanceTables.Add(danceTable);
            }

            dancePools.Add(obj);
        }

        return dancePools;
    }

    public static RepeatedField<DancePool> ConvertToDancePools(Dictionary<int, Queue<Queue<BubbleInfo>>> teamPoolDic, Dictionary<int, List<PlayerInfo>> teamDic)
    {
        RepeatedField<DancePool> dancePools = new RepeatedField<DancePool>();
        
        foreach (var pool in teamPoolDic)
        {
            DancePool obj = new()
            {
                SessionId = teamDic[pool.Key][0].SessionId, // 팀 첫번째 플레이어의 세션ID
                //TeamNumber = teamDic[1][0].TeamNumber,
                DanceTables = { }
            };
            foreach (var queue in pool.Value)
            {
                DanceTable danceTable = new()
                {
                    Commands = { }
                };
                foreach (var bubbleInfo in queue)
                {
                    DanceCommand command = new DanceCommand
                    {
                        Direction = (Direction)bubbleInfo.Rotation,
                        TargetSessionId = bubbleInfo.sessionId
                    };
                    danceTable.Commands.Add(command);
                }
                obj.DanceTables.Add(danceTable);
            }

            dancePools.Add(obj);
        }
        return dancePools;
    }

    public static Dictionary<string, Queue<Queue<BubbleInfo>>> ConvertToPlayerPoolDic(RepeatedField<DancePool> dancePools)
    {
        Dictionary<string, Queue<Queue<BubbleInfo>>> playerPoolDic = new Dictionary<string, Queue<Queue<BubbleInfo>>>();

        foreach (var dancePool in dancePools)
        {
            Queue<Queue<BubbleInfo>> playerQueue = new Queue<Queue<BubbleInfo>>();

            foreach (var danceTable in dancePool.DanceTables)
            {
                Queue<BubbleInfo> bubbleQueue = new Queue<BubbleInfo>();

                foreach (var command in danceTable.Commands)
                {
                    BubbleInfo bubbleInfo = new BubbleInfo();
                    bubbleInfo.SetSessionId(command.TargetSessionId);
                    bubbleInfo.SetRotation((float)command.Direction);
                    bubbleInfo.SetColor(command.TargetSessionId);

                    bubbleQueue.Enqueue(bubbleInfo);
                }

                playerQueue.Enqueue(bubbleQueue);
            }

            playerPoolDic[dancePool.SessionId] = playerQueue;
        }

        return playerPoolDic;
    }
    #endregion
}
