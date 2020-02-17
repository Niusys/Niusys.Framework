using Niusys.Buses.Messages;
using System;

namespace Niusys.Buses.Events
{
    public abstract class DomainEvent : Message, IDomainEvent
    {
        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
        }

        public Guid EventId { get; set; }

        public DateTime TimeStamp { get; set; }

        public Guid? TimelineId { get; set; }
    }
}
