--drop table temp_import;
-- create temporary table for storing json data
CREATE TEMPORARY TABLE temp_import_resourceregistrydelegationchanges (doc JSON);

select * from temp_import_resourceregistrydelegationchanges;
-- copy file content to newly created table
COPY temp_import_resourceregistrydelegationchanges from '../testdata/resourcedelegationchangesnd.json';

select * from temp_import_resourceregistrydelegationchanges;

INSERT INTO delegation.resourceregistrydelegationchanges (delegationchangetype, resourceid_fk, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, performedbypartyid, blobstoragepolicypath, blobstorageversionid, created)
SELECT (doc->>'delegationchangetype')::delegation.delegationchangetype, (doc->>'resourceid_fk')::integer, (doc->>'offeredbypartyid')::integer, (doc->>'coveredbypartyid')::integer,(doc->>'coveredbyuserid')::integer,(doc->>'performedbyuserid')::integer,(doc->>'performedbypartyid')::integer,(doc->>'blobstoragepolicypath')::text,(doc->>'blobstorageversionid')::text,(doc->>'created')::timestamp
FROM temp_import_resourceregistrydelegationchanges as tmp
WHERE not exists (select  resourceid_fk from delegation.resourceregistrydelegationchanges where resourceid_fk = (tmp.doc->>'resourceid_fk')::integer and offeredbypartyid = (doc->>'offeredbypartyid')::integer and coveredbypartyid =(doc->>'coveredbypartyid')::integer);

DROP TABLE temp_import_resourceregistrydelegationchanges;
SELECT * FROM delegation.resourceregistrydelegationchanges;