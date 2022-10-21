CREATE OR REPLACE FUNCTION delegation.get_receiveddelegations(
	_coveredbypartyid integer,
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
		coveredByPartyID = _coveredByPartyId
		AND resourcetype = _resourcetype
		GROUP BY resourceid, coveredByPartyId, offeredByPartyId, coveredByUserId
	  )AS selectMaxChange
	 ON delegationChangeId = selectMaxChange.maxChange
	 WHERE delegationchangetype!='revoke_last'
$BODY$;
