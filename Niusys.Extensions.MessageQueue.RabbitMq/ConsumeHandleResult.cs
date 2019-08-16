namespace Niusys.Extensions.MessageQueue.RabbitMq
{
    public enum ConsumeHandleResult
    {
        /// <summary>
        /// 消息被正确处理已确认
        /// </summary>
        Act = 1,
        /// <summary>
        /// 消息消费出现异常, 被转移到错误队列
        /// </summary>
        ActAndMoveToErrorQueue = 2,

        /// <summary>
        /// 消息消费出现异常, 被退回队列, 重新尝试消费
        /// </summary>
        Reject = -1
    }
}
