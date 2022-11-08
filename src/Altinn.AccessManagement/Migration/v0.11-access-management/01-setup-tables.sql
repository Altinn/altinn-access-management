-- Migrate altinnappid to new resourceid column and set resourcetype
UPDATE delegation.delegationChanges
SET resourceid = altinnappid,
resourcetype = 'orgapp';

-- Index: idx_resourceid
CREATE INDEX IF NOT EXISTS idx_resourceid
  ON delegation.delegationChanges USING btree
  (resourceid COLLATE pg_catalog."default" ASC NULLS LAST)
  TABLESPACE pg_default;