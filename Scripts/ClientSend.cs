using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.Inst.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.Inst.udp.SendData(_packet);
    }
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.Inst.myId);
            _packet.Write(ActionManager.Inst.usernameField.text);
            SendTCPData(_packet);
        }
    }

    public static void MovePlayer(Vector2 _newPosition, int playerId)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMove))
        {
            _packet.Write(playerId);
            _packet.Write(_newPosition);
            SendUDPData(_packet);
        }
    }

    public static void PlatformPull(Vector2 Platform, Vector2 toPlace)
    {
        using (Packet _packet = new Packet((int)ClientPackets.platformOrPersonPull))
        {
            _packet.Write(Platform);
            _packet.Write(toPlace);
            SendUDPData(_packet);
        }
    }

    public static void WallDestroy(Vector2 _platform)
    {
        using (Packet _packet = new Packet((int)ClientPackets.wallDestroy))
        {
            _packet.Write(_platform);
            SendUDPData(_packet);
        }
    }

    public static void GiveArtifactTo(Vector2 _playerPosition)
    {
        using (Packet _packet = new Packet((int)ClientPackets.giveArtifact))
        {
            _packet.Write(_playerPosition);
            SendUDPData(_packet);
        }
    }
}
