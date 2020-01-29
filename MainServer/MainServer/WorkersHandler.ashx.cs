using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace MainServer
{
    public class Todo
    {
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public Todo(string p1, string p2)
        {
            Param1 = p1;
            Param2 = p2;
        }
    }
    /// <summary>
    /// Сводное описание для WorkersHandler
    /// </summary>
    public class WorkersHandler : IHttpHandler
    {
        private static readonly Queue<Todo> todos = new Queue<Todo>();
        // Список всех клиентов
        private static readonly List<WebSocket> Clients = new List<WebSocket>();

        // Блокировка для обеспечения потокабезопасности
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public void ProcessRequest(HttpContext context)
        {
            //Если запрос является запросом веб сокета
            if (context.IsWebSocketRequest)
                context.AcceptWebSocketRequest(WebSocketRequest);
        }

        private async Task WebSocketRequest(AspNetWebSocketContext context)
        {
            // Получаем сокет клиента из контекста запроса
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
                var testBuffer = new ArraySegment<byte>(new byte[1024]);
                // Ожидаем данные от него
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);


                if (socket == Clients[0])
                {
                    
                    string[] separators = { ":", " " };
                    string[] words = message.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length == 2)
                        todos.Enqueue(new Todo(words[0], words[1]));

                    if (socket.State == WebSocketState.Open)
                    {
                        var msgBytes = Encoding.UTF8.GetBytes("status");
                        await Clients[1].SendAsync(new ArraySegment<byte>(msgBytes, 0, msgBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                        await Clients[2].SendAsync(new ArraySegment<byte>(msgBytes, 0, msgBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                        //await Clients[1].SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                if (socket == Clients[1])
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        if ((message == "w1 able") && (todos.Count != 0))
                        {
                            Todo todo = todos.Dequeue();
                            var msgBytes = Encoding.UTF8.GetBytes(todo.Param1 + " " + todo.Param2);
                            await Clients[1].SendAsync(new ArraySegment<byte>(msgBytes, 0, msgBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        else
                            await Clients[0].SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                if (socket == Clients[2])
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        if ((message == "w2 able")&&(todos.Count!=0))
                        {
                            Todo todo = todos.Dequeue();
                            var msgBytes = Encoding.UTF8.GetBytes(todo.Param1 + " " + todo.Param2);
                            await Clients[2].SendAsync(new ArraySegment<byte>(msgBytes, 0, msgBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        else
                            await Clients[0].SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }


                ////Передаём сообщение всем клиентам
                //for (int i = 0; i < Clients.Count; i++)
                //{

                //    WebSocket client = Clients[i];

                //    try
                //    {
                //        if (client.State == WebSocketState.Open)
                //        {
                //            await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                //        }
                //    }

                //    catch (ObjectDisposedException)
                //    {
                //        Locker.EnterWriteLock();
                //        try
                //        {
                //            Clients.Remove(client);
                //            i--;
                //        }
                //        finally
                //        {
                //            Locker.ExitWriteLock();
                //        }
                //    }
                //}

            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}