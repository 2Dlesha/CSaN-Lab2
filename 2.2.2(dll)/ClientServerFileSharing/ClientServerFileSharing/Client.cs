using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClientServerFileSharing
{
    public class Client
    {
        public IPAddress ipAddr;
        public IPEndPoint ipEndPoint;
        public string defaultStoragePlace = "C:\\serverstorrage";//"G:\\Учеба\\tmp\\Client\\";

        public void SetStoragePlace(string place)
        {
            defaultStoragePlace = place;
        }

        public Client(string ip, int port)
        {
            ipAddr = IPAddress.Parse(ip);
            ipEndPoint = new IPEndPoint(ipAddr, port);
        }

        //прием и сохранение файла
        public void GetFileFromServer(Socket socket, string fileName)
        {
            if (!Directory.Exists(defaultStoragePlace))
                Directory.CreateDirectory(defaultStoragePlace);

            using (FileStream fstream = new FileStream(string.Concat(defaultStoragePlace, fileName, ".csv"), FileMode.OpenOrCreate))
            {
                var array = new byte[256];
                int bytes = 0;
                do
                {
                    bytes = socket.Receive(array, array.Length, 0);
                    fstream.Write(array, 0, bytes);
                }
                while (socket.Available > 0);
            }
        }

        //Обработка приема файла с сервера
        public void GetFile(Socket socket, string command, string serverAnswer)
        {
            if (serverAnswer.IndexOf("yes") != -1)
            {
                if (command.IndexOf("End") != -1)
                {
                    string fileName = GetAnswerFromServer(socket);
                    GetFileFromServer(socket, fileName);
                }
                else
                {
                    GetFileFromServer(socket, command);
                }
            }
        }

        //запрос и получение файла без вывода на экран
        public void SendCommandToServer(string command)
        {
            string message = command;
            if (message == "") message = " ";

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(ipEndPoint);

            SendRequestToServer(clientSocket, message);
            string serverAnswer = GetAnswerFromServer(clientSocket);

            GetFile(clientSocket, message, serverAnswer);

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        //отправка сообщения на сервер
        public void SendRequestToServer(Socket clientSocket, string command)
        {
            byte[] data = Encoding.Unicode.GetBytes(command);
            clientSocket.Send(data);
        }

        //получение сообщения с сервера
        public string GetAnswerFromServer(Socket clientSocket)
        {
            var data = new byte[256];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = clientSocket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (clientSocket.Available > 0);
            return builder.ToString();
        }

        //преобразование команды в имя файла
        public string GetFileNameFromAnswer(string command)
        {
            return command.Replace("File", "Weight");
        }
    }
}
