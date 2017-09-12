using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Web.WebSocket
{
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _sockets = new ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();

        public System.Net.WebSockets.WebSocket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> GetAll()
        {
            return _sockets;
        }

        public string GetId(System.Net.WebSockets.WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public void AddSocket(System.Net.WebSockets.WebSocket socket, string userId)
        {
            _sockets.TryAdd(userId, socket);
        }

        public async Task RemoveSocket(string id)
        {
            System.Net.WebSockets.WebSocket socket;
            _sockets.TryRemove(id, out socket);

            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: "Closed by the WebSocketManager",
                cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        //private string CreateConnectionId()
        //{
        //    return Guid.NewGuid().ToString();
        //}
    }
}
