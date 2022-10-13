using System;
using System.Collections.Generic;
using System.Numerics;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace GameServer
{
	class Server
	{
		public static int MaxPlayers { get; private set; }

		public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); // В InitializeData
        public static Dictionary<int, Player> players = new Dictionary<int, Player>(); // В InitializeData
        public static Field field; // В GameInitialize
        private static int fieldSize;
        //public static int realPlayersCount => clients.Count(x => x.Value.tcp.socket != null);
        public static List<int> queue;
        public static int whichTurn = 0;

        public static Artifact[] artifacts;

		public static int Port { get; private set; }
		private static TcpListener tcpListener;
		private static UdpClient udpListener;


		public delegate void PacketHandler(int _fromClient, Packet _packet);
		public static Dictionary<int, PacketHandler> packetHandlers;

		public static void Start(int _maxPlayers, int _fieldSize, int _port)
		{
			MaxPlayers = _maxPlayers;
			Port = _port;
            fieldSize = _fieldSize;
			InitializeServerData();

			tcpListener = new TcpListener(IPAddress.Any, Port);
			tcpListener.Start();
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

			udpListener = new UdpClient(Port);
			udpListener.BeginReceive(UDPReceiveCallback, null);

			Console.WriteLine($"Server started on port: {Port}");
		}

		private static void TCPConnectCallback(IAsyncResult _result)
		{
			TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
			for (int i = 1; i <= MaxPlayers; i++)
			{
				if (clients[i].tcp.socket == null)
				{
					clients[i].tcp.Connect(_client);
                    Console.WriteLine($"We have Person {i}");
                    return;
				}
			}
			Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to Connect ");
		}

        //Здесь мы по сути обрабатываем поступающие подсоединения и сразу же их устанавливаем, так как UDP подразумевает, что нам достаточно обработать информацию
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    //Так как UDP идёт после TCP, значит у клиента уже должен быть ID, который он нам при передачи информации отправляет
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }
                    //Если у клиента нет udp подключения, значит это его первое подключение, следовательно просто передаём ему новый IP и порт
                    if (clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }
                    //Если он уже имеет подключение - обрабатываем присланный от него
                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving UDP data: {_ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
                clients[i].player = new Player(new Vector2(0, 0), i);
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.movePosition, ServerHandle.PlayerMove},
                { (int)ClientPackets.platformPull, ServerHandle.PlatformPull},
                { (int)ClientPackets.destroiedWall, ServerHandle.WallDestroied},
                { (int)ClientPackets.artifactMoved, ServerHandle.MoveArtifact}
            };
        }

        public static void GameInitilize()
        {
            field = new Field(fieldSize+2); // Инициализируем поле в нашем классе и записываем в пакет результат генерации
            artifacts = new Artifact[3];
            queue = new List<int>();

            Random r = new Random();

            Vector2 playerPosition;
            int turn;
            Vector2 artifactPosition;

            for (int i = 0; i < 3; i++)
            {
                artifactPosition = new Vector2(r.Next(1, field.Count - 2), r.Next(0, field.Count - 2));
                if (field[artifactPosition.Y, artifactPosition.X].isWater || artifacts.Any(x => x?.postition == artifactPosition))
                {
                    i--;
                    continue;
                }
                artifacts[i] = new Artifact(artifactPosition);
            }


            // Инициализируем игроков, добавляя их к каждому клиенту, а также в список Игроков, через конструктор, чтобы удобнее распределять их передвижения
            for (int i = 1; i <= MaxPlayers; i++)
            {
                playerPosition = new Vector2(r.Next(1, field.Count-2), r.Next(0, field.Count-2));
                if (field[playerPosition.Y, playerPosition.X].isWater || players.Any(x => x.Value.position == playerPosition))
                {
                    i--;
                    continue;
                }
                clients[i].player.position = playerPosition;

                // Инициализируем очередь по которой игроки будут ходить
                turn = r.Next(1, MaxPlayers + 1);
                while (queue.Count(x => x == turn) > 0)
                {
                    turn = r.Next(1, MaxPlayers + 1);
                }
                queue.Add(turn);
                Console.WriteLine(queue[i-1]);
            }
            ServerSend.SendGameProperties();
        }
    }
}
