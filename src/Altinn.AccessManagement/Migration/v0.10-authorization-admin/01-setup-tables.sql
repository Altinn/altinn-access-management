--Adding columns to the table delegation.delegationChanges
ALTER TABLE delegation.delegationChanges
ADD COLUMN performedbypartyid integer,
ALTER COLUMN performedbyuserid DROP NOT NULL;


