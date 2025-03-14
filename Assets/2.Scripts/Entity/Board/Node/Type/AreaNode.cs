using UnityEngine;
using UnityEngine.UIElements;

public class AreaNode : BaseNode, IPurchase
{
    private int playerIndex = -1;
    [SerializeField] MeshRenderer plane;
    int price;

    string IPurchase.message => $"{price}의 열쇠를 지불하여 해당 칸을 구매 할 수 있습니다.";


    public async override void Action()
    {
        int c = BoardManager.Instance.Curplayer.queue.Count;
        int d = BoardManager.Instance.Curplayer.dice;

        if (c > 1 || d > 0)
        {
            base.Action();
            return;
        }

        //var m = BoardManager.Instance;
        //int index = m.playerTokenHandlers.IndexOf(p);

        var player = BoardManager.Instance.Curplayer;
        int index = BoardManager.Instance.curPlayerIndex;
        IPurchase purchase = this;

        //if (playerIndex == -1)
        //{
        //    var ui = await UIManager.Show<PurchaseNodeUI>(purchase, index);
        //    return;
        //}

        if (playerIndex != index)
        {
            if (playerIndex != -1) Damage(player.data);
            var ui = await UIManager.Show<PurchaseNodeUI>(purchase, index);
        }
    }
    private void Damage(BoardTokenData p)
    {
        //임시 주석
        //p.hp -= 0;

        //GamePacket packet = new();

        ////packet. = new()
        ////{

        ////};

        //SocketManager.Instance.OnSend(packet);
    }

    public void Purchase(int index)
    {
        //playerIndex = index;
        //plane.material = BoardManager.Instance.materials[index];
        //int i = BoardManager.Instance.areaNodes.IndexOf(this);

        GamePacket packet = new();
        //packet.PurchaseTileRequest = new()
        //{
        //    SessionId = "1",
        //    Tile = SocketManager.ConvertVector(transform.position)
        //};

        SocketManager.Instance.OnSend(packet);

        Cancle();
    }

    public void Cancle()
    {
        BoardTokenHandler p = BoardManager.Instance.Curplayer;
        p.SetNode(nodes[0],true);
        p.GetDice(0);

        BoardManager.Instance.TurnEnd();
    }

    public void SetArea(int index)
    {
        //playerIndex = index;
        plane.material = BoardManager.Instance.materials[index];
    }
}
