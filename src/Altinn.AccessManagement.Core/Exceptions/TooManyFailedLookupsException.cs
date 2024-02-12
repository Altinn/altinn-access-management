using System;
using System.Runtime.Serialization;

namespace Altinn.AccessManagement.Core
{
    /// <summary>
    /// Represents a situation where a user has performed too many failed lookup requests.
    /// </summary>
    [Serializable]
    public class TooManyFailedLookupsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyFailedLookupsException"/> class.
        /// </summary>
        public TooManyFailedLookupsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyFailedLookupsException"/> class.
        /// </summary>
        /// <param name="message">The message that descibes the error.</param>
        public TooManyFailedLookupsException(string message)
            : base(message)
        {
        }
    }
}
