-- Function: UPDATE delegation.select_active_resourceregistrydelegationchanges_coveredbypartys with optional OfferedBy filtering
CREATE OR REPLACE FUNCTION delegation.select_active_resourceregistrydelegationchanges_coveredbypartys(
	_coveredByPartyIds int[],
	_offeredByPartyIds int[] = null,
	_resourceRegistryIds text[] = null,
	_resourceTypes text[] = null)
    RETURNS TABLE (
		resourceRegistryDelegationChangeId int,
		delegationChangeType delegation.delegationchangetype,
		resourceRegistryId text,
		resourceType text,
		offeredByPartyId int,
		coveredByUserId int,
		coveredByPartyId int,
		performedByUserId int,
		performedByPartyId int,
		blobStoragePolicyPath text,
		blobStorageVersionId text,
		created timestamp with time zone)
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1000
AS $BODY$
	WITH res AS (
		SELECT
		  resourceId,
		  resourceRegistryId,
		  resourceType
		FROM accessmanagement.Resource
		WHERE (_resourceRegistryIds IS NULL OR resourceRegistryId = ANY (_resourceRegistryIds))
		AND (_resourceTypes IS NULL OR resourceType = ANY (_resourceTypes))
	)
	SELECT
		rrDelegationChange.resourceRegistryDelegationChangeId,
		rrDelegationChange.delegationChangeType,
		res.resourceRegistryId,
		res.resourceType,
		rrDelegationChange.offeredByPartyId,
		rrDelegationChange.coveredByUserId,
		rrDelegationChange.coveredByPartyId,
		rrDelegationChange.performedByUserId,
		rrDelegationChange.performedByPartyId,
		rrDelegationChange.blobStoragePolicyPath,
		rrDelegationChange.blobStorageVersionId,	
		rrDelegationChange.created
	FROM delegation.ResourceRegistryDelegationChanges AS rrDelegationChange
		INNER JOIN res ON rrDelegationChange.resourceId_fk = res.resourceid
		INNER JOIN
		(
			SELECT MAX(resourceRegistryDelegationChangeId) AS maxChange
			FROM delegation.ResourceRegistryDelegationChanges AS rrdc
				INNER JOIN res ON rrdc.resourceId_fk = res.resourceid
			WHERE
				(_offeredByPartyIds IS NULL OR offeredByPartyId = ANY (_offeredByPartyIds))
				AND coveredByPartyId = ANY (_coveredbypartyids)
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$BODY$;
