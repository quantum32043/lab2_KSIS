using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        IPAddress ip = IPAddress.Parse("192.168.100.30");
        int port = 5000;

        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(new IPEndPoint(ip, port));
        Console.WriteLine("Клиент подключился!");

        // Запрос ввода логина
        byte[] loginPromptBytes = new byte[1024];
        int loginPromptByteCount = client.Receive(loginPromptBytes);
        string loginPrompt = Encoding.UTF8.GetString(loginPromptBytes, 0, loginPromptByteCount);
        Console.Write(loginPrompt);

        string login = Console.ReadLine();
        client.Send(Encoding.UTF8.GetBytes(login));

        string input = "";
        while (input != "exit")
        {
            // Отправка сообщения
            Task.Run(async () =>
            {
                input = Console.ReadLine();
                await client.SendAsync(Encoding.UTF8.GetBytes(input));
            });

            // Получение ответа от сервера
            byte[] receivedBytes = new byte[1024];
            int byteCount = client.Receive(receivedBytes);
            string receivedData = Encoding.UTF8.GetString(receivedBytes, 0, byteCount);
            Console.WriteLine(receivedData);
        }

        client.Close();
        Console.WriteLine("Отключение от сервера!");
    }
}