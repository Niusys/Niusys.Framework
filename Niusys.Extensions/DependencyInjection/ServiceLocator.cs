using System;

namespace Niusys.Extensions.DependencyInjection
{
    /// <summary>
    /// Service provider
    /// </summary>
    /// <remarks>
    /// This is a anti-pattern, but it works well with NLog, and NLog should also support non-DI
    /// </remarks>
    public static class ServiceLocator
    {
        /// <summary>
        /// The current service provider for reading ASP.NET Core session, request etc.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; set; }
    }
}
