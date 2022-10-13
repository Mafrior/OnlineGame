using UnityEngine;
using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    public static Client Inst;
    public static int dataBufferSize = 4096;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    public Person player; // Скрипт, которым мы управляем

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected;

    private void Awake()
    {
        if (Inst == null) {
            Inst = this;
            return;
        }
        if(Inst == this)
        {
            Debug.Log("Instznce already exists");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    public void ConnectToServer()
    {
        InitializeClientData();
        isConnected = true;
        tcp.Connect();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    // Вырубает свет если игрок просто отключается
    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();
        }
    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedData;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(Inst.ip, Inst.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);
            if (!socket.Connected)
            {
                return;
            }
            receivedData = new Packet();
            stream = socket.GetStream();
            //Начинает Асинхронно методу считывать данные, если они прийдут, то записывает их в буфер, с позиции ноль,
            //количеством dataBufferSize и вызывает ReceivedCallback
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result); //_result - результат асинхроного вызова
                if(_byteLength <= 0){
                    Inst.Disconnect();
                    return;
                }
                byte[] data = new byte[dataBufferSize];
                //Копируем данные из буфера, чтобы передать его дальше на асинхронное чтение
                Array.Copy(receiveBuffer, data, _byteLength);
                //Начинает обработку переданных (_data) данных и обновляет пакет, если это необходимо,
                //так как BeginStream вызывается асинхронно и может передать больше данных
                receivedData.Reset(HandleData(data));
                //Начинает Асинхронно методу считывать данные, если они прийдут, то записывает их в буфер, с позиции ноль,
                //количеством dataBufferSize и вызывает ReceivedCallback
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        //Очищает пакет, если вернёт true. Так как бывает так(при сбоях), что наши пакеты разделились на несколько доставок, или соедениться в одну, 
        //а потом разломаться, нам необходимо точно понимать, когда обновлять данные, а когда подождать до полного прихода
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0; //Инициализируем длину пакета
            receivedData.SetBytes(_data); //Присваиваем новому пакету данные, которые пришли от сервера, считанные в ReceiveCallback
            if(receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt(); //Устанавливаем длину пакета
                if(_packetLength <= 0)
                {
                    return true; //Здесь очищает, так как пришёл пакет с написанной длинной, но длина равна нулю
                }
            }
            //Здесь мы выполняем считывание пакета, пока длина пакета больше нуля, пока не будут считанны все пакеты, которые пришли,
            //так как длина пакета указывает на общую длину, а там может содержаться несколько методов для обработки, которые мы последовательно считываем
            while(_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                //Считали данные пакета (включая номер пакета от сервера) и считываем их согласно PacketHandler и одновременно вызываем поток 
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });
                _packetLength = 0;
                if(receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if(_packetLength <= 0)
                    {
                        return true; //Здесь также очищаем после обработки пакте, так как дальше идёт размер пакета равный нулю
                    }
                }
            }
            if(_packetLength <= 1)
            {
                return true; //Если от пакета остался один бит, то тоже можно стирать
            }
            //Если же длина оказалась больше, то мы не reset'аем, а значит откатываем наш readPos назад до длины, чтобы при прихождении полного пакета,
            //мы начали обработку с "начала"
            return false;  

        }

        public void SendData(Packet _packet)
        {
            try
            {
                if(socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch
            {
                Debug.Log("Cant send the data");
            }
        }
        
        // Вырубает свет если игрок подключился, но подключение кривое и не корректное
        private void Disconnect()
        {
            Inst.Disconnect();

            stream = null;
            receiveBuffer = null;
            receivedData = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(Inst.ip), Inst.port);
        }
        //Подключаемся только после подключения TCP в ClientHandle.Welcome
        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(Inst.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    Inst.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        //Так как в UDP протоколе нам не важно, дойдёт ли информация до игрока или нет, то мы не особо занимаемся её тщательной проверкой,
        //а значит при сбое доставки, наш пакет просто не обработается корректно пользователем
        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();
                    if (_packetId >= 3)
                    {
                        ActionManager.Inst.MoveQueue();
                    }
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        // Вырубает свет если игрок подключился, но подключение кривое и не корректное
        private void Disconnect()
        {
            Inst.Disconnect();

            socket = null;
            endPoint = null;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.fieldInitialize, ClientHandle.GetField },
            { (int)ServerPackets.someOneMove, ClientHandle.PlayerMoveTo },
            { (int)ServerPackets.someOnePullPlatformOrPerson, ClientHandle.PlatformPulled},
            { (int)ServerPackets.someOneDestroyWall, ClientHandle.WallDestroied},
            { (int)ServerPackets.giveArtifact, ClientHandle.ArtifactMoved}
        };
    }
}
