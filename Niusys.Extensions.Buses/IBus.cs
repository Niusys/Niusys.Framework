using Niusys.Buses.Commands;
using Niusys.Buses.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Niusys.Buses
{
    public interface IBus
    {
        Task Send(Command message, Dictionary<string, string> optionalHeaders = null, bool isLongTimeMessage = false);
        Task Send(DomainEvent message, Dictionary<string, string> optionalHeaders = null, bool isLongTimeMessage = false);
        Task EntityInserted(object entity);
        Task EntityUpdated(object entity);
        Task EntityDeleted(object entity);
    }
}
