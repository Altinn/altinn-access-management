-- Function: new insert function delegation.insert_delegationchange to replace old stored proc delegation.insert_change
DROP FUNCTION IF EXISTS delegation.insert_delegationchange(delegation.delegationchangetype, character varying, integer, integer, integer, integer, character varying, character varying);

CREATE OR REPLACE FUNCTION delegation.insert_delegationchange(
    _delegationChangeType delegation.delegationChangeType,
    _altinnappid character varying,
    _offeredbypartyid integer,
    _coveredbyuserid integer,
    _coveredbypartyid integer,
    _performedbyuserid integer,
    _blobstoragepolicypath character varying,
    _blobstorageversionid character varying
)
RETURNS TABLE (
	delegationChangeId int,
	delegationChangeType delegation.delegationChangeType,
	altinnAppId text,
	offeredByPartyId int,
	coveredByUserId int,
	coveredByPartyId int,
	performedByUserId int,
	blobStoragePolicyPath text,
	blobStorageVersionId text,
	created timestamp with time zone)
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
    blobStorageVersionId
  )
  VALUES (
    _delegationChangeType,
    _altinnAppId,
    _offeredByPartyId,
    _coveredByUserId,
    _coveredByPartyId,
    _performedByUserId,
    _blobStoragePolicyPath,
    _blobStorageVersionId
  )
  RETURNING
    delegationChangeId,
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByUserId,
    coveredByPartyId,
    performedByUserId,
    blobStoragePolicyPath,
    blobStorageVersionId,
    created
$BODY$;

-- Function: get_all_changes from including isDeleted to return delegationChangeType
DROP FUNCTION IF EXISTS delegation.get_all_changes(character varying, integer, integer, integer);

CREATE OR REPLACE FUNCTION delegation.get_all_changes(
  _altinnAppId character varying,
  _offeredByPartyId integer,
  _coveredByUserId integer,
  _coveredByPartyId integer
)
RETURNS TABLE (
	delegationChangeId int,
	delegationChangeType delegation.delegationChangeType,
	altinnAppId text,
	offeredByPartyId int,
	coveredByUserId int,
	coveredByPartyId int,
	performedByUserId int,
	blobStoragePolicyPath text,
	blobStorageVersionId text,
	created timestamp with time zone)
LANGUAGE 'sql'
STABLE PARALLEL SAFE
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
    created
  FROM delegation.delegationChanges
  WHERE
    altinnAppId = _altinnAppId
    AND offeredByPartyId = _offeredByPartyId
    AND (_coveredByUserId IS NULL OR coveredByUserId = _coveredByUserId)
    AND (_coveredByPartyId IS NULL OR coveredByPartyId = _coveredByPartyId)
$BODY$;


-- Function: delegation.get_all_current_changes from including isDeleted to return delegationChangeType
DROP FUNCTION IF EXISTS delegation.get_all_current_changes_offeredbypartyid_only(character varying[], integer[]);

CREATE OR REPLACE FUNCTION delegation.get_all_current_changes_offeredbypartyid_only(
  _altinnappids character varying[],
  _offeredbypartyids integer[]
)
RETURNS TABLE (
	delegationChangeId int,
	delegationChangeType delegation.delegationChangeType,
	altinnAppId text,
	offeredByPartyId int,
	coveredByUserId int,
	coveredByPartyId int,
	performedByUserId int,
	blobStoragePolicyPath text,
	blobStorageVersionId text,
	created timestamp with time zone)
LANGUAGE 'sql'
STABLE PARALLEL SAFE 
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
    created
  FROM delegation.delegationchanges
	INNER JOIN
	(
		SELECT MAX(delegationChangeId) AS maxChange
	 	FROM delegation.delegationchanges
		WHERE
		  (_altinnappids IS NULL OR altinnAppId = ANY (_altinnAppIds))
		  AND (offeredByPartyId = ANY (_offeredByPartyIds))
		GROUP BY altinnAppId, offeredByPartyId, coveredByPartyId, coveredByUserId
	) AS selectMaxChange
	ON delegationChangeId = selectMaxChange.maxChange
$BODY$;


-- Function: delegation.get_all_current_changes_coveredbypartyids from including isDeleted to return delegationChangeType
DROP FUNCTION IF EXISTS delegation.get_all_current_changes_coveredbypartyids(character varying[], integer[], integer[]);

CREATE OR REPLACE FUNCTION delegation.get_all_current_changes_coveredbypartyids(
  _altinnappids character varying[],
  _offeredbypartyids integer[],
  _coveredbypartyids integer[]
)
RETURNS TABLE (
	delegationChangeId int,
	delegationChangeType delegation.delegationChangeType,
	altinnAppId text,
	offeredByPartyId int,
	coveredByUserId int,
	coveredByPartyId int,
	performedByUserId int,
	blobStoragePolicyPath text,
	blobStorageVersionId text,
	created timestamp with time zone)
