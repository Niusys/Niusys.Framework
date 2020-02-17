using System;

namespace Niusys.Extensions.DependencyInjection
{
    [Flags]
    public enum ExecuteOrderType
    {
        /// <summary>
        /// Lowest
        /// </summary>
        Lowest = -2,
        /// <summary>
        /// Lower
        /// </summary>
        Lower = -1,
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Higher
        /// </summary>
        Higher = 1,
        /// <summary>
        /// Highest
        /// </summary>
        Highest = 2
    }
}
