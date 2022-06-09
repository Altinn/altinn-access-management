using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Brigde.Enums
{
    /// <summary>
    /// Enum for deciding which authRequests to get
    /// </summary>
    public enum RestAuthorizationRequestDirection
    {
        /// <summary>
        /// Default none
        /// </summary>
        None = 0,

        /// <summary>
        /// Incoming requests
        /// </summary>
        Incoming = 1,

        /// <summary>
        /// Outgoing requests
        /// </summary>
        Outgoing = 2,

        /// <summary>
        /// Both incoming and outgoing
        /// </summary>
        Both = 3,
    }
}
