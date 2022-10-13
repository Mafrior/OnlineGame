using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Сообщение с сервера {_msg}");

        Client.Inst.myId = _myId;
        ClientSend.WelcomeReceived();
        Client.Inst.udp.Connect(((IPEndPoint)Client.Inst.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void PlayerMoveTo(Packet _packet)
    {
        PlayersManager.Inst.ChangePosition(_packet.ReadInt(), _packet.ReadVector());
    }

    public static void PlatformPulled(Packet _packet)
    {
        int _fromClient = _packet.ReadInt();
        Vector2 platform = _packet.ReadVector();
        Vector2 toPlace = _packet.ReadVector();
        FIeld.SwapPlatform(FIeld.Inst[platform.y, platform.x], FIeld.Inst[toPlace.y, toPlace.x]);
    }

    public static void WallDestroied(Packet _packet)
    {
        FIeld.DestroyWall(_packet.ReadVector());
    }

    public static void ArtifactMoved(Packet _packet)
    {   
        Artifacts.Inst.MoveArtifact(_packet.ReadVector(), _packet.ReadVector());
    }

    // Получаем поле и игроков с сервера, инициализируем поле и игроков
    public static void GetField(Packet _packet)
    {
        int _fieldSize = _packet.ReadInt();
        FIeld.Inst.InitializeField(_fieldSize, _packet); // Инициализация поля

        int _playersCount = _packet.ReadInt();
        PlayersManager.Inst.GetPositions(_playersCount, _packet); // Инициализация игроков

        Artifacts.Inst.GetArtefacts(_packet); // Инициализируем артефакты

        ActionManager.Inst.GetQueue(_playersCount, _packet); // Инициализируем очередь
    }
}
