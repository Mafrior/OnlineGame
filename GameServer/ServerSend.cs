using System;
using System.Numerics;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GameServer
{
    class ServerSend
    {
        private static void SendTCPData(int _toCLient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toCLient].tcp.SendData(_packet);
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
            Console.WriteLine("Send end Succesfull");
        }

        private static void SendTCPDataToAll(int _exceptCLient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptCLient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        public static void Welcome(int _toCLient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toCLient);
                SendTCPData(_toCLient, _packet);
            }
        }

        // Передаём игрокам, передвижение одного из игроков
        public static void PlayerMove(int _fromClient, Vector2 _newPosition)
        {
            using (Packet _packet = new Packet((int)ServerPackets.someOneMove))
            {
                _packet.Write(_fromClient);
                _packet.Write(_newPosition);
                SendUDPDataToAll(_packet);
            }
        }

        // Инициализируем поле, вместе с игроками
        public static void SendGameProperties()
        {
            using (Packet _packet = new Packet((int)ServerPackets.fieldInitialize)) {
                _packet.Write(Server.field.Count);
                for (int i = 0; i < Server.field.Count; i++)
                {
                    for(int j = 0; j < Server.field.Count; j++)
                    {
                        _packet.Write(Server.field[i, j].isWater); // Результат генерации
                        _packet.Write(Server.field[i, j].rotation); // Результат генерации
                    }
                }
                _packet.Write(Server.MaxPlayers);
                for (int i = 1; i <= Server.MaxPlayers; i++)
                {
                    _packet.Write(Server.clients[i].player.position);
                }
                for(int i = 0; i < 3; i++)
                {
                    _packet.Write(Server.artifacts[i].postition);
                }
                for(int i = 0; i < Server.MaxPlayers; i++)
                {
                    _packet.Write(Server.queue[i]);
                }
                SendTCPDataToAll(_packet);
            }
        }

        public static void PlatformPull(int _fromClient, Vector2 Platform, Vector2 toPlace)
        {
            using (Packet _packet = new Packet((int)ServerPackets.someOnePullPersonOrPlatform))
            {
                _packet.Write(_fromClient);
                _packet.Write(Platform);
                _packet.Write(toPlace);
                SendUDPDataToAll(_packet);
            }
        }

        public static void DestoyWall(Vector2 _platPos)
        {
            using (Packet _packet = new Packet((int)ServerPackets.someOneDestroyWall))
            {
                _packet.Write(_platPos);
                SendUDPDataToAll(_packet);
            }
        }

        public static void MoveArtifact(Vector2 _artifactPosition, Vector2 _newPosition)
        {
            using (Packet _packet = new Packet((int)ServerPackets.someOneDestroyWall))
            {
                _packet.Write(_artifactPosition);
                _packet.Write(_newPosition);
                SendUDPDataToAll(_packet);
            }
        }
    }
}
