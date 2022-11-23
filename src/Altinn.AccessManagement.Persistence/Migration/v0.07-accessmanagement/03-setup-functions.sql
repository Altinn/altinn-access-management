-- Function: CREATE new delegation.insert_resourceregistrydelegationchange
CREATE OR REPLACE FUNCTION delegation.insert_resourceregistrydelegationchange(
	_delegationchangetype delegation.delegationchangetype,
	_resourceregistryid text,
	_offeredbypartyid int,
	_coveredbyuserid int,
	_coveredbypartyid int,
	_performedbyuserid int,
	_performedbypartyid int,
	_blobstoragepolicypath text,
	_blobstorageversionid text,	
	_delegatedTime timestamp with time zone DEFAULT CURRENT_TIMESTAMP)
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
    VOLATILE PARALLEL UNSAFE
    ROWS 1
AS $BODY$
  WITH res AS (
		SELECT
		  resourceId,
		  resourceRegistryId,
		  resourceType
		FROM accessmanagement.Resource
		WHERE resourceRegistryId = _resourceregistryid
	),
	insertedDelegation AS (
	INSERT INTO delegation.ResourceRegistryDelegationChanges(
		delegationChangeType,
		resourceId_fk,
		offeredByPartyId,
		coveredByUserId,
		coveredByPartyId,
		performedByUserId,
		performedByPartyId,
		blobStoragePolicyPath,
		blobStorageVersionId,	
		created
	  )
	  SELECT _delegationChangeType,
		res.resourceId,
		_offeredByPartyId,
		_coveredByUserId,
		_coveredByPartyId,
		_performedByUserId,
		_performedbypartyid,
		_blobStoragePolicyPath,
		_blobStorageVersionId,
		_delegatedTime
	  FROM res
	  RETURNING 
	  	resourceRegistryDelegationChangeId,
		delegationChangeType,
		resourceId_fk,
		offeredByPartyId,
		coveredByUserId,
		coveredByPartyId,
		performedByUserId,
		performedByPartyId,
		blobStoragePolicyPath,
		blobStorageVersionId,	
		created
  )
  SELECT
  	ins.resourceRegistryDelegationChangeId,
	ins.delegationChangeType,
  	res.resourceRegistryId,
	res.resourceType,
	ins.offeredByPartyId,
	ins.coveredByUserId,
	ins.coveredByPartyId,
	ins.performedByUserId,
	ins.performedByPartyId,
	ins.blobStoragePolicyPath,
	ins.blobStorageVersionId,	
	ins.created
  FROM insertedDelegation AS ins
  JOIN res ON ins.resourceId_fk = res.resourceid;
$BODY$;

-- Function: CREATE delegation.select_current_resourceregistrydelegationchange
CREATE OR REPLACE FUNCTION delegation.select_current_resourceregistrydelegationchange(
	_resourceRegistryId text,
	_offeredbypartyid int,
	_coveredbyuserid int,
	_coveredbypartyid int)
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
    ROWS 1
AS $BODY$
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
	JOIN accessmanagement.Resource AS res ON rrDelegationChange.resourceId_fk = res.resourceid
  WHERE
    res.resourceRegistryId = _resourceRegistryId
    AND offeredByPartyId = _offeredByPartyId
    AND (_coveredByUserId IS NULL OR coveredByUserId = _coveredByUserId)
    AND (_coveredByPartyId IS NULL OR coveredByPartyId = _coveredByPartyId)
  ORDER BY resourceRegistryDelegationChangeId DESC LIMIT 1
$BODY$;

-- Function: CREATE delegation.select_active_resourceregistrydelegationchanges_offeredby
CREATE OR REPLACE FUNCTION delegation.select_active_resourceregistrydelegationchanges_offeredby(
		_offeredByPartyId int,
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
				offeredByPartyId = _offeredByPartyId
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$BODY$;

-- Function: CREATE delegation.select_active_resourceregistrydelegationchanges_coveredbyuser
CREATE OR REPLACE FUNCTION delegation.select_active_resourceregistrydelegationchanges_coveredbyuser(
		_coveredByUserId int,
		_offeredByPartyIds int[] = null,
		_resourceRegistryIds text[] = null,
		_resourceTypes text[]  = null)
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
				coveredByUserId = _coveredbyuserid
				AND (_offeredByPartyIds IS NULL OR offeredByPartyId = ANY (_offeredByPartyIds))
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$BODY$;

-- Function: CREATE delegation.select_active_resourceregistrydelegationchanges_coveredbypartys
CREATE OR REPLACE FUNCTION delegation.select_active_resourceregistrydelegationchanges_coveredbypartys(
	_coveredByPartyIds int[],
	_offeredByPartyIds int[],
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
				offeredByPartyId = ANY (_offeredByPartyIds)
				AND coveredByPartyId = ANY (_coveredbypartyids)
			GROUP BY resourceId_fk, offeredByPartyId, coveredByPartyId, coveredByUserId
		) AS selectMaxChange
	ON resourceRegistryDelegationChangeId = selectMaxChange.maxChange
	WHERE delegationchangetype != 'revoke_last'
$BODY$;
