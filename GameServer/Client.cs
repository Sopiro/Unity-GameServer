using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Client
    {
        public static int dataBufferSize = 4096;
        public int id;
        public TCP tcp;

        public Client(int clinetId)
        {
            id = clinetId;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient socket = null;

            private readonly int id;

            private NetworkStream stream;
            private byte[] receiveBuffer;

            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;
                  
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    int byteLen = stream.EndRead(ar);
                    if(byteLen <= 0)
                    {
                        return;
                    }

                    byte[] data = new byte[byteLen];
                    Array.Copy(receiveBuffer, data, byteLen);

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reveiving TCP data:{e}");
                }
            }
        }
    }
}
