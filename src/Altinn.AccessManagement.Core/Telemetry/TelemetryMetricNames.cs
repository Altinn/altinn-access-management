using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Core.Telemetry;

/// <summary>
/// Collection of MetricNames used in Open Telemetry
/// Experimental
/// </summary>
[ExcludeFromCodeCoverage]
public static class TelemetryMetricNames
{
    /// <summary>
    /// Base name for all metrics
    /// </summary>
    private static string BaseName
    {
        get { return "altinn.accessmanagement"; }
    }

    /// <summary>
    /// Counter for counting each insert
    /// </summary>
    public static string CounterDelegationInsertCount
    {
        get { return BaseName + ".delegation.insert.counter"; }
    }

    /// <summary>
    /// Collection of metricnames for Api
    /// </summary>
    public static class Api
    {
        /// <summary>
        /// Base name for Api names
        /// </summary>
        private static string Name
        {
            get { return BaseName + ".api"; }
        }

        /// <summary>
        /// Count of request made by Maskinporten
        /// </summary>
        public static string MaskinPortenRequests
        {
            get { return Name + ".maskinporten.offered"; }
        }
    }
}
