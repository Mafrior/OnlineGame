using System;
using System.Numerics;
using System.Linq;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromCLient, Packet _packet)
        {
            int _clientId = _packet.ReadInt();
            string _username = _packet.ReadString();
            Server.players[_clientId].userName = _username;
            Console.WriteLine($"{Server.clients[_fromCLient].tcp.socket.Client.RemoteEndPoint} connected successfully with nickname {_username}, {_fromCLient}");
            if(_fromCLient != _clientId)
            {
                Console.WriteLine($"We changed Id to {_clientId}");
            }
            if (_clientId == Server.MaxPlayers)
            {
                Server.GameInitilize();
            }
        }

        public static void PlayerMove(int _fromClient, Packet _packet)
        {
            if (Server.queue[Server.whichTurn] != _fromClient) { return; }
            Server.whichTurn = Server.whichTurn == Server.MaxPlayers - 1 ? 0 : Server.whichTurn + 1;
            int _replacedPlayer = _packet.ReadInt();
            Vector2 _newposition = _packet.ReadVector();
            if(Server.artifacts.Any(x => x.postition == _newposition))
            {
                Server.players[_replacedPlayer].pickedUpArtifact = true;
            }
            Server.players[_replacedPlayer].position = _newposition;
            Console.WriteLine($"Player {_replacedPlayer} replaced to position {_newposition}");
            ServerSend.PlayerMove(_replacedPlayer, _newposition);
        }

        public static void PlatformPull(int _fromClient, Packet _packet)
        {
            if (Server.queue[Server.whichTurn] != _fromClient) { return; }
            Server.whichTurn = Server.whichTurn == Server.MaxPlayers - 1 ? 0 : Server.whichTurn + 1;
            Vector2 _platform = _packet.ReadVector();
            Vector2 _toPlace = _packet.ReadVector();
            ServerSend.PlatformPull(_fromClient, _platform, _toPlace);
            Console.WriteLine($"{_fromClient} pull {_platform} to place {_toPlace}");
        }

        public static void WallDestroied(int _fromClient, Packet _packet)
        {
            if (Server.queue[Server.whichTurn] != _fromClient) { return; }
            Server.whichTurn = Server.whichTurn == Server.MaxPlayers-1 ? 0 : Server.whichTurn+1;
            Vector2 platformPos = _packet.ReadVector();
            Console.WriteLine($"Wall on Platform {platformPos} was destroed");
            ServerSend.DestoyWall(platformPos);
        }

        public static void MoveArtifact(int _fromClient, Packet _packet)
        {
            if (Server.queue[Server.whichTurn] != _fromClient) { return; }
            Server.whichTurn = Server.whichTurn == Server.MaxPlayers - 1 ? 0 : Server.whichTurn + 1;
            Vector2 _newPosition = _packet.ReadVector();
            if (Server.players.First(x => x.Value.position == _newPosition).Value.pickedUpArtifact) { return; }
            ServerSend.MoveArtifact(Server.players[_fromClient].position, _newPosition);
        }
    }
}
