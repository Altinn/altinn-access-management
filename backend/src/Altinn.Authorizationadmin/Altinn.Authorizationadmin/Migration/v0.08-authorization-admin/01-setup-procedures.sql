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
	  INNER JOIN (
	  SELECT MAX(delegationChangeId) AS maxChange
	 	FROM delegation.delegationchanges
		WHERE
		offeredByPartyId = _offeredByPartyId
		AND resourcetype = _resourcetype
		GROUP BY resourceid, offeredByPartyId, coveredByPartyId, coveredByUserId
	  )AS selectMaxChange
	 ON delegationChangeId = selectMaxChange.maxChange
	 WHERE delegationchangetype='grant'
$BODY$;

