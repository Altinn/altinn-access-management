using Altinn.Authorization.ABAC.Xacml;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the interface for the Policy Retrival Point
    /// </summary>
    public interface IPolicyRetrievalPoint
    {
        /// <summary>
        /// Returns a policy based on the context request
        /// </summary>
        /// <param name="request">The context request</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>XacmlPolicy</returns>
        Task<XacmlPolicy> GetPolicyAsync(XacmlContextRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a policy based the org and app
        /// </summary>
        /// <param name="org">The organisation</param>
        /// <param name="app">The app</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>XacmlPolicy</returns>
        Task<XacmlPolicy> GetPolicyAsync(string org, string app, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a policy based the resourceRegistryId
        /// </summary>
        /// <param name="resourceRegistry">The Resource Registry Id</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>XacmlPolicy</returns>
        Task<XacmlPolicy> GetPolicyAsync(string resourceRegistry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a specific version of a policy if it exits on the provided path
        /// </summary>
        /// <param name="policyPath">The blobstorage path to the policy file</param>
        /// <param name="version">The specific blob storage version to get</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>XacmlPolicy and ETag tuple</returns>
        Task<XacmlPolicy> GetPolicyVersionAsync(string policyPath, string version, CancellationToken cancellationToken = default);
    }
}
