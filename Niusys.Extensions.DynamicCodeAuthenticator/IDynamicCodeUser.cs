using System.Collections.Generic;
using System.Linq;

namespace Niusys.Extensions.DynamicCodeAuthenticator
{
    public interface IDynamicCodeUser
    {
        string UserName { get; }
        string DynamicCodeSecret { get; set; }
        bool IsDynamicCodeEnabled { get; set; }
    }
}
