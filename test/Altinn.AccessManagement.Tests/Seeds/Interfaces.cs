using System;
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