--Procedure: delegation.get_all_offereddelegations
CREATE OR REPLACE FUNCTION delegation.get_all_offereddelegations(
	_offeredbypartyid integer,
	_resourcetype character varying)
    RETURNS SETOF delegation.delegationchanges 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1000

AS $BODY$
	  SELECT
	 delegationchangeid,
    delegationChangeType,
    altinnappid,
    offeredbypartyid,
    coveredbypartyid,
    coveredbyuserid,
    performedbyuserid,
    blobstoragepolicypath,
    blobstorageversionid,
    created,
	resourceid,
	resourcetype
	  FROM delegation.delegationChanges
	  WHERE
		offeredByPartyId = _offeredByPartyId
		and resourcetype = _resourcetype
$BODY$;

