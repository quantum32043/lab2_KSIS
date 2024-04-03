using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Dictionary<Socket, string> clientLogins = new Dictionary<Socket, string>();
        
        IPAddress ip = IPAddress.Parse("192.168.100.30");
        int port = 5000;
        
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(ip, port));
        IPEndPoint localEndPoint = serverSocket.LocalEndPoint as IPEndPoint;
        Console.WriteLine("IP сервера: " + serverSocket.LocalEndPoint!.ToString());
        serverSocket.Listen(10);

        while (true)
        {
            Socket client = serverSocket.Accept();
            Console.WriteLine("Кто-то подключился!");

            // Запрос логина
            byte[] loginPrompt = Encoding.UTF8.GetBytes("Введите свой логин: ");
            client.Send(loginPrompt);

            byte[] loginBytes = new byte[1024];
            int loginByteCount = client.Receive(loginBytes);
            string login = Encoding.UTF8.GetString(loginBytes, 0, loginByteCount);

            // Сохранение логина
            clientLogins.Add(client, login);

            Box box = new Box(client, clientLogins);
            Thread t = new Thread(HandleClients);
            t.Start(box);
        }
    }

    public static void HandleClients(object o)
    {
        Box box = (Box)o;
        Dictionary<Socket, string> connections = box.Clients;

        while (true)
        {
            Socket client = box.Client;
            byte[] buffer = new byte[1024];
            int byteCount = client.Receive(buffer);
            string data = Encoding.UTF8.GetString(buffer, 0, byteCount);

            Broadcast(connections, client, data);
            Console.WriteLine(box.Clients[box.Client] + ": " + data);
        }
    }

    public static void Broadcast(Dictionary<Socket, string> connections, Socket client, string data)
    {
        string msg = "";
        foreach (Socket c in connections.Keys)
        {
            msg = connections[client] + ": " + data;
;            c.Send(Encoding.UTF8.GetBytes(msg));
        }
    }
}

class Box
{
    public Socket Client;
    public Dictionary<Socket, string> Clients;

    public Box(Socket client, Dictionary<Socket, string> clients)
    {
        Client = client;
        Clients = clients;
    }
}
