using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDartPanel : MonoBehaviour
{
    private List<DiceGameData> sendServerData;  //서버에 전송할 데이터

    //다트판 속성
    private float xPositionLimit = 0.5f;  //옆으로 이동하기까지 제한
    private float pannelSpeed = 0.3f;   //다트판 이동 속도
    private bool swapDirection = false;
    public bool isMove = true;  //움직이고 있는지
    public Vector3 moveDirection = Vector3.zero;

    private void Awake()
    {
        sendServerData = new List<DiceGameData>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //다음 차례
        MinigameManager.Instance.GetMiniGame<GameDart>().NextDart();
    }

    public void MoveEvent()
    {
        if(isMove)
        {
            //다트판 이동하기 (세로 한쪽만 던지면 시시하니까)
            //좌우로 왔다갔다 하게
            if (transform.localPosition.x < -xPositionLimit)
                moveDirection = Vector3.right;
            else if (transform.localPosition.x > xPositionLimit)
                moveDirection = Vector3.left;

            ApplyMove();
        }
    }

    private void ApplyMove()
    {
        transform.Translate(moveDirection * (Time.deltaTime * pannelSpeed));
    }

    /// <summary>
    /// Limit 검사
    /// </summary>
    /// <returns>양수면 오른쪽 음수면 왼쪽</returns>
    public int GetReverse()
    {
        if (transform.localPosition.x < -xPositionLimit)
            return 1;
        else if (transform.localPosition.x > xPositionLimit)
            return -1;
        else
            return 0;
    }

    private IEnumerator SendServer()
    {
        while (isMove)
        {
            GamePacket packet = new();
            {
                packet.DartPannelSyncRequest = new()
                {
                    SessionId = MinigameManager.Instance.mySessonId,
                    Location = SocketManager.ToVector(transform.localPosition)
                };
                Debug.Log(packet.DartPannelSyncNotification.Location);
            }
            SocketManager.Instance.OnSend(packet);
            yield return new WaitForSeconds(0.1f);
        }
    }
}