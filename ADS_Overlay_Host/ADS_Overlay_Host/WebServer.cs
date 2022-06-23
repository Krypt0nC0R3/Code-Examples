using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using WebSocketSharp.Server;
using WebSocketSharp;

namespace Krypt0nWebServer
{
    class WebServer
    {
        /// <summary>
        /// Адрес сервера, по умлочанию 127.0.0.1
        /// </summary>
        public IPAddress IP { get; set; }
        /// <summary>
        /// Порт сервера, по умлочанию 6574
        /// </summary>
        public int Port { get { return port; } set { port = (value > 0) ? value : 6574; } }
        /// <summary>
        /// Путь к корню сервера, по умолчанию текущая папка
        /// </summary>
        public string Root { get { return root; } set { root = (!String.IsNullOrEmpty(value)) ? value : "/"; } }
        public bool IsRunning { get; private set; }

        private int port;
        private string root;
        private int timeout = 8;
        private Encoding charEncoder = Encoding.UTF8;
        private Socket serverSocket;
        private WebSocketServer Websoket;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public WebServer()
        {
            IP = IPAddress.Loopback;
            port = 6574;
            root = @"\";
            IsRunning = false;
        }
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="IP">IP адрес сервера</param>
        /// <param name="Port">Порт</param>
        /// <param name="Root">Путь к корню сервера</param>
        public WebServer(IPAddress IP, int Port, string Root)
        {
            this.IP = IP;
            port = Port;
            root = Root;
        }

        public bool Start()
        {
            if (IsRunning) return false;
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IP, port));
                serverSocket.Listen(10);
                serverSocket.ReceiveTimeout = timeout;
                serverSocket.SendTimeout = timeout;
                IsRunning = true;
                Websoket = new WebSocketServer("ws://" + IP.ToString() + ":" + (Port + 1).ToString());
                Websoket.AddWebSocketService<WS>("/Values");
                //Websoket.AuthenticationSchemes = WebSocketSharp.Net.AuthenticationSchemes.None;
                Websoket.Start();
                WebSocketServiceHost host;
                Websoket.WebSocketServices.TryGetServiceHost("/Values", out host);
            }
            catch { return false; }
            Thread requestListenerT = new Thread(() =>
            {
                while (IsRunning)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = serverSocket.Accept();
                        // Создаем новый поток для нового клиента и продолжаем слушать сокет.
                        Thread requestHandler = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;
                            try { handleTheRequest(clientSocket); }
                            catch
                            {
                                try { clientSocket.Close(); } catch { }
                            }
                        });
                        requestHandler.Start();
                    }
                    catch { }
                }
            });
            requestListenerT.Start();

            return true;
        }

        public bool Stop()
        {
            if (!IsRunning) return false;
            IsRunning = false;
            try
            {
                WebSocketServiceHost host;
                Websoket.WebSocketServices.TryGetServiceHost("/Values", out host);
                //host.Sessions.Broadcast("0");
                Websoket.Stop();
                serverSocket.Close();
            }
            catch { }
            serverSocket = null;
            Websoket = null;
            return true;
        }
        /// <summary>
        /// Отправляет информацию если WebSocket открыт
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Возвращает true при успешной передаче</returns>
        public bool SendWSMessage(string message)
        {
            if (!IsRunning) return false;
            WebSocketServiceHost host;
            Websoket.WebSocketServices.TryGetServiceHost("/Values", out host);
            System.Diagnostics.Debug.WriteLine(host.Sessions.Count);
            System.Diagnostics.Debug.WriteLine(host.Sessions.IDs);
            if (host.Sessions.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No WS sessions!");
                return false;
            }

            else
            {
                host.Sessions.Broadcast(message);
            }

            //Websoket.WebSocketServices.Broadcast(Encoding.UTF8.GetBytes(message));
            return true;
        }
        private void handleTheRequest(Socket clientSocket)
        {
            byte[] buffer = new byte[10240]; // 10 kb, just in case
            int receivedBCount = clientSocket.Receive(buffer); // Получаем запрос
            string strReceived = charEncoder.GetString(buffer, 0, receivedBCount);

            // Парсим запрос
            string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));

            int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = strReceived.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = strReceived.Substring(start, length);
            string requestedFile;
            if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
                requestedFile = requestedUrl.Split('?')[0];
            else // Вы можете реализовать другие методы
            {
                notImplemented(clientSocket);
                return;
            }

            requestedFile = requestedFile.Replace("/", "\\").Replace("\\..", ""); // Not to go back
            start = requestedFile.LastIndexOf('.') + 1;
            if (start > 0)
            {
                length = requestedFile.Length - start;
                string extension = requestedFile.Substring(start, length);
                if (File.Exists(root + requestedFile)) // Если да
                                                       // ТО отсылаем запрашиваемы контент:
                    sendOkResponse(clientSocket, File.ReadAllBytes(root + requestedFile), "text/html");
                else
                    notFound(clientSocket); // Мы не поддерживаем данный контент.
            }
            else
            {
                // Если файл не указан, пробуем послать index.html
                // Вы можете добавить больше(например "default.html")
                if (requestedFile.Substring(length - 1, 1) != "\\")
                    requestedFile += "\\";
                if (File.Exists(root + requestedFile + "index.htm"))
                    sendOkResponse(clientSocket, File.ReadAllBytes(root + requestedFile + "\\index.htm"), "text/html");
                else if (File.Exists(root + requestedFile + "index.html"))
                    sendOkResponse(clientSocket, File.ReadAllBytes(root + requestedFile + "\\index.html"), "text/html");
                else
                    notFound(clientSocket);
            }
        }
        private void notImplemented(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset = utf - 8\"></head><body><h2>Krypt0n's Server</h2>501 - Method Not Implemented</body></html> ", "501 Not Implemented", "text/html");

        }

        private void notFound(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><metahttp - equiv =\"Content-Type\" content=\"text/html; charset = utf - 8\"></head><body><h2>Krypt0n's Server </h2>404 - Not Found</body></html>", "404 Not Found", "text/html");
        }

        private void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {
            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }

        private void sendResponse(Socket clientSocket, string strContent, string responseCode, string contentType)
        {
            byte[] bContent = charEncoder.GetBytes(strContent);
            sendResponse(clientSocket, bContent, responseCode, contentType);
        }

        private void sendResponse(Socket clientSocket, byte[] bContent, string responseCode, string contentType)
        {
            try
            {
                byte[] bHeader = charEncoder.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: Atasoy Simple Web Server\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch { }
        }
    }

    class WS : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Send(Encoding.UTF8.GetBytes("Not Implemented!"));
        }
        public void SendMessage(string message)
        {

        }
    }
}
