-- Enum: delegation.instanceType
DO $$ BEGIN
	CREATE TYPE delegation.instanceDelegationMode AS ENUM ('parallelsigning', 'normal');
EXCEPTION
	WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
	CREATE TYPE delegation.uuidtype AS ENUM ('urn:altinn:person:uuid', 'urn:altinn:organization:uuid', 'urn:altinn:systemuser:uuid', 'urn:altinn:enterpriseuser:uuid', 'urn:altinn:resource');
EXCEPTION
	WHEN duplicate_object THEN 
        ALTER TYPE delegation.uuidtype ADD VALUE IF NOT EXISTS 'urn:altinn:resource';
END $$;

-- Table: delegation.instancedelegationchanges

-- DROP TABLE IF EXISTS delegation.instancedelegationchanges;

CREATE TABLE IF NOT EXISTS delegation.instancedelegationchanges
(
    instancedelegationchangeid bigint NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
    delegationchangetype delegation.delegationchangetype NOT NULL,
    instanceDelegationMode delegation.instanceDelegationMode NOT NULL,
    resourceid text NOT NULL,
    instanceid text NOT NULL,
    fromuuid uuid NOT NULL,
    fromtype delegation.uuidtype NOT NULL,
    touuid uuid NOT NULL,
    totype delegation.uuidtype NOT NULL,
    performedby text,
    performedbytype delegation.uuidtype,
    blobstoragepolicypath text COLLATE pg_catalog."default" NOT NULL,
    blobstorageversionid text COLLATE pg_catalog."default" NOT NULL,
    created timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS delegation.instancedelegationchanges
    OWNER to platform_authorization_admin;

GRANT ALL ON TABLE delegation.instancedelegationchanges TO platform_authorization;

GRANT ALL ON TABLE delegation.instancedelegationchanges TO platform_authorization_admin;

-- Index: idx_instancedelegation_from

-- DROP INDEX IF EXISTS delegation.idx_instancedelegation_from;

CREATE INDEX IF NOT EXISTS idx_instancedelegation_from
    ON delegation.instancedelegationchanges USING btree
    (fromuuid ASC NULLS LAST, fromtype ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_instancedelegation_to

-- DROP INDEX IF EXISTS delegation.idx_instancedelegation_to;

CREATE INDEX IF NOT EXISTS idx_instancedelegation_to
    ON delegation.instancedelegationchanges USING btree
    (touuid ASC NULLS LAST, totype ASC NULLS LAST)
    TABLESPACE pg_default;