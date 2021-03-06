﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Demo.Web.Common;
using Demo.Web.WebSocket;

namespace Demo.Web.Hubs
{
    public class ChatHub : WebSocketHandler
    {
        public ChatHub(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {
        }

        public override async Task OnConnected(System.Net.WebSockets.WebSocket socket, string userId)
        {
            await base.OnConnected(socket, userId);

            //var socketId = WebSocketConnectionManager.GetId(socket);
          
        }



        public override async Task OnDisconnected(System.Net.WebSockets.WebSocket socket)
        {
            //var socketId = WebSocketConnectionManager.GetId(socket);

            await base.OnDisconnected(socket);
        }
    }
}
