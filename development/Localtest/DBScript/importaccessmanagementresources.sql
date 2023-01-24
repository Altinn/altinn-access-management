--drop table temp_import;
-- create temporary table for storing json data
CREATE TEMPORARY TABLE temp_import_amresource (doc JSON);

select * from temp_import_amresource;
-- copy file content to newly created table
COPY temp_import_amresource from '../testdata/accessmanagementresourcend.json';
select * from temp_import_amresource;
INSERT INTO accessmanagement.resource (resourceregistryid, resourcetype, created, modified)
SELECT (doc->>'resourceregistryid')::text, (doc->>'resourcetype')::text, (doc->>'created')::timestamp,(doc->>'modified')::timestamp
FROM temp_import_amresource as tmp
WHERE not exists (select resourceregistryid from accessmanagement.resource where resourceregistryid = (tmp.doc->>'resourceregistryid')::text);

DROP TABLE temp_import_amresource;
SELECT * FROM accessmanagement.resource;
