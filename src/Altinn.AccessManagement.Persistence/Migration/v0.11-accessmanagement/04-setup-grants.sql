-- Setup grants again for delegation schema for the runtime user
GRANT USAGE ON SCHEMA delegation TO platform_authorization;
GRANT SELECT,INSERT,UPDATE,REFERENCES,DELETE,TRUNCATE,TRIGGER ON ALL TABLES IN SCHEMA delegation TO platform_authorization;
GRANT ALL ON ALL SEQUENCES IN SCHEMA delegation TO platform_authorization;