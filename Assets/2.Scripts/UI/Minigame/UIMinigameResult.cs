using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIMinigameResult : UIBase
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image[] RankPanels;
    [SerializeField] private Sprite[] RankPanelsSprites;
    [SerializeField] private TextMeshProUGUI[] RankTxts;
    [SerializeField] private TextMeshProUGUI returnTxt;

    private Dictionary<int, int> colorIdxs = new();
    private Dictionary<int, int> coinDics = new()
    { { 1, 20 }, { 2, 10 }, { 3, 5 }, { 4, 1 } };

    public override void Opened(object[] param)
    {
        GameManager.OnPlayerLeft += PlayerLeftEvent;

        UIManager.Hide<UIMinigameIce>();

        foreach (var panel in RankPanels)
        {
            panel.gameObject.SetActive(false);
        }

        //ranks의 string : sessionId, int : 등수
        if (param.Length == 2)
        {
            if (param[0] is Dictionary<string, int> ranks)
            {
                foreach (var rank in ranks)
                {
                    string sessionid = rank.Key; //id
                    int rankNum = rank.Value; //등수
                    int color = GameManager.Instance.SessionDic[sessionid].Color; //색깔
                    colorIdxs.Add(color, rankNum);

                    RankPanels[color].gameObject.SetActive(true);

                    //등수에 맞는 위치에 색깔 지정
                    RankPanels[rankNum - 1].sprite = RankPanelsSprites[color];

                    //등수 + 닉네임 설정
                    RankTxts[rankNum - 1].text = $"{rankNum}등\n{GameManager.Instance.SessionDic[sessionid].Nickname}";

                    //보상 지급
                    BoardManager.Instance.playerTokenHandlers[color].data.keyAmount += coinDics[rankNum];

                    //미니게임 순서 재정의
                    GameManager.Instance.SessionDic[sessionid].SetOrder(rankNum - 1);
                }
            }
            else
            {
                Debug.LogError("param 오류 : idx0이 ranks가 아님");
            }

            if (param[1] is long returnTime)
            {
                StartCoroutine(ReturnTxt());
                StartCoroutine(ReturnBoard(returnTime));
            }
        }
        else
        {
            Debug.LogError("param 오류 : object[] length가 다름");
        }

        Sequence sequence = DOTween.Sequence();
        sequence.Append(titleText.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack));
        sequence.Append(titleText.transform.DOScale(1f, 0.3f).SetEase(Ease.InOutBounce));
        sequence.Join(titleText.transform.DOShakePosition(0.3f, new Vector3(10f, 0, 0), 20, 90, false, true));
    }

    public override async void Closed(object[] param)
    {
        base.Closed(param);
        colorIdxs.Clear();
        GameManager.OnPlayerLeft -= PlayerLeftEvent;
        await UIManager.Show<BoardUI>();
    }

    private IEnumerator ReturnTxt()
    {
        int leftSeconds = 5;
        
        while (leftSeconds > 0)
        {
            returnTxt.text = $"{leftSeconds}초 후 보드로 돌아갑니다...";
            leftSeconds--;
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator ReturnBoard(long returnTime)
    {
        yield return new WaitUntil(() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= returnTime);
        UIManager.Hide<UIMinigameResult>();
        BoardManager.Instance.NextTurn();
    }

    private void PlayerLeftEvent(int color)
    {
        RankPanels[colorIdxs[color]].color = new Color(145 / 255f, 145 / 255f, 145 / 255f, 220 / 255f);
        RankTxts[colorIdxs[color]].text = "오프라인";
        RankTxts[colorIdxs[color]].color = new Color(150 / 255f, 150 / 255f, 150 / 255f);
    }
}