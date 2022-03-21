using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Program
    {
        static public int queue = 0;
        static public int maxRequest;
        static void Main(string[] args)
        {
            const int port = 8888;
            TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            Console.WriteLine("Введите максимальное количество одновременно обрабатываемых запросов");
            maxRequest = Convert.ToInt32(Console.ReadLine());

            try
            {

                serverSocket.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = serverSocket.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);
                    clientObject.AddUser();
                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Request));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (serverSocket != null)
                    serverSocket.Stop();
            }
        }
    }
}