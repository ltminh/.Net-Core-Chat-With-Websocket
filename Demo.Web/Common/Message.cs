using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Web.Common
{
    public enum MessageType
    {
        Text,
        ClientMethodInvocation,
        ConnectionEvent
    }

    public class Message
    {
        public MessageType MessageType { get; set; }
        public string Data { get; set; }
    }
}
