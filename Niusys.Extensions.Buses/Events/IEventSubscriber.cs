using System.Threading.Tasks;

namespace Niusys.Buses.Events
{
    /// <summary>
    /// Event遵循pub/sub模式, pub方通常为CommandHandler, 可以有多个EventHandler
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventSubscriber<in TEvent>
    {
        Task Handle(TEvent message);
    }
}
