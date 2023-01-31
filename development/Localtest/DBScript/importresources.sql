--drop table temp_import;
-- create temporary table for storing json data
CREATE TEMPORARY TABLE temp_import_resources (doc JSON);

select * from temp_import_resources;
-- copy file content to newly created table
COPY temp_import_resources from '../testdata/resourcesnd.json';

select * from temp_import_resources;

INSERT INTO resourceregistry.resources
SELECT (doc->>'identifier')::text, (doc->>'created')::time, (doc->>'modified')::time, (doc->>'serviceresourcejson')::jsonb
FROM temp_import_resources as tmp
WHERE not exists (select identifier from resourceregistry.resources where identifier = (tmp.doc->>'identifier')::text);
DROP TABLE temp_import_resources;
SELECT * FROM resourceregistry.resources;