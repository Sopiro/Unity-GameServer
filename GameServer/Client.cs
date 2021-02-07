using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    public class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public TCP tcp;
        public UDP udp;

        public Client(int _clinetId)
        {
            id = _clinetId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient Socket { get; private set; }

            private readonly int id;

            private NetworkStream stream;
            private byte[] receiveBuffer;

            private Packet receivedData;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                Socket = _socket;

                Socket.ReceiveBufferSize = dataBufferSize;
                Socket.SendBufferSize = dataBufferSize;

                stream = Socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                // Start reading steam asynchronously with callback function
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, Constants.WELLCOME_MSG);
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    int byteLen = stream.EndRead(ar);
                    if (byteLen <= 0) return;

                    byte[] data = new byte[byteLen];
                    Array.Copy(receiveBuffer, data, byteLen);

                    receivedData.Reset(HandleData(data));

                    // Restart reading
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reveiving TCP data:{e}");
                }
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP:{e}");
                }
            }

            private bool HandleData(byte[] _data)
            {
                int packetLen = 0;

                receivedData.SetBytes(_data);

                // Read packet length
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLen = receivedData.ReadInt();
                    if (packetLen <= 0)
                    {
                        return true;
                    }
                }

                // Handling receibed packet via corresponding packet hanlder function
                while (packetLen > 0 && packetLen <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLen);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet p = new Packet(packetBytes))
                        {
                            int packetId = p.ReadInt();
                            Server.packetHandlers[packetId](id, p);
                        }
                    });

                    packetLen = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLen = receivedData.ReadInt();
                        if (packetLen <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLen <= 1)
                {
                    return true;
                }

                return false;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packet)
            {
                int packetLen = _packet.ReadInt();
                byte[] packetBytes = _packet.ReadBytes(packetLen);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet p = new Packet(packetBytes))
                    {
                        int packetId = p.ReadInt();

                        Server.packetHandlers[packetId](id, p);
                    }
                });
            }
        }
    }
}
