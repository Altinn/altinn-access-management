using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Persistence.Configuration
{
    public static class TelemetryConfig
    {
        public static readonly ActivitySource activitySource = new ActivitySource("Altinn.AccessManagement.Persistence");
    }
}
