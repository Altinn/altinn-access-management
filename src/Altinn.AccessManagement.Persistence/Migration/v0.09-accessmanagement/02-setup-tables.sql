-- Table: accessmanagement.resource
ALTER TABLE accessmanagement.resource
DROP COLUMN IF EXISTS resourcestatus;

-- Index: dropping resourcestatus will automatically drop the index idx_resource_resourcereferenceid and must be recreated without the resourcestatus
CREATE INDEX IF NOT EXISTS idx_resource_resourcereferenceid
  ON accessmanagement.resource USING btree
  (resourceRegistryId COLLATE pg_catalog."default" ASC NULLS LAST)
  INCLUDE(resourceType)
  TABLESPACE pg_default;

-- A drop create as there is no "add constraint if not exists" this is just for local dev where changes to yuniql db can be performed.
ALTER TABLE accessmanagement.resource
DROP CONSTRAINT IF EXISTS unique_resourceregisterid;

ALTER TABLE accessmanagement.resource
ADD CONSTRAINT unique_resourceregisterid UNIQUE (resourceregistryid);