#nullable enable

using Altinn.Authorization.ProblemDetails;

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
        = _factory.Create(4, "Invalid party URN.");
}
