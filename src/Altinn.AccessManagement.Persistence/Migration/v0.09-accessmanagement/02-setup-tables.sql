-- Table: accessmanagement.resource
ALTER TABLE accessmanagement.resource
DROP COLUMN IF EXISTS resourcestatus;

-- A drop create as there is no "add constraint if not exists" this is just for local dev where changes to yuniql db can be performed.
ALTER TABLE accessmanagement.resource
DROP CONSTRAINT IF EXISTS unique_resourceregisterid;

ALTER TABLE accessmanagement.resource
ADD CONSTRAINT unique_resourceregisterid UNIQUE (resourceregistryid);