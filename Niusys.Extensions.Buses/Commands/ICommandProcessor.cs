using System.Threading.Tasks;

namespace Niusys.Buses
{
    /// <summary>
    /// 一个Command只能有一个Handler, Command的pub方为WorkService
    /// CommandHandler里面可以RaizeEvent
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandProcessor<in TCommand>
    {
        Task Process(TCommand command);
    }
}
