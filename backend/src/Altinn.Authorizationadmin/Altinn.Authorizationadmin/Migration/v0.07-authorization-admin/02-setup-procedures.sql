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
