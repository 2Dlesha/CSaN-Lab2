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
    public class Server
    {
        public IPAddress ipAddr;
        public IPEndPoint ipEndPoint;
        public string defaultStoragePlace = "C:\\serverstorrage";// "G:\\Учеба\\tmp\\Server\\";

        public void SetStoragePlace(string place)
        {
            defaultStoragePlace = place;
        }

        public Server(string ip, int port)
        {
            ipAddr = IPAddress.Parse(ip);
            ipEndPoint = new IPEndPoint(ipAddr, port);
        }

        //Бесконечная прослушка клиентов
        public void StartListen()
        {
            if (!Directory.Exists(defaultStoragePlace))
                Directory.CreateDirectory(defaultStoragePlace);
    
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(10);

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                string command = GetComandFromClient(clientSocket);

                if (AnswerOnRequest(clientSocket, command))
                {
                    SendFile(clientSocket, command);
                }

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }

        //отправка заданного файла клиенту
        public void SendFile(Socket socket, string fileName)
        {
            string srcFile;
            if (fileName.IndexOf("End") != -1)
            {
                srcFile = GetNewestFile(defaultStoragePlace, fileName);
                Encoding.Unicode.GetBytes(srcFile);
                socket.Send(Encoding.Unicode.GetBytes(srcFile));
                srcFile = string.Concat(defaultStoragePlace, srcFile, ".csv");
            }
            else
            {
                srcFile = string.Concat(defaultStoragePlace, fileName, ".csv");
            }
            socket.SendFile(srcFile);
        }

        //Создание сообщения-ответа на запрос клиента
        public string AnswerString(string command, Date date)
        {
            string dateTime = date.year.ToString() + "-" + date.month.ToString() + "-" +date.day.ToString(); 
            if (command.IndexOf("Super") != -1)
            {
                return string.Concat("SuperFile ", dateTime);
            }
            return string.Concat("File ", dateTime);
        }

        public string AnswerString(string command)
        {
            if (command.IndexOf("Super") != -1)
            {
                return"SuperFile End";
            }
            return "File End";
        }

        //прием сообщения от клиента
        public string GetComandFromClient(Socket socket)
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];
            do
            {
                bytes = socket.Receive(data);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);
            return builder.ToString();
        }

        //ответ на запрос клиента
        public bool AnswerOnRequest(Socket socket, string command)
        {
            string message;
            if (IsCommandCorrect(command))
            {
                if (FileExist(command))
                {
                    message = string.Concat(AnswerString(command,GetDateFromString(command)), " yes");
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    socket.Send(data);
                    return true;
                }
                else
                {
                    if (command.IndexOf("End") != -1)
                    {
                        if (Directory.GetFiles(defaultStoragePlace).Length != 0)
                        {
                            if (GetNewestFile(defaultStoragePlace, command) != "none")
                            {
                                message = string.Concat(AnswerString(command, GetDateFromString(GetNewestFile(defaultStoragePlace, command))), " yes");
                                byte[] data1 = Encoding.Unicode.GetBytes(message);
                                socket.Send(data1);
                                return true;
                            }
                            else
                            {
                                message = string.Concat(AnswerString(command), " no");
                                byte[] data1 = Encoding.Unicode.GetBytes(message);
                                socket.Send(data1);
                                return false;
                            }
                        }
                        else
                        {
                            message = string.Concat(AnswerString(command), " no");
                            byte[] data1 = Encoding.Unicode.GetBytes(message);
                            socket.Send(data1);
                            return false;
                        }
                    }
                    message = string.Concat(AnswerString(command, GetDateFromString(command)), " no");
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    socket.Send(data);
                    return false;
                }
            }

            byte[] errorData = Encoding.Unicode.GetBytes("Incorrect Command");
            socket.Send(errorData);
            return false;
        }

        //проверка корректности команды
        public bool IsCommandCorrect(string command)
        {
            Regex regex1 = new Regex(@"^(Super)?Resweight\s[0-9]{4}-[0-9]{2}-[0-9]{2}$");
            Regex regex2 = new Regex(@"^(Super)?Resweight\sEnd$");
            return (regex1.IsMatch(command) || regex2.IsMatch(command));
        }

        //проверка существования файла
        public bool FileExist(string fileName)
        {
            if (File.Exists(string.Concat(defaultStoragePlace, fileName, ".csv")))
                return true;
            return false;
        }

        public struct Date
        {
            public Date(int d, int m, int y)
            {
                day = d;
                month = m;
                year = y;
            }
            public int day;
            public int month;
            public int year;
        }

        //извлечение даты из строки
        public Date GetDateFromString(string stringWithDate)
        {
            string dt = (Regex.Match(stringWithDate, @"[0-9]{4}-[0-9]{2}-[0-9]{2}")).Value;
            string[] dmy = Regex.Split(dt, "-");
            Date date = new Date(0, 0, 0); ;
            if (dmy.Length == 3)
            {
                date = new Date(Convert.ToInt32(dmy[2]), Convert.ToInt32(dmy[1]), Convert.ToInt32(dmy[0]));
            }
            return date;
        }

        //получение самого свежего файла
        public string GetNewestFile(string directoryName,string command)
        {
            string[] allFiles = Directory.GetFiles(directoryName);
            List<string> files = new List<string>();
            if (command.IndexOf("Super") != -1)
            {
                for (int i = 0; i < allFiles.Length; i++)              
                    if (allFiles[i].IndexOf("Super") != -1)  files.Add(allFiles[i]);           
            }
            else
            {
                for (int i = 0; i < allFiles.Length; i++)
                    if (allFiles[i].IndexOf("Super") == -1)  files.Add(allFiles[i]);
            }

            if (files.Count > 0)
            {
                Date maxDate = GetDateFromString(files[0]);
                string newestFile = Path.GetFileNameWithoutExtension(files[0]);

                Date buf = new Date(0, 0, 0);

                for (int i = 0; i < files.Count; i++)
                {
                    buf = GetDateFromString(files[i]);
                    int date1 = Convert.ToInt32(string.Concat(maxDate.year.ToString(), maxDate.month.ToString(), maxDate.day.ToString()));
                    int date2 = Convert.ToInt32(string.Concat(buf.year.ToString(), buf.month.ToString(), buf.day.ToString()));
                    if (date1 < date2)
                    {
                        maxDate = buf;
                        newestFile = Path.GetFileNameWithoutExtension(files[i]);
                    }
                }
                return newestFile;
            }
            return "none";
        }
    }
}
