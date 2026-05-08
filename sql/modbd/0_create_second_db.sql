-- DE ADAUGAT IN tsnames.ora !!! (la mine e in calea C:\_dev\WINDOWS.X64_193000_db_home\network\admin)
/*
ORCLPDB =
  (DESCRIPTION =
    (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))
    (CONNECT_DATA = (SERVICE_NAME = orclpdb))
  )

ORCLPDB2 =
  (DESCRIPTION =
    (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))
    (CONNECT_DATA = (SERVICE_NAME = orclpdb2))
  )

-- Aliases per lab convention
BD_AM =
  (DESCRIPTION =
    (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))
    (CONNECT_DATA = (SERVICE_NAME = orclpdb))
  )

BD_EU =
  (DESCRIPTION =
    (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))
    (CONNECT_DATA = (SERVICE_NAME = orclpdb2))
  )
*/


-- Connect to root container
ALTER SESSION SET CONTAINER = CDB$ROOT;

-- Create the second PDB as a clone of seed
CREATE PLUGGABLE DATABASE ORCLPDB2
  ADMIN USER pdbadmin IDENTIFIED BY admin
  FILE_NAME_CONVERT=('pdbseed','orclpdb2');

ALTER PLUGGABLE DATABASE ORCLPDB2 OPEN;
ALTER PLUGGABLE DATABASE ORCLPDB2 SAVE STATE;
ALTER SYSTEM REGISTER;

-- Verify
SELECT name, open_mode FROM v$pdbs;