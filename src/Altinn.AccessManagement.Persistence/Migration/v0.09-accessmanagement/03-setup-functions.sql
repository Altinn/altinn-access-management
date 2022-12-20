CREATE OR REPLACE FUNCTION accessmanagement.upsert_resourceregistryresource(
	_resourceregistryid text,
	_resourcetype text)
    RETURNS TABLE(resourceid bigint, resourceregistryid text, resourcetype text, created timestamp with time zone, modified timestamp with time zone) 
    LANGUAGE 'sql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1

AS $BODY$
  	
	INSERT INTO 
		accessmanagement.resource (resourceregistryid, resourcetype, created, modified)
	VALUES
		(_resourceregistryid, _resourcetype, now(), now()) 
	ON CONFLICT (resourceregistryid)
	DO
		UPDATE SET resourcetype = _resourcetype, modified = now();	
	
	SELECT	    
		r.resourceid,
		r.resourceregistryid,
		r.resourcetype,
		r.created,
		r.modified
	FROM
		accessmanagement.resource r
	WHERE
		r.resourceregistryid = _resourceregistryid
	
$BODY$;

ALTER FUNCTION accessmanagement.upsert_resourceregistryresource(text, text)
    OWNER TO platform_authorization_admin;
