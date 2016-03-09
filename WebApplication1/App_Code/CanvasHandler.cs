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
    public class CanvasHandler : IHttpHandler
    {
        private static string ch = "";//хранилище всех координат и кисточки 

        private static readonly List<WebSocket> Clients = new List<WebSocket>();// Список всех сокетов canvas

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();// Блокировка для обеспечения потокабезопасности
        
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

            Locker.EnterWriteLock();
            try
            {
                Clients.Add(socket);// Добавляем сокет в список
                //State.Add(сh);
            }
            finally
            {
                Locker.ExitWriteLock();
            }
            
            if(ch.Length != 0) await Send(ch, socket);//если хранилище не пустое отправляет новому клиенту содержимое хранилища

            while (true)// Слушаем его
            {
                var result = await Receve(socket);// Ожидаем данные от него
                
                if (result.Equals("n")) ch += result;//если пришел конец линии просто записываем его в хранилище не отпраляя клиентам 
                else
                {
                    if (result.Equals("o")) ch = ""; //если пришел знак обнулить хранилище обнуляем его
                    else ch += result; //иначе записываем то что пришло в хранилище и 
                    //в цикле передаём сообщение всем клиентам

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
        }




        private async Task<string> Receve(WebSocket socket)//прием сообщения от клиента
        {
            var buf = new ArraySegment<byte>(new byte[1024]);
            var result = await socket.ReceiveAsync(buf, CancellationToken.None);
            string rc = System.Text.Encoding.UTF8.GetString(buf.Array, 0, result.Count);
            return rc;
        }
        private async Task Send(string s, WebSocket socket)//отправка сообщения клиенту
        {
            var sbuf = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(s));
            await socket.SendAsync(sbuf, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}