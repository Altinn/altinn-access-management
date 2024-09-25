#nullable enable

using Altinn.Authorization.ProblemDetails;
using System.Net;

namespace Altinn.AccessManagement.Core.Errors;

/// <summary>
/// Validation errors for the Access Management.
/// </summary>
public static class ValidationErrors
{
    private static readonly ValidationErrorDescriptorFactory _factory
        = ValidationErrorDescriptorFactory.New("AM");

    /// <summary>
    /// Gets a validation error descriptor for when an invalid party URN is provided.
    /// </summary>
    public static ValidationErrorDescriptor InvalidPartyUrn { get; }
        = _factory.Create(1, "Invalid party URN.");

    public static ValidationErrorDescriptor InvalidResource { get; }
        = _factory.Create(2, $"Resource must be valid.");

    public static ValidationErrorDescriptor MissingPolicy { get; }
        = _factory.Create(3, $"Resource must have policy.");

    public static ValidationErrorDescriptor MissingDelegableRights { get; }
        = _factory.Create(4, $"Resource must have policy including delegable rights.");
}