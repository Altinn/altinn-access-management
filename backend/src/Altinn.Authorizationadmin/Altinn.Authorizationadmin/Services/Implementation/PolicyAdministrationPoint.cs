﻿using System.Net;
using System.Text.Json;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.AuthorizationAdmin.Core;
using Altinn.AuthorizationAdmin.Core.Enums;
using Altinn.AuthorizationAdmin.Core.Helpers;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;
using Altinn.AuthorizationAdmin.Core.Repositories.Interface;
using Altinn.AuthorizationAdmin.Core.Services.Interface;
using Altinn.AuthorizationAdmin.Services.Interface;
using Azure;
using Azure.Storage.Blobs.Models;

namespace Altinn.AuthorizationAdmin.Services.Implementation
{
    /// <summary>
    /// The Policy Administration Point responsible for storing and modifying delegation policies
    /// </summary>
    public class PolicyAdministrationPoint : IPolicyAdministrationPoint
    {
        private readonly ILogger<IPolicyAdministrationPoint> _logger;
        private readonly IPolicyRetrievalPoint _prp;
        private readonly IPolicyRepository _policyRepository;
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly IDelegationChangeEventQueue _eventQueue;
        private readonly int delegationChangeEventQueueErrorId = 911;
        private readonly IResourceRegistryClient _resourceRegistryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyAdministrationPoint"/> class.
        /// </summary>
        /// <param name="policyRetrievalPoint">The policy retrieval point.</param>
        /// <param name="policyRepository">The policy repository (blob storage).</param>
        /// <param name="delegationRepository">The delegation change repository (postgresql).</param>
        /// <param name="eventQueue">The delegation change event queue service to post events for any delegation change.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="resourceRegistryClient">Client for accessing the resourceregistry.</param>
        public PolicyAdministrationPoint(IPolicyRetrievalPoint policyRetrievalPoint, IPolicyRepository policyRepository, IDelegationMetadataRepository delegationRepository, IDelegationChangeEventQueue eventQueue, ILogger<IPolicyAdministrationPoint> logger, IResourceRegistryClient resourceRegistryClient)
        {
            _prp = policyRetrievalPoint;
            _policyRepository = policyRepository;
            _delegationRepository = delegationRepository;
            _eventQueue = eventQueue;
            _logger = logger;
            _resourceRegistryClient = resourceRegistryClient;
        }

        /// <inheritdoc/>
        public async Task<bool> WritePolicyAsync(string org, string app, Stream fileStream)
        {
            if (fileStream == null)
            {
                throw new ArgumentException("The policy file can not be null");
            }

            string filePath = PolicyHelper.GetAltinnAppsPolicyPath(org, app);
            Response<BlobContentInfo> response = await _policyRepository.WritePolicyAsync(filePath, fileStream);

            return response?.GetRawResponse()?.Status == (int)HttpStatusCode.Created;
        }

        /// <inheritdoc/>
        public async Task<List<Rule>> TryWriteDelegationPolicyRules(List<Rule> rules)
        {
            List<Rule> result = new List<Rule>();
            Dictionary<string, List<Rule>> delegationDict = DelegationHelper.SortRulesByDelegationPolicyPath(rules, out List<Rule> unsortables);

            foreach (string delegationPolicypath in delegationDict.Keys)
            {
                bool writePolicySuccess = false;

                try
                {
                    writePolicySuccess = await WriteDelegationPolicyInternal(delegationPolicypath, delegationDict[delegationPolicypath]);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An exception occured while processing authorization rules for delegation on delegation policy path: {delegationPolicypath}", delegationPolicypath);
                }

                foreach (Rule rule in delegationDict[delegationPolicypath])
                {
                    if (writePolicySuccess)
                    {
                        rule.CreatedSuccessfully = true;
                        rule.Type = RuleType.DirectlyDelegated;
                    }
                    else
                    {
                        rule.RuleId = string.Empty;
                    }

                    result.Add(rule);
                }
            }

            if (unsortables.Count > 0)
            {
                string unsortablesJson = JsonSerializer.Serialize(unsortables);
                _logger.LogError("One or more rules could not be processed because of incomplete input:\n{unsortablesJson}", unsortablesJson);
                result.AddRange(unsortables);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<List<Rule>> TryDeleteDelegationPolicyRules(List<RequestToDelete> rulesToDelete)
        {
            List<Rule> result = new List<Rule>();

            foreach (RequestToDelete deleteRequest in rulesToDelete)
            {
                List<Rule> currentRules = await DeleteRulesInPolicy(deleteRequest);
                if (currentRules != null)
                {
                    result.AddRange(currentRules);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<List<Rule>> TryDeleteDelegationPolicies(List<RequestToDelete> policiesToDelete)
        {
            List<Rule> result = new List<Rule>();

            foreach (RequestToDelete policyToDelete in policiesToDelete)
            {
                List<Rule> currentRules = await DeleteAllRulesInPolicy(policyToDelete);
                if (currentRules != null)
                {
                    result.AddRange(currentRules);
                }
            }

            return result;
        }

        private async Task<bool> WriteDelegationPolicyInternal(string policyPath, List<Rule> rules)
        {
            string altinnAppId = null;
            ServiceResource resource = null;

            if (!DelegationHelper.TryGetDelegationParamsFromRule(rules.First(), out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app, out int offeredByPartyId, out int? coveredByPartyId, out int? coveredByUserId, out int delegatedByUserId)
                || resourceMatchType == ResourceAttributeMatchType.None)
            {
                _logger.LogWarning("This should not happen. Incomplete rule model received for delegation to delegation policy at: {policyPath}. Incomplete model should have been returned in unsortable rule set by TryWriteDelegationPolicyRules. DelegationHelper.SortRulesByDelegationPolicyPath might be broken.", policyPath);
                return false;
            }

            if (resourceMatchType == ResourceAttributeMatchType.ResourceRegistry)
            {
                XacmlPolicy resourcePolicy = await _prp.GetPolicyAsync(resourceRegistryId);
                if (resourcePolicy == null)
                {
                    _logger.LogWarning("No valid resource policy found for delegation policy path: {policyPath}", policyPath);
                    return false;
                }

                resource = await _resourceRegistryClient.GetResource(resourceRegistryId);
                if (resource == null)
                {
                    _logger.LogWarning("The specified resource {resourceRegistryId} does not exist.", resourceRegistryId);
                    return false;
                }

                foreach (Rule rule in rules)
                {
                    if (!DelegationHelper.PolicyContainsMatchingRule(resourcePolicy, rule))
                    {
                        _logger.LogWarning("Matching rule not found in resource policy. Action might not exist for Resource, or Resource itself might not exist. Delegation policy path: {policyPath}. Rule: {rule}", policyPath, rule);
                        return false;
                    }
                }
            }
            else if (resourceMatchType == ResourceAttributeMatchType.AltinnApp)
            {
                XacmlPolicy appPolicy = await _prp.GetPolicyAsync(org, app);
                if (appPolicy == null)
                {
                    _logger.LogWarning("No valid App policy found for delegation policy path: {policyPath}", policyPath);
                    return false;
                }

                foreach (Rule rule in rules)
                {
                    if (!DelegationHelper.PolicyContainsMatchingRule(appPolicy, rule))
                    {
                        _logger.LogWarning("Matching rule not found in app policy. Action might not exist for Resource, or Resource itself might not exist. Delegation policy path: {policyPath}. Rule: {rule}", policyPath, rule);
                        return false;
                    }
                }
            }

            if (!await _policyRepository.PolicyExistsAsync(policyPath))
            {
                // Create a new empty blob for lease locking
                await _policyRepository.WritePolicyAsync(policyPath, new MemoryStream());
            }

            string leaseId = await _policyRepository.TryAcquireBlobLease(policyPath);
            if (leaseId != null)
            {
                try
                {
                    // Check for a current delegation change from postgresql
                    DelegationChange currentChange = new DelegationChange();

                    if (!string.IsNullOrWhiteSpace(org) || !string.IsNullOrWhiteSpace(app))
                    {
                        altinnAppId = $"{org}/{app}";
                    }

                    currentChange = await _delegationRepository.GetCurrentDelegationChange(altinnAppId, resourceRegistryId, offeredByPartyId, coveredByPartyId, coveredByUserId);

                    XacmlPolicy existingDelegationPolicy = null;
                    if (currentChange != null && currentChange.DelegationChangeType != DelegationChangeType.RevokeLast)
                    {
                        existingDelegationPolicy = await _prp.GetPolicyVersionAsync(policyPath, currentChange.BlobStorageVersionId);
                    }

                    // Build delegation XacmlPolicy either as a new policy or add rules to existing
                    XacmlPolicy delegationPolicy;
                    if (existingDelegationPolicy != null)
                    {
                        delegationPolicy = existingDelegationPolicy;
                        foreach (Rule rule in rules)
                        {
                            if (!DelegationHelper.PolicyContainsMatchingRule(delegationPolicy, rule))
                            {
                                delegationPolicy.Rules.Add(PolicyHelper.BuildDelegationRule(org, app, resourceRegistryId, offeredByPartyId, coveredByPartyId, coveredByUserId, rule));
                            }
                        }
                    }
                    else
                    {
                        delegationPolicy = PolicyHelper.BuildDelegationPolicy(org, app, resourceRegistryId, offeredByPartyId, coveredByPartyId, coveredByUserId, rules);
                    }

                    // Write delegation policy to blob storage
                    MemoryStream dataStream = PolicyHelper.GetXmlMemoryStreamFromXacmlPolicy(delegationPolicy);
                    Response<BlobContentInfo> blobResponse = await _policyRepository.WritePolicyConditionallyAsync(policyPath, dataStream, leaseId);
                    Response httpResponse = blobResponse.GetRawResponse();
                    if (httpResponse.Status != (int)HttpStatusCode.Created)
                    {
                        _logger.LogError("Writing of delegation policy at path: {policyPath} failed. Response Status Code:\n{httpResponse.Status}. Response Reason Phrase:\n{httpResponse.ReasonPhrase}", policyPath, httpResponse.Status, httpResponse.ReasonPhrase);
                        return false;
                    }

                    // Write delegation change to postgresql
                    DelegationChange change = new DelegationChange
                    {
                        DelegationChangeType = DelegationChangeType.Grant,
                        AltinnAppId = altinnAppId,
                        OfferedByPartyId = offeredByPartyId,
                        CoveredByPartyId = coveredByPartyId,
                        CoveredByUserId = coveredByUserId,
                        PerformedByUserId = delegatedByUserId,
                        BlobStoragePolicyPath = policyPath,
                        BlobStorageVersionId = blobResponse.Value.VersionId,
                        ResourceId = resourceRegistryId,
                        ResourceType = resource != null ? resource.ResourceType.ToString() : null
                    };

                    change = await _delegationRepository.InsertDelegation(change);
                    if (change == null || change.DelegationChangeId <= 0)
                    {
                        // Comment:
                        // This means that the current version of the root blob is no longer in sync with changes in authorization postgresql delegation.delegatedpolicy table.
                        // The root blob is in effect orphaned/ignored as the delegation policies are always to be read by version, and will be overritten by the next delegation change.
                        _logger.LogError("Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: {policyPath}", policyPath);
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(change.AltinnAppId))
                    {
                        try
                        {
                            await _eventQueue.Push(change);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical(new EventId(delegationChangeEventQueueErrorId, "DelegationChangeEventQueue.Push.Error"), ex, "AddRules could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange: {change}", change);
                        }
                    }

                    return true;
                }
                finally
                {
                    _policyRepository.ReleaseBlobLease(policyPath, leaseId);
                }
            }

            _logger.LogInformation("Could not acquire blob lease lock on delegation policy at path: {policyPath}", policyPath);
            return false;
        }

        private async Task<List<Rule>> ProcessPolicyFile(string policyPath, string org, string app, RequestToDelete deleteRequest, string resourceregistryId)
        {
            ServiceResource resource = null;
            string altinnAppId = null;
            List<Rule> currentRules = new List<Rule>();
            string leaseId = await _policyRepository.TryAcquireBlobLease(policyPath);
            if (leaseId == null)
            {
                _logger.LogError("Could not acquire blob lease lock on delegation policy at path: {policyPath}", policyPath);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(org) || !string.IsNullOrWhiteSpace(app))
            {
                altinnAppId = $"{org}/{app}";
            }

            resource = await _resourceRegistryClient.GetResource(resourceregistryId);
            if (resource == null)
            {
                _logger.LogWarning("The specified resource {resourceRegistryId} does not exist.", resourceregistryId);
            }

            try
            {
                bool isAllRulesDeleted = false;
                string coveredBy = DelegationHelper.GetCoveredByFromMatch(deleteRequest.PolicyMatch.CoveredBy, out int? coveredByUserId, out int? coveredByPartyId);
                string offeredBy = deleteRequest.PolicyMatch.OfferedByPartyId.ToString();
                DelegationChange currentChange = await _delegationRepository.GetCurrentDelegationChange(altinnAppId, resourceregistryId, deleteRequest.PolicyMatch.OfferedByPartyId, coveredByPartyId, coveredByUserId);

                XacmlPolicy existingDelegationPolicy = null;
                if (currentChange.DelegationChangeType == DelegationChangeType.RevokeLast)
                {
                    _logger.LogWarning("The policy is already deleted for App: {org}/{app} CoveredBy: {coveredBy} OfferedBy: {offeredBy}", org, app, coveredBy, offeredBy);
                    return null;
                }

                existingDelegationPolicy = await _prp.GetPolicyVersionAsync(currentChange.BlobStoragePolicyPath, currentChange.BlobStorageVersionId);

                foreach (string ruleId in deleteRequest.RuleIds)
                {
                    XacmlRule xacmlRuleToRemove = existingDelegationPolicy.Rules.FirstOrDefault(r => r.RuleId == ruleId);
                    if (xacmlRuleToRemove == null)
                    {
                        _logger.LogWarning("The rule with id: {ruleId} does not exist in policy with path: {policyPath}", ruleId, policyPath);
                        continue;
                    }

                    existingDelegationPolicy.Rules.Remove(xacmlRuleToRemove);
                    Rule currentRule = PolicyHelper.CreateRuleFromPolicyAndRuleMatch(deleteRequest, xacmlRuleToRemove);
                    currentRules.Add(currentRule);
                }

                isAllRulesDeleted = existingDelegationPolicy.Rules.Count == 0;

                // if nothing is deleted no update has been done and policy and postgree update can be skipped
                if (currentRules.Count > 0)
                {
                    Response<BlobContentInfo> response;
                    try
                    {
                        // Write delegation policy to blob storage
                        MemoryStream dataStream = PolicyHelper.GetXmlMemoryStreamFromXacmlPolicy(existingDelegationPolicy);
                        response = await _policyRepository.WritePolicyConditionallyAsync(policyPath, dataStream, leaseId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Writing of delegation policy at path: {policyPath} failed. Is delegation blob storage account alive and well?", policyPath);
                        return null;
                    }

                    // Write delegation change to postgresql
                    DelegationChange change = new DelegationChange
                    {
                        DelegationChangeType = isAllRulesDeleted ? DelegationChangeType.RevokeLast : DelegationChangeType.Revoke,
                        AltinnAppId = altinnAppId,
                        OfferedByPartyId = deleteRequest.PolicyMatch.OfferedByPartyId,
                        CoveredByPartyId = coveredByPartyId,
                        CoveredByUserId = coveredByUserId,
                        PerformedByUserId = deleteRequest.DeletedByUserId,
                        BlobStoragePolicyPath = policyPath,
                        BlobStorageVersionId = response.Value.VersionId,
                        ResourceId = resourceregistryId,
                        ResourceType = resource != null ? resource.ResourceType.ToString() : null
                    };

                    change = await _delegationRepository.InsertDelegation(change);
                    if (change == null || change.DelegationChangeId <= 0)
                    {
                        // Comment:
                        // This means that the current version of the root blob is no longer in sync with changes in authorization postgresql delegation.delegatedpolicy table.
                        // The root blob is in effect orphaned/ignored as the delegation policies are always to be read by version, and will be overritten by the next delegation change.
                        _logger.LogError("Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: {policyPath}. is authorization postgresql database alive and well?", policyPath);
                        return null;
                    }

                    if (!string.IsNullOrWhiteSpace(change.AltinnAppId))
                    {
                        try
                        {
                            await _eventQueue.Push(change);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical(new EventId(delegationChangeEventQueueErrorId, "DelegationChangeEventQueue.Push.Error"), ex, "DeleteRules could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange: {change}", change);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured while processing rules to delete in policy: {policyPath}", policyPath);
                return null;
            }
            finally
            {
                _policyRepository.ReleaseBlobLease(policyPath, leaseId);
            }

            return currentRules;
        }

        private async Task<List<Rule>> DeleteAllRulesInPolicy(RequestToDelete policyToDelete)
        {
            string altinnAppId = null;
            ServiceResource resource = null;
            DelegationHelper.TryGetResourceFromAttributeMatch(policyToDelete.PolicyMatch.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app);
            string coveredBy = DelegationHelper.GetCoveredByFromMatch(policyToDelete.PolicyMatch.CoveredBy, out int? coveredByUserId, out int? coveredByPartyId);

            string policyPath;
            try
            {
                policyPath = PolicyHelper.GetDelegationPolicyPath(resourceMatchType, resourceRegistryId, org, app, policyToDelete.PolicyMatch.OfferedByPartyId.ToString(), coveredByUserId, coveredByPartyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Not possible to build policy path App: {org}/{app} CoveredBy: {coveredBy} OfferedBy: {policyToDelete.PolicyMatch.OfferedByPartyId}", org, app, coveredBy, policyToDelete.PolicyMatch.OfferedByPartyId);
                return null;
            }

            if (!await _policyRepository.PolicyExistsAsync(policyPath))
            {
                _logger.LogWarning("No blob was found for the expected path: {policyPath} this must be removed without upading the database", policyPath);
                return null;
            }

            string leaseId = await _policyRepository.TryAcquireBlobLease(policyPath);
            if (leaseId == null)
            {
                _logger.LogError("Could not acquire blob lease on delegation policy at path: {policyPath}", policyPath);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(org) || !string.IsNullOrWhiteSpace(app))
            {
                altinnAppId = $"{org}/{app}";
            }

            resource = await _resourceRegistryClient.GetResource(resourceRegistryId);
            if (resource == null)
            {
                _logger.LogWarning("The specified resource {resourceRegistryId} does not exist.", resourceRegistryId);
            }

            try
            {
                DelegationChange currentChange = await _delegationRepository.GetCurrentDelegationChange($"{org}/{app}", null, policyToDelete.PolicyMatch.OfferedByPartyId, coveredByPartyId, coveredByUserId);

                if (currentChange.DelegationChangeType == DelegationChangeType.RevokeLast)
                {
                    _logger.LogWarning("The policy is already deleted for App: {org}/{app} CoveredBy: {coveredBy} OfferedBy: {policyToDelete.PolicyMatch.OfferedByPartyId}", org, app, coveredBy, policyToDelete.PolicyMatch.OfferedByPartyId);
                    return null;
                }

                XacmlPolicy existingDelegationPolicy = await _prp.GetPolicyVersionAsync(currentChange.BlobStoragePolicyPath, currentChange.BlobStorageVersionId);
                List<Rule> currentPolicyRules = new List<Rule>();
                foreach (XacmlRule xacmlRule in existingDelegationPolicy.Rules)
                {
                    currentPolicyRules.Add(PolicyHelper.CreateRuleFromPolicyAndRuleMatch(policyToDelete, xacmlRule));
                }

                existingDelegationPolicy.Rules.Clear();

                Response<BlobContentInfo> response;
                try
                {
                    MemoryStream dataStream = PolicyHelper.GetXmlMemoryStreamFromXacmlPolicy(existingDelegationPolicy);
                    response = await _policyRepository.WritePolicyConditionallyAsync(policyPath, dataStream, leaseId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Writing of delegation policy at path: {policyPath} failed. Is delegation blob storage account alive and well?}", policyPath);
                    return null;
                }

                DelegationChange change = new DelegationChange
                {
                    DelegationChangeType = DelegationChangeType.RevokeLast,
                    AltinnAppId = altinnAppId,
                    OfferedByPartyId = policyToDelete.PolicyMatch.OfferedByPartyId,
                    CoveredByPartyId = coveredByPartyId,
                    CoveredByUserId = coveredByUserId,
                    PerformedByUserId = policyToDelete.DeletedByUserId,
                    BlobStoragePolicyPath = policyPath,
                    BlobStorageVersionId = response.Value.VersionId,
                    ResourceId = resourceRegistryId,
                    ResourceType = resource != null ? resource.ResourceType.ToString() : null
                };

                change = await _delegationRepository.InsertDelegation(change);
                if (change == null || change.DelegationChangeId <= 0)
                {
                    // Comment:
                    // This means that the current version of the root blob is no longer in sync with changes in authorization postgresql delegation.delegatedpolicy table.
                    // The root blob is in effect orphaned/ignored as the delegation policies are always to be read by version, and will be overritten by the next delegation change.
                    _logger.LogError("Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: {policyPath}. is authorization postgresql database alive and well?", policyPath);
                    return null;
                }

                try
                {
                    await _eventQueue.Push(change);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(new EventId(delegationChangeEventQueueErrorId, "DelegationChangeEventQueue.Push.Error"), ex, "DeletePolicy could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange: {change}", change);
                }

                return currentPolicyRules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured while processing rules to delete in policy: {policyPath}", policyPath);
                return null;
            }
            finally
            {
                _policyRepository.ReleaseBlobLease(policyPath, leaseId);
            }
        }

        private async Task<List<Rule>> DeleteRulesInPolicy(RequestToDelete rulesToDelete)
        {
            string coveredBy = DelegationHelper.GetCoveredByFromMatch(rulesToDelete.PolicyMatch.CoveredBy, out int? coveredByUserId, out int? coveredByPartyId);

            DelegationHelper.TryGetResourceFromAttributeMatch(rulesToDelete.PolicyMatch.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app);

            //if (resourceMatchType != ResourceAttributeMatchType.AltinnApp)
            //{
            //    throw new NotImplementedException("Deletion of rules for this resource type is not implemented");
            //}

            string policyPath;
            try
            {
                policyPath = PolicyHelper.GetDelegationPolicyPath(resourceMatchType, resourceRegistryId, org, app, rulesToDelete.PolicyMatch.OfferedByPartyId.ToString(), coveredByUserId, coveredByPartyId);
            }
            catch (Exception ex)
            {
                string rulesToDeleteString = string.Join(", ", rulesToDelete.RuleIds);
                _logger.LogError(ex, "Not possible to build policy path App: {org}/{app} CoveredBy: {coveredBy} OfferedBy: {policyToDelete.PolicyMatch.OfferedByPartyId} RuleIds: {rulesToDeleteString}", org, app, coveredBy, rulesToDelete.PolicyMatch.OfferedByPartyId, rulesToDeleteString);
                return null;
            }

            if (!await _policyRepository.PolicyExistsAsync(policyPath))
            {
                _logger.LogWarning("No blob was found for the expected path: {policyPath} this must be removed without updating the database", policyPath);
                return null;
            }

            List<Rule> currentRules = await ProcessPolicyFile(policyPath, org, app, rulesToDelete, resourceRegistryId);

            return currentRules;
        }
    }
}
