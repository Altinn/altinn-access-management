using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the interface for the Policy Administration Point
    /// </summary>
    public interface IPolicyAdministrationPoint
    {
        /// <summary>
        /// Returns a bool based on writing file to storage was successful
        /// </summary>
        /// <param name="org">Unique identifier of the organisation responsible for the app.</param>
        /// <param name="app">Application identifier which is unique within an organisation.</param>
        /// <param name="fileStream">A stream containing the content of the policy file</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        Task<bool> WritePolicyAsync(string org, string app, Stream fileStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Trys to sort and store the set of rules as delegation policy files in blob storage.
        /// </summary>
        /// <param name="rules">The set of rules to be delegated</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>The list of rules with created Id and result status</returns>
        Task<List<Rule>> TryWriteDelegationPolicyRules(List<Rule> rules, CancellationToken cancellationToken = default);

        /// <summary>
        /// Trys to sort and store the set of rules as delegation policy files in blob storage.
        /// </summary>
        /// <param name="rules">The set of instance rules to be delegated</param>
        /// <param name="cancellationToken">CancellationToke</param>
        /// <returns>The list of instance rules with created Id and result status</returns>
        Task<InstanceRight> TryWriteInstanceDelegationPolicyRules(InstanceRight rules, CancellationToken cancellationToken = default);

        /// <summary>
        /// Trys to sort and revoke the set of rules as delegation policy files in blob storage.
        /// </summary>
        /// <param name="rules">The set of instance rules to be revoked</param>
        /// <param name="cancellationToken">CancellationToke</param>
        /// <returns>The list of instance rules with created Id and result status</returns>
        Task<InstanceRight> TryWriteInstanceRevokePolicyRules(InstanceRight rules, CancellationToken cancellationToken = default);

        /// <summary>
        /// Trys to sort and revoke the set of rules as delegation policy files in blob storage.
        /// </summary>
        /// <param name="rights">The set of instance rules to be revoked</param>
        /// <param name="cancellationToken">CancellationToke</param>
        /// <returns>The list of instance rules with created Id and result status</returns>
        Task<List<InstanceRight>> TryWriteInstanceRevokeAllPolicyRules(List<InstanceRight> rights, CancellationToken cancellationToken = default);

        /// <summary>
        /// Trys to sort and delete the set of rules matching the list of ruleMatches to delete from delegation policy files in blob storage.
        /// </summary>
        /// <param name="rulesToDelete">Entity to define which rules to be deleted</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>The list of rules with created Id and result status</returns>
        Task<List<Rule>> TryDeleteDelegationPolicyRules(List<RequestToDelete> rulesToDelete, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a List of policies based on input list of matches to remove
        /// </summary>
        /// <param name="policiesToDelete">entity containing match for all the policies to delete</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>A list containing all the policies that is deleted</returns>
        Task<List<Rule>> TryDeleteDelegationPolicies(List<RequestToDelete> policiesToDelete, CancellationToken cancellationToken = default);
    }
}
