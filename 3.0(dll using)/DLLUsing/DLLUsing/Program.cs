using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClientServerFileSharing;

namespace DLLUsing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Server or Client:(S/C): ");
            string choose = Console.ReadLine();

            if (choose == "S")
            {
                Console.WriteLine("Start as server");
                var server = new Server("127.0.0.1", 11000);
                //тут можно задать папку где хранятся файлы сервера
               // server.SetStoragePlace("G:\\Учеба\\tmp\\Server\\");
                server.StartListen();
            }
            else
            {
                Console.WriteLine("Start as client");
                var client = new Client("127.0.0.1", 11000);
                //тут можно задать папку куда будут сохранятся файлы клиента
                //client.SetStoragePlace("G:\\Учеба\\tmp\\Client\\");

                while (true)
                {
                    Console.Write("Enter command: ");
                    string command = Console.ReadLine().Trim();
                    if (command == "") command = " ";

                    Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSocket.Connect(client.ipEndPoint);

                    client.SendRequestToServer(clientSocket, command);
                    string serverAnswer = client.GetAnswerFromServer(clientSocket);

                    Console.WriteLine(serverAnswer);

                    client.GetFile(clientSocket, command, serverAnswer);

                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
        }
    }
}
