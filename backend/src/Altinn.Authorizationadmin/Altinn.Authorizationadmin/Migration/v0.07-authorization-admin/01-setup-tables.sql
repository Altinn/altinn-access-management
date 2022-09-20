--Adding columns to the table delegation.delegationChanges
ALTER TABLE delegation.delegationChanges
ADD COLUMN resourceid character varying,
ADD COLUMN resourcetype character varying;
