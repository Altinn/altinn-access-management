--Procedure: delegation.get_current_change_based_on_resourceregistryid
CREATE OR REPLACE FUNCTION delegation.get_current_change_based_on_resourceregistryid(
	_resourceregistryid character varying,
	_offeredbypartyid integer,
	_coveredbyuserid integer,
	_coveredbypartyid integer)
    RETURNS SETOF delegation.delegationchanges 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1

AS $BODY$
SELECT
    delegationChangeId,
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByUserId,
    coveredByPartyId,
    performedByUserId,
	blobStoragePolicyPath,
    blobStorageVersionId,
    created,
	resourceid,
    resourcetype,
	performedByPartyId
  FROM delegation.delegationChanges
  WHERE
    resourceid = _resourceregistryid
    AND offeredByPartyId = _offeredByPartyId
    AND (_coveredByUserId IS NULL OR coveredByUserId = _coveredByUserId)
    AND (_coveredByPartyId IS NULL OR coveredByPartyId = _coveredByPartyId)
  ORDER BY delegationChangeId DESC LIMIT 1
$BODY$;

--Procedure: delegation.get_current_change
CREATE OR REPLACE FUNCTION delegation.get_current_change(
	_altinnappid character varying,
	_offeredbypartyid integer,
	_coveredbyuserid integer,
	_coveredbypartyid integer)
    RETURNS SETOF delegation.delegationchanges 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1

AS $BODY$
SELECT
    delegationChangeId,
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByUserId,
    coveredByPartyId,
    performedByUserId,
	blobStoragePolicyPath,
    blobStorageVersionId,
    created,
	resourceid,
	resourcetype,
	performedByPartyId
  FROM delegation.delegationChanges
  WHERE
    altinnAppId = _altinnAppId
    AND offeredByPartyId = _offeredByPartyId
    AND (_coveredByUserId IS NULL OR coveredByUserId = _coveredByUserId)
    AND (_coveredByPartyId IS NULL OR coveredByPartyId = _coveredByPartyId)
  ORDER BY delegationChangeId DESC LIMIT 1
$BODY$;

--Procedure: delegation.InsertResourceDelegation
CREATE OR REPLACE FUNCTION delegation.insert_delegationchange(
	_delegationchangetype delegation.delegationchangetype,
	_altinnappid character varying,
	_offeredbypartyid integer,
	_coveredbyuserid integer,
	_coveredbypartyid integer,
	_performedbyuserid integer,
	_performedByPartyId integer,
	_performedDateTime timestamp with time zone,
	_blobstoragepolicypath character varying,
	_blobstorageversionid character varying,
	_resourceid character varying,
	_resourcetype character varying)
    RETURNS SETOF delegation.delegationchanges 
    LANGUAGE 'sql'
    VOLATILE
    ROWS 1

AS $BODY$
INSERT INTO delegation.delegationChanges(
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByUserId,
    coveredByPartyId,
    performedByUserId,
	created,
    blobStoragePolicyPath,
    blobStorageVersionId,
	resourceid,
	resourcetype,
    performedByPartyId
  )
  VALUES (
    _delegationChangeType,
    _altinnAppId,
    _offeredByPartyId,
    _coveredByUserId,
    _coveredByPartyId,
    _performedByUserId,
	_performedDateTime,
    _blobStoragePolicyPath,
    _blobStorageVersionId,
	_resourceid,
	_resourcetype,
    _performedByPartyId
  ) RETURNING *;
$BODY$;

ALTER FUNCTION delegation.insert_delegationchange(delegation.delegationchangetype, character varying, integer, integer, integer, integer, integer, timestamp with time zone, character varying, character varying, character varying, character varying)
    OWNER TO platform_authorization_admin;
