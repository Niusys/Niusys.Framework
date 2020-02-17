using Niusys.Extensions.Storage.Mongo;
using System;

namespace Niusys.Extensions.Buses
{
    public class MessageHandlerResult : MongoEntity
    {
        public enum MessageTypeEnum { Command = 1, Event = 2 }
        public enum HandleResultEnum { Success = 1, Fail = -1, PartialSuccess = 2 }
        public Guid MessageId { get; set; }
        public string RoutingKey { get; set; }
        public int MessageType { get; set; }
        public string MessageBody { get; set; }
        public string HandlerNode { get; set; }
        public DateTime Logged { get; set; }
        public int HandleResult { get; set; }
        public string ResultMessage { get; set; }
        public long Duration { get; set; }
    }
}
