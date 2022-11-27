-- FUNCTION: CREATE new delegation.select_active_resourceregistrydelegationchanges_admin
CREATE OR REPLACE FUNCTION delegation.select_active_resourceregistrydelegationchanges_admin(
	_offeredbypartyid integer,
	_coveredbypartyid integer,
	_resourceregistryids text[] DEFAULT NULL::text[],
	_resourcetypes text[] DEFAULT NULL::text[])
    RETURNS TABLE(resourceregistrydelegationchangeid integer, delegationchangetype delegation.delegationchangetype, resourceregistryid text, resourcetype text, offeredbypartyid integer, coveredbyuserid integer, coveredbypartyid integer, performedbyuserid integer, performedbypartyid integer, blobstoragepolicypath text, blobstorageversionid text, created timestamp with time zone) 
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
				offeredByPartyId = _offeredByPartyId
			AND 
				coveredByPartyId = _coveredByPartyId
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$BODY$;
