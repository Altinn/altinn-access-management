using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Core.Telemetry;

/// <summary>
/// Collection of MetricNames used in Open Telemetry
/// </summary>
public static class TelemetryMetricNames
{
    private static string BaseName { get { return "altinn.accessmanagement."; } }

    /// <summary>
    /// Counter for counting each insert
    /// </summary>
    public static string CounterDelegationInsertCount { get { return BaseName + "delegation.insert.counter"; } }

    public static class Api
    {
        private static string BaseName { get { return "altinn.accessmanagement.api"; } }

        public static string MaskinPortenRequests { get { return BaseName + "maskinporten.offered"; } }
    }
}