LANGUAGE 'sql'
STABLE PARALLEL SAFE
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
    created
  FROM delegation.delegationchanges
    INNER JOIN
    (
	  SELECT MAX(delegationChangeId) AS maxChange
	  FROM delegation.delegationchanges
	  WHERE
	    (_altinnappids IS NULL OR altinnAppId = ANY (_altinnAppIds))
	    AND (offeredByPartyId = ANY (_offeredByPartyIds))
	    AND coveredByPartyId = ANY (_coveredByPartyIds)
      GROUP BY altinnAppId, offeredByPartyId, coveredByPartyId
    ) AS selectMaxChange
    ON delegationChangeId = selectMaxChange.maxChange
$BODY$;

-- Function: delegation.get_all_current_changes_coveredbyuserids from including isDeleted to return delegationChangeType
DROP FUNCTION IF EXISTS delegation.get_all_current_changes_coveredbyuserids(character varying[], integer[], integer[]);

CREATE OR REPLACE FUNCTION delegation.get_all_current_changes_coveredbyuserids(
  _altinnappids character varying[],
  _offeredbypartyids integer[],
  _coveredbyuserids integer[]
)
RETURNS TABLE (
	delegationChangeId int,
	delegationChangeType delegation.delegationChangeType,
	altinnAppId text,
	offeredByPartyId int,
	coveredByUserId int,
	coveredByPartyId int,
	performedByUserId int,
	blobStoragePolicyPath text,
	blobStorageVersionId text,
	created timestamp with time zone)
LANGUAGE 'sql'
STABLE PARALLEL SAFE 
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
    created
  FROM delegation.delegationchanges
    INNER JOIN
    (
	  SELECT MAX(delegationChangeId) AS maxChange
	  FROM delegation.delegationchanges
	  WHERE
        (_altinnappids IS NULL OR altinnAppId = ANY (_altinnAppIds))
        AND (offeredByPartyId = ANY (_offeredByPartyIds))
        AND coveredByUserId = ANY (_coveredByUserIds)
	  GROUP BY altinnAppId, offeredByPartyId, coveredByUserId
    ) AS selectMaxChange
    ON delegationChangeId = selectMaxChange.maxChange
$BODY$;

-- Function: select_delegationchanges_by_id_range
DROP FUNCTION IF EXISTS delegation.select_delegationchanges_by_id_range(bigint, bigint);

CREATE OR REPLACE FUNCTION delegation.select_delegationchanges_by_id_range(
	_startid bigint,
	_endid bigint DEFAULT '9223372036854775807'::bigint)
    RETURNS TABLE(
        delegationchangeid integer,
        delegationchangetype delegation.delegationchangetype,
        altinnappid text,
        offeredbypartyid integer,
        coveredbypartyid integer,
        coveredbyuserid integer,
        performedbyuserid integer,
        blobstoragepolicypath text,
        blobstorageversionid text,
        created timestamp with time zone) 
    LANGUAGE 'sql'
    COST 100
    STABLE PARALLEL SAFE 
    ROWS 1000

AS $BODY$

  SELECT
    delegationChangeId,
    delegationChangeType,
    altinnAppId, 
    offeredByPartyId,
    coveredByPartyId,
    coveredByUserId,    
    performedByUserId,
    blobStoragePolicyPath,
    blobStorageVersionId,
    created
  FROM delegation.delegationChanges
  WHERE
    delegationChangeId BETWEEN _startId AND _endId
$BODY$;

-- Function: update delegation.get_current_change for correct select order coveredByPartyId before coveredByUserId to match delegation.delegationchanges column order
DROP FUNCTION IF EXISTS delegation.get_current_change(character varying, integer, integer, integer);
CREATE OR REPLACE FUNCTION delegation.get_current_change(
	_altinnappid character varying,
	_offeredbypartyid integer,
	_coveredbyuserid integer,
	_coveredbypartyid integer)
    RETURNS TABLE(
        delegationchangeid integer,
        delegationchangetype delegation.delegationchangetype,
        altinnappid text,
        offeredbypartyid integer,
        coveredbypartyid integer,
        coveredbyuserid integer,
        performedbyuserid integer,
        blobstoragepolicypath text,
        blobstorageversionid text,
        created timestamp with time zone) 
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
    coveredByPartyId,
    coveredByUserId,
    performedByUserId,
    blobStoragePolicyPath,
    blobStorageVersionId,
    created
  FROM delegation.delegationChanges
  WHERE
    altinnAppId = _altinnAppId
    AND offeredByPartyId = _offeredByPartyId
    AND (_coveredByUserId IS NULL OR coveredByUserId = _coveredByUserId)
    AND (_coveredByPartyId IS NULL OR coveredByPartyId = _coveredByPartyId)
  ORDER BY delegationChangeId DESC LIMIT 1
$BODY$;
