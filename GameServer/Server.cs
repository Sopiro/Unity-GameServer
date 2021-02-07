using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    public static class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting server...");

            InitServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            // Start accepting client asynchronously with callback funciton
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on port: {Port}.");
        }


        private static void TCPConnectCallback(IAsyncResult ar)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(ar);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint} ...");

            // Apply new client connection
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.Socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connet: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(ar, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet p = new Packet(data))
                {
                    int clientId = p.ReadInt();

                    if (clientId == 0)
                    {
                        return;
                    }

                    Client client = clients[clientId];

                    // Initial receiving for identifying 
                    if (client.udp.endPoint == null)
                    {
                        Console.WriteLine($"Udp connection established to: {clientId}");
                        client.udp.Connect(clientEndPoint); 
                        return;
                    }

                    if (client.udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        client.udp.HandleData(p);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error receiving UDP Data: {e.Message}");
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
            catch (Exception e)
            {
                Console.WriteLine($"Error sending UDP Data to {_clientEndPoint}: {e.Message}");
            }
        }

        private static void InitServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomReceived }
            };

            Console.WriteLine("Initialized packets.");
        }
    }
}
