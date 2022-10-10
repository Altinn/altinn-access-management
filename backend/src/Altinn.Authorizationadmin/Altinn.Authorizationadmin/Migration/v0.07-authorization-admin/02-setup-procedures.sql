--Procedure: delegation.InsertResourceDelegation
CREATE OR REPLACE FUNCTION delegation.InsertResourceDelegation(
  _delegationChangeType delegation.delegationChangeType,
  _altinnappid character varying,
  _offeredbypartyid integer,
  _coveredbyuserid integer,
  _coveredbypartyid integer,
  _performedbyuserid integer,
  _blobstoragepolicypath character varying,
  _blobstorageversionid character varying,
  _resourceid character varying,
  _resourcetype character varying
)
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
    blobStoragePolicyPath,
    blobStorageVersionId,
    resourceid,
    resourcetype
  )
  VALUES (
    _delegationChangeType,
    _altinnAppId,
    _offeredByPartyId,
    _coveredByUserId,
    _coveredByPartyId,
    _performedByUserId,
    _blobStoragePolicyPath,
    _blobStorageVersionId,
    _resourceid,
    _resourcetype
  ) RETURNING *;
$BODY$;

--Procedure: delegation.get_delegatedresources
CREATE OR REPLACE FUNCTION delegation.get_delegatedresources(
	_offeredbypartyid integer)
    RETURNS TABLE(offeredbypartyid integer, performedbyuserid integer, coveredbypartyid integer, resourceid character varying, serviceresourcejson jsonb) 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1000

AS $BODY$
	  SELECT
	  offeredByPartyId,
	  performedbyuserid,
	  coveredbypartyid,
	  resourceid,
	  serviceresourcejson
	  FROM delegation.delegationChanges d
	  JOIN resourceregistry.resources r ON r.identifier = d.resourceid
	  WHERE
		offeredByPartyId = _offeredByPartyId
$BODY$;

--Procedure: delegation.get_resources
CREATE OR REPLACE FUNCTION delegation.get_resources(
	_offeredbypartyid integer)
    RETURNS TABLE(resourceid character varying, serviceresourcejson jsonb) 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1000

AS $BODY$
	  SELECT
	  resourceid,
	  serviceresourcejson
	  FROM delegation.delegationChanges d
	  JOIN resourceregistry.resources r ON r.identifier = d.resourceid
	  WHERE
		offeredByPartyId = _offeredByPartyId
$BODY$;

--Procedure: delegation.get_receiveddelegations
CREATE OR REPLACE FUNCTION delegation.get_receiveddelegations(
	_coveredbypartyid integer)
    RETURNS TABLE(performedbyuserid integer, offeredbypartyid integer, coveredbypartyid integer, resourceid character varying, serviceresourcejson jsonb) 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1000

AS $BODY$
	  SELECT
	  performedbyuserid,
	  offeredbypartyid,
	  coveredbypartyid,
	  resourceid,
	  serviceresourcejson
	  FROM delegation.delegationChanges d
	  JOIN resourceregistry.resources r ON r.identifier = d.resourceid
	  WHERE
		coveredbypartyid = _coveredByPartyId
$BODY$;
