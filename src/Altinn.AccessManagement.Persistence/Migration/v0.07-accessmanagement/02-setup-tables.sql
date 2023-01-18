-- Table: accessmanagement.resource
CREATE TABLE IF NOT EXISTS accessmanagement.Resource
(
  resourceId bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  resourceRegistryId text NOT NULL,
  resourceType text NOT NULL,
  created timestamp with time zone NOT NULL,
  modified timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
)
TABLESPACE pg_default;

-- Index: idx_resource_resourcereferenceid
CREATE INDEX IF NOT EXISTS idx_resource_resourcereferenceid
  ON accessmanagement.resource USING btree
  (resourceRegistryId COLLATE pg_catalog."default" ASC NULLS LAST)
  INCLUDE(resourceType)
  TABLESPACE pg_default;

-- Table: delegation.ResourceRegistryDelegationChanges
CREATE TABLE IF NOT EXISTS delegation.ResourceRegistryDelegationChanges
(
  resourceRegistryDelegationChangeId bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  delegationChangeType delegation.delegationChangeType NOT NULL,
  resourceId_fk integer NOT NULL,
  offeredByPartyId integer NOT NULL,
  coveredByPartyId integer,
  coveredByUserId integer,
  performedByUserId integer,
  performedByPartyId integer,
  blobStoragePolicyPath text COLLATE pg_catalog."default" NOT NULL,
  blobStorageVersionId text COLLATE pg_catalog."default" NOT NULL,
  created timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
)
TABLESPACE pg_default;

-- Set start value of resourceRegistryDelegationChangeId to 100K to allow some room for manual debugging without collision
ALTER SEQUENCE delegation.resourceregistrydelegationcha_resourceregistrydelegationcha_seq RESTART WITH 100000 INCREMENT BY 1;

-- Foreign Key: delegation.resourceregistrydelegationchanges.resourceId_fk
ALTER TABLE delegation.ResourceRegistryDelegationChanges
ADD CONSTRAINT resourceregistrydelegationchanges_resourceid_fk
FOREIGN KEY (resourceId_fk)
REFERENCES accessmanagement.resource(resourceId);

-- Index: idx_rrdelegation_offeredby
CREATE INDEX IF NOT EXISTS idx_rrdelegation_offeredby
    ON delegation.ResourceRegistryDelegationChanges USING btree
    (offeredbypartyid ASC NULLS LAST)
    TABLESPACE pg_default;

-- Index: idx_rrdelegation_coveredbyuser
CREATE INDEX IF NOT EXISTS idx_rrdelegation_coveredbyuser
    ON delegation.ResourceRegistryDelegationChanges USING btree
    (coveredbyuserid ASC NULLS LAST)
    TABLESPACE pg_default;

-- Index: idx_rrdelegation_coveredbyparty
CREATE INDEX IF NOT EXISTS idx_rrdelegation_coveredbyparty
    ON delegation.ResourceRegistryDelegationChanges USING btree
    (coveredbypartyid ASC NULLS LAST)
    TABLESPACE pg_default;