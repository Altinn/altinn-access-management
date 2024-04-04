﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Altinn.AccessManagement.Core.Telemetry;

/// <summary>
/// Holds configuration for OpenTelemetry implementation
/// </summary>
[ExcludeFromCodeCoverage]
public static class TelemetryConfig
{
    /// <summary>
    /// Used as source for the current project
    /// </summary>
    public static readonly ActivitySource ActivitySource = new("Altinn.AccessManagement");

    /// <summary>
    /// Used as source for the current project
    /// </summary>
    public static readonly Meter Meter = new("Altinn.AccessManagement");
}