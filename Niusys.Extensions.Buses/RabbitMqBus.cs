using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Niusys.Buses.Commands;
using Niusys.Extensions.DependencyInjection;
using Niusys.Extensions.MessageQueue.RabbitMq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Niusys.Buses
{
    public class RabbitMqBus<TMessagePublishPool> : IBus, IScopedDependency
        where TMessagePublishPool : PublishPool
    {
        private readonly ILogger _logger;

        public RabbitMqBus(ILogger<RabbitMqBus<TMessagePublishPool>> logger, TMessagePublishPool defaultBizMessagePublishPool)

        {
            _logger = logger;
            messagePublishPool = defaultBizMessagePublishPool;
        }

        public TMessagePublishPool messagePublishPool { get; }

        public async Task Send(Command command, Dictionary<string, string> optionalHeaders = null, bool isLongTimeMessage = false)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.TimeStamp = DateTime.Now;
            var mainKey = isLongTimeMessage ? "miaoapi.longtimecommand" : "miaoapi.command";
            var routingKey = $"{mainKey}.{command.GetType().FullName.ToLower()}";
            messagePublishPool.Write(JsonConvert.SerializeObject(command), routingKey);
            await Task.CompletedTask;
        }

        public async Task Send(Niusys.Buses.Events.DomainEvent message, Dictionary<string, string> optionalHeaders = null, bool isLongTimeMessage = false)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message.TimeStamp = DateTime.Now;
            var mainKey = isLongTimeMessage ? "miaoapi.longtimeevent" : "miaoapi.event";
            var routingKey = $"{mainKey}.{message.GetType().FullName.ToLower()}";
            messagePublishPool.Write(JsonConvert.SerializeObject(message), routingKey);
            await Task.CompletedTask;
        }

        public async Task EntityInserted(object entity)
        {
            await EntityChangeEvent("inserted", entity);
        }

        public async Task EntityUpdated(object entity)
        {
            await EntityChangeEvent("updated", entity);
        }

        public async Task EntityDeleted(object entity)
        {
            await EntityChangeEvent("deleted", entity);
        }

        private async Task EntityChangeEvent(string operation, object entity)
        {
            var timeStamp = DateTime.Now;
            //var userSession = ServiceProvider.GetService<IUserSession>();
            var routingKey = $"miaoapi.entity.{operation}.{entity.GetType().FullName.ToLower()}";

            messagePublishPool.Write(JsonConvert.SerializeObject(new
            {
                Message = $"修改了实体{entity.GetType().FullName}",//$"IP:{userSession.FrontServerIp}上",
                EntityData = JsonConvert.SerializeObject(entity, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                TimeStamp = timeStamp
            }), routingKey);
            await Task.CompletedTask;
        }
    }
}
