using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Contexts;

/// <summary>
/// A wrapper class used by mock implementation for getting data
/// </summary>
public class MockContext
{
    /// <summary>
    /// ctor
    /// </summary>
    public MockContext(params Action<MockContext>[] values)
    {
        foreach (var value in values)
        {
            value(this);
        }
    }

    /// <summary>
    /// List of mock resources
    /// </summary>
    public List<ServiceResource> Resources { get; set; } = [];

    /// <summary>
    /// List of mock parties
    /// </summary>
    public List<Party> Parties { get; set; } = [];

    /// <summary>
    /// List of mock user profiles
    /// </summary>
    public List<UserProfile> UserProfiles { get; set; } = [];

    /// <summary>
    /// Dictionary of mainunits. Where key is partyid of the subunit and values are the main units.
    /// </summary>
    public Dictionary<int, MainUnit> MainUnits { get; set; } = [];

    /// <summary>
    /// Dictionary of keyroles where the key is userid and value a list of party ids.
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, List<int>> KeyRoles { get; set; } = [];

    /// <summary>
    /// JWT token.
    /// </summary>
    public string JwtToken { get; set; } = string.Empty;

    /// <summary>
    /// A list of Db seed functation that are executed after database has been migrated.
    /// </summary>
    public List<Func<Task>> DbSeeds { get; } = [];
}