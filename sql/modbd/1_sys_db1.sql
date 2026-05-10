-- =============================================================
-- Script de rulat pe DB1 (ORCLPDB) ca SYS / SYSDBA
-- Stație 1 - găzduiește: BDDALL, ARTGALLERY_AM, ARTGALLERY_GLOBAL
-- =============================================================
-- Conectare exemplu:
--   sqlplus sys/<parola_sys>@//localhost:1521/ORCLPDB as sysdba

ALTER PLUGGABLE DATABASE ORCLPDB OPEN;
ALTER SESSION SET CONTAINER = ORCLPDB;

-- Utilizatori găzduiți pe DB1
CREATE USER BDDALL              IDENTIFIED BY 1234;
CREATE USER ARTGALLERY_AM       IDENTIFIED BY parola_am;
CREATE USER ARTGALLERY_GLOBAL   IDENTIFIED BY parola_global;

GRANT DBA TO BDDALL, ARTGALLERY_AM, ARTGALLERY_GLOBAL;
GRANT UNLIMITED TABLESPACE TO BDDALL;
GRANT UNLIMITED TABLESPACE TO ARTGALLERY_AM;
GRANT UNLIMITED TABLESPACE TO ARTGALLERY_GLOBAL;

-- DB Links definite de pe DB1
-- link_eu  -> DB2 (ARTGALLERY_EU pe ORCLPDB2) prin Net Service Name BD_EU
-- link_am  -> loopback la DB1 (ARTGALLERY_AM pe ORCLPDB) prin Net Service Name BD_AM
-- link_global -> loopback la DB1 (ARTGALLERY_GLOBAL pe ORCLPDB)
CREATE PUBLIC DATABASE LINK link_eu
    CONNECT TO ARTGALLERY_EU IDENTIFIED BY parola_eu
    USING 'BD_EU';

CREATE PUBLIC DATABASE LINK link_am
    CONNECT TO ARTGALLERY_AM IDENTIFIED BY parola_am
    USING 'BD_AM';

CREATE PUBLIC DATABASE LINK link_global
    CONNECT TO ARTGALLERY_GLOBAL IDENTIFIED BY parola_global
    USING 'BD_AM';

COMMIT;


-- Drop link-urile create gre?it
--DROP PUBLIC DATABASE LINK link_bddall;
--DROP PUBLIC DATABASE LINK link_am;
--DROP PUBLIC DATABASE LINK link_global;
--DROP PUBLIC DATABASE LINK link_eu;


-- Conectare fara tsnames.ora
 CREATE PUBLIC DATABASE LINK link_eu
 CONNECT TO ARTGALLERY_EU IDENTIFIED BY parola_eu
 USING '(DESCRIPTION=
            (ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))
            (CONNECT_DATA=(SERVICE_NAME=ORCLPDB2))
        )';
 
 CREATE PUBLIC DATABASE LINK link_am
 CONNECT TO ARTGALLERY_AM IDENTIFIED BY parola_am
 USING '(DESCRIPTION=
            (ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))
            (CONNECT_DATA=(SERVICE_NAME=ORCLPDB))
        )';
 
 CREATE PUBLIC DATABASE LINK link_global
 CONNECT TO ARTGALLERY_GLOBAL IDENTIFIED BY parola_global
 USING '(DESCRIPTION=
            (ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))
            (CONNECT_DATA=(SERVICE_NAME=ORCLPDB))
        )';
 
 COMMIT;

-- Verificare
  SELECT * FROM dual@link_eu;
  SELECT * FROM dual@link_am;
  SELECT * FROM dual@link_global;
 
 
 
-- SELECT OWNER, DB_LINK, USERNAME, HOST, CREATED 
-- FROM DBA_DB_LINKS;
