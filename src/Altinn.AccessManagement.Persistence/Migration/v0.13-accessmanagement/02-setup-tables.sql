-- Enum: delegation.instanceType
DO $$ BEGIN
	CREATE TYPE delegation.instanceDelegationSource AS ENUM ('user', 'app');
EXCEPTION
	WHEN duplicate_object THEN null;
END $$;

-- Table: delegation.instancedelegationchanges

-- DROP TABLE IF EXISTS delegation.instancedelegationchanges;
ALTER TABLE IF EXISTS delegation.instancedelegationchanges ADD COLUMN IF NOT EXISTS instanceDelegationSource delegation.instanceDelegationSource NOT NULL DEFAULT 'app'


