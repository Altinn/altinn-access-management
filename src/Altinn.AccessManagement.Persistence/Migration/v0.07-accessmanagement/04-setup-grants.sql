-- Grants for new accessmanagement schema for the runtime user
GRANT USAGE ON SCHEMA accessmanagement TO platform_authorization;
GRANT SELECT,INSERT,UPDATE,REFERENCES,DELETE,TRUNCATE,TRIGGER ON ALL TABLES IN SCHEMA accessmanagement TO platform_authorization;
GRANT ALL ON ALL SEQUENCES IN SCHEMA accessmanagement TO platform_authorization;

-- Grants for new table and sequence in delegation schema for the runtime user
GRANT SELECT,INSERT,UPDATE,REFERENCES,DELETE,TRUNCATE,TRIGGER ON ALL TABLES IN SCHEMA delegation TO platform_authorization;
GRANT ALL ON ALL SEQUENCES IN SCHEMA delegation TO platform_authorization;