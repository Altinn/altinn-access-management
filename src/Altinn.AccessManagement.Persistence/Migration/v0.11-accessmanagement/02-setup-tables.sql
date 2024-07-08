-- Enum: delegation.delegationChangeType
DO $$ BEGIN
	CREATE TYPE delegation.uuidType AS ENUM ('urn:altinn:person:uuid', 'urn:altinn:organization:uuid', 'urn:altinn:systemuser:uuid', 'urn:altinn:enterpriseuser:uuid');
EXCEPTION
	WHEN duplicate_object THEN null;
END $$;

-- Table: delegation.delegationchanges
ALTER TABLE delegation.delegationchanges
	ADD COLUMN fromUuid UUID,
	ADD COLUMN fromType delegation.uuidType,
	ADD COLUMN toUuid UUID,
	ADD COLUMN toType delegation.uuidType,
	ADD COLUMN performedByUuid UUID,
	ADD COLUMN performedByType delegation.uuidType;

-- Index: idx_delegation_from
CREATE INDEX IF NOT EXISTS idx_delegation_from
	ON delegation.delegationChanges USING btree
	(fromUuid, fromType ASC NULLS LAST)
	TABLESPACE pg_default;
	
-- Index: idx_delegation_to
CREATE INDEX IF NOT EXISTS idx_delegation_to
	ON delegation.delegationChanges USING btree
	(toUuid, toType ASC NULLS LAST)
	TABLESPACE pg_default;
	
-- Table: delegation.resourceregistrydelegationchanges
ALTER TABLE delegation.resourceregistrydelegationchanges
	ADD COLUMN fromUuid UUID,
	ADD COLUMN fromType delegation.uuidType,
	ADD COLUMN toUuid UUID,
	ADD COLUMN toType delegation.uuidType,
	ADD COLUMN performedByUuid UUID,
	ADD COLUMN performedByType delegation.uuidType;

-- Index: idx_rrdelegation_from
CREATE INDEX IF NOT EXISTS idx_rrdelegation_from
	ON delegation.resourceregistryDelegationchanges USING btree
	(fromUuid, fromType ASC NULLS LAST)
	TABLESPACE pg_default;
	
-- Index: idx_rrdelegation_to
CREATE INDEX IF NOT EXISTS idx_rrdelegation_to
	ON delegation.resourceregistryDelegationChanges USING btree
	(toUuid, toType ASC NULLS LAST)
	TABLESPACE pg_default;
