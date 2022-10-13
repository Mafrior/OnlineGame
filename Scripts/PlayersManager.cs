using UnityEngine;
using System.Collections.Generic;

//Управление игроками
public class PlayersManager : MonoBehaviour
{
    public static PlayersManager Inst { get; set; }

    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();
    public GameObject otherPlayers;

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            return;
        }
        if (Inst == this)
        {
            Debug.Log("Instznce already exists");
            Destroy(this);
        }
    }

    // Изменяем позицию игрока
    public void ChangePosition(int _clientId, Vector2 _newPos)
    {
        if (FIeld.Inst[_newPos.y, _newPos.x].isWater) { Debug.Log("Девочки вы упали"); return; }
        players[_clientId].transform.position = FIeld.Inst[_newPos.y, _newPos.x].gameObject.transform.position + new Vector3(0, 0.2f);
        players[_clientId].GetComponent<Person>().position = _newPos;
    }

    // Полученные позиции игроков мы используем для спавна их на поле
    public void GetPositions(int _playerCount, Packet _packet)
    {
        for (int i = 1; i <= _playerCount; i++)
        {
            Vector2 position = _packet.ReadVector();
            //Если номер позиции равен нашему Id, то активируем нашего клиента, передаём ему позицию и включаем у него скрипт игрока
            if (i == Client.Inst.myId)
            {
                players.Add(i, Client.Inst.gameObject);
                players[i].GetComponent<SpriteRenderer>().enabled = true;
                players[i].transform.position = FIeld.Inst[position.y, position.x].gameObject.transform.position + new Vector3(0, 0.2f);
                Client.Inst.player.position = position;
                continue;
            }
            // Если это какие-то другие игроки - просто инициализируем спрайты
            players.Add(i, Instantiate(otherPlayers));
            // Передаём игрокам позицию, соответствующую позиции платформы с такими же векторами
            players[i].transform.position = FIeld.Inst[position.y, position.x].gameObject.transform.position + new Vector3(0, 0.2f);
            players[i].transform.SetParent(this.transform);
        }
    }

}
