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
