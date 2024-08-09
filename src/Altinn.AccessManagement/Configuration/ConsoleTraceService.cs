using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Models;
using Yuniql.Extensibility;

namespace Altinn.AccessManagement.Configuration
{
    /// <summary>
    /// Copied from sample project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConsoleTraceService(ILogger logger) : ITraceService
    {
        /// <inheritdoc/>
        public bool IsDebugEnabled { get; set; } = false;

        /// <inheritdoc/>
        public bool IsTraceSensitiveData { get; set; } = false;

        /// <inheritdoc/>
        public bool IsTraceToFile { get; set; } = false;

        /// <inheritdoc/>
        public bool IsTraceToDirectory { get; set; } = false;

        /// <inheritdoc/>
        public string TraceDirectory { get; set; }

        /// <inheritdoc/>
        public void Info(string message, object payload = null)
        {
            logger.LogInformation(message, payload);
        }

        /// <inheritdoc/>
        public void Error(string message, object payload = null)
        {
            logger.LogError(message, payload);
        }

        /// <inheritdoc/>
        public void Debug(string message, object payload = null)
        {
            logger.LogDebug(message, payload);
        }

        /// <inheritdoc/>
        public void Success(string message, object payload = null)
        {
            logger.LogInformation(message, payload);
        }

        /// <inheritdoc/>
        public void Warn(string message, object payload = null)
        {
            logger.LogInformation(message, payload);
        }
    }
}
