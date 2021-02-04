using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomReceived(int _fromClient, Packet _packet)
        {
            int clientId = _packet.ReadInt();
            string userName = _packet.ReadString();

            Console.WriteLine($"{userName} [{Server.clients[_fromClient].tcp.Socket.Client.RemoteEndPoint}] connected successfully and is now {_fromClient}");

            if (_fromClient != clientId)
                Console.WriteLine($"Player \"{userName}\" (ID: {_fromClient} had assumed the wrong client ID ({clientId})");
        }
    }
}
