using System;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Game Server";

            Server.Start(10, 1234);

            Console.ReadKey();
        }
    }
}
