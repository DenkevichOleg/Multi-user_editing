using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebSockets;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace kursovoi3sem3.App_Code
{
    public class IISHandler : IHttpHandler
    {
        string ch = "";
        // Список всех клиентов
        private static readonly List<WebSocket> Clients = new List<WebSocket>();
        // Блокировка для обеспечения потокабезопасности
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest) 
            {
                context.AcceptWebSocketRequest(WebSocketRequest);
            }
        }
        private async Task WebSocketRequest(AspNetWebSocketContext context) 
        {
            var socket = context.WebSocket;

            // Добавляем его в список клиентов
            Locker.EnterWriteLock();
            try
            {
                Clients.Add(socket);
            }
            finally
            {
                Locker.ExitWriteLock();
            }

            // Слушаем его
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);

                // Ожидаем данные от него
                var result = await Receve(socket);


                //Передаём сообщение всем клиентам
                for (int i = 0; i < Clients.Count; i++)
                {

                    WebSocket client = Clients[i];

                    try
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            await Send(result, client);
                        }
                    }

                    catch (ObjectDisposedException)
                    {
                        Locker.EnterWriteLock();
                        try
                        {
                            Clients.Remove(client);
                            i--;
                        }
                        finally
                        {
                            Locker.ExitWriteLock();
                        }
                    }
                }

            }
        }




        private async Task<string> Receve(WebSocket socket) 
        {
            var buf = new ArraySegment<byte>(new byte[1024]);
            var result = await socket.ReceiveAsync(buf, CancellationToken.None);
            string rc = System.Text.Encoding.UTF8.GetString(buf.Array, 0, result.Count);
            return rc;
        }
        private async Task Send(string s, WebSocket socket)
        {
            var sbuf = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(s));
            await socket.SendAsync(sbuf, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}