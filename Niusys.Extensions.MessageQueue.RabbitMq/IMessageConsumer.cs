using System;
using System.Collections.Generic;
using System.Text;

namespace Niusys.Extensions.MessageQueue.RabbitMq
{
    public interface IMessageConsumer
    {
        string SessionName { get; }
        string QueueName { get; }
        bool IsStoped { get; }
        void StartConsume(string sessionName);
        void PrepareClose();
        void StopConsume();
    }
}
