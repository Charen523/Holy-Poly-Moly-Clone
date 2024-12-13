using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMinigameDart : UIBase
{
    #region UI 속성
    [Header("Force Power")]
    [SerializeField] private Slider forcePower;

    [Header("Result")]
    [SerializeField] private Image[] resultImage;
    [SerializeField] private TextMeshProUGUI[] stateTexts;
    [SerializeField] private TextMeshProUGUI[] scoreTexts;
    #endregion

    private Color[] playerColor = {Color.red, Color.yellow, Color.green, Color.blue};

    private string nickname = "";

    private void Start()
    {
        //forcePower 초기
        SetForceLimit(1.5f, 3f);
        
        for (int i = 0; i < stateTexts.Length; i++)
        {
            stateTexts[i].color = playerColor[i];
            resultImage[i].color = Color.gray;
            SetReady(i);
        }
    }

    public void SetForceLimit(float min, float max)
    {
        forcePower.minValue = min;
        forcePower.maxValue = max;
    }

    public void ChangeForcePower(float f)
    {
        forcePower.value = f;
    }

    #region Result 메서드
    public void SetNickname(string name)
    {
        nickname = name;
    }
    public void SetReady(int idx)
    {
        ApplyText(idx, "준비");
    }
    public void SetMyTurn(int idx)
    {
        stateTexts[idx].color = Color.white;
        resultImage[idx].color = playerColor[idx];
        ApplyText(idx, "내 차례");
    }
    public void SetFinish(int idx)
    {
        ApplyText(idx, "OK!");
    }
    public void SetScore(int idx, float score)
    {
        if (score >= 10f)
            ApplyText(idx, "Miss");
        else
            ApplyText(idx, score.ToString("N4"));
    }

    private void ApplyText(int idx, string txt)
    {
        stateTexts[idx].text = $"{nickname} : {txt}";
    }
    #endregion

    //Todo : 내 차례가 되면 힘조절 UI 활성

    public override void Opened(object[] param)
    {
        HashSet<int> usedColors = new HashSet<int>();
        foreach(var dic in GameManager.Instance.SessionDic)
        {
            int color = dic.Value.Color;
            
        }
        for (int i = 0; i < resultImage.Length; i++)
        {
            if(!usedColors.Contains(i))
            {
                resultImage[i].gameObject.SetActive(false);
            }
        }
    }
    public override void Closed(object[] param)
    {

    }
}
