using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    public static class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();

            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();

            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i == _exceptClient) continue;

                Server.clients[i].tcp.SendData(_packet);
            }
        }

        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet p = new Packet((int)ServerPackets.welcome))
            {
                p.Write(_msg);
                p.Write(_toClient);

                SendTCPData(_toClient, p);
            }
        }
    }
}
