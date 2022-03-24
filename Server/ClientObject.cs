using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Server
{
    class ClientObject
    {
        NetworkStream stream = null;
        private string userName = "";
        public TcpClient client;
        static readonly object _locker = new();
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        private bool CheckPalindrom(string message)
        {
            char[] arr = message.ToCharArray();
            Array.Reverse(arr);
            string reverseMessage = new string(arr).ToLower();
            if (message.ToLower() == reverseMessage)
                return true;
            else
                return false;
        }

        private void DataDecoder(NetworkStream stream, byte[] data, ref StringBuilder builder)
        {
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);
        }

        public void AddUser()
        {
            //получаем имя
            stream = client.GetStream();
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            DataDecoder(stream, data, ref builder);
            userName = builder.ToString();
            //Console.WriteLine($"Подключился пользователь: {userName}");
        }

        public void Request()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            try
            {
                while (true)
                {
                    builder.Clear();
                    DataDecoder(stream, data, ref builder);
                    Thread.Sleep(50);
                    lock (_locker)
                    {
                        Program.queue++;
                        //Thread.Sleep(50);
                        Console.WriteLine($"Queue: {Program.queue}; ThreadID: {Thread.CurrentThread.ManagedThreadId}");
                    }
                    if (Program.queue <= Program.maxRequest)
                    {
                        string message = builder.ToString();
                        Console.WriteLine($"{userName}: {message}");
                        Thread.Sleep(1000);
                        message = Regex.Replace(message, @"[^\dа-яёА-ЯЁ]", "");
                        string palindromOrNot = "Ваше сообщение не явлется палиндромом";
                        if (CheckPalindrom(message))
                        {
                            palindromOrNot = "Ваше сообщение является палиндромом";
                        }
                        data = Encoding.Unicode.GetBytes(palindromOrNot);
                        
                    }
                    else
                    {
                        string error = "Количество запросов превысило возможности сервера. Прошу повторить попытку позже или уменьшить количество запросов\n";
                        data = Encoding.Unicode.GetBytes(error);
                    }
                    stream.Write(data, 0, data.Length);
                    Program.queue--;
                    Console.WriteLine(Program.queue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }
}
