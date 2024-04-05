#pragma warning disable SA1600

using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Tests.Seeds;

public interface IParty
{
    Party Party { get; }
}

public interface IUserProfile
{
    UserProfile UserProfile { get; }
}

public interface IToken
{
    string Token { get; }
}

public interface IAccessManagementResource
{
    ServiceResource Resource { get; }
}