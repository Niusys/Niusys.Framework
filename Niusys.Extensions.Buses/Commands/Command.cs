using Niusys.Buses.Messages;
using System;

namespace Niusys.Buses.Commands
{
    public abstract class Command : Message, ICommand
    {
        protected Command()
        {
            CommandId = Guid.NewGuid();
        }

        public Guid CommandId { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
