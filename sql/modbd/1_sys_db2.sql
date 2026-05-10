-- =============================================================
-- Script de rulat pe DB2 (ORCLPDB2) ca SYS / SYSDBA
-- Stație 2 - găzduiește: ARTGALLERY_EU
-- =============================================================
-- Conectare exemplu:
--   sqlplus sys/<parola_sys>@//localhost:1521/ORCLPDB2 as sysdba

ALTER SESSION SET CONTAINER = ORCLPDB2;

-- Utilizator găzduit pe DB2
CREATE USER ARTGALLERY_EU IDENTIFIED BY parola_eu;
GRANT DBA TO ARTGALLERY_EU;
GRANT UNLIMITED TABLESPACE TO ARTGALLERY_EU;

-- DB Links definite de pe DB2
-- link_am     -> DB1 (ARTGALLERY_AM pe ORCLPDB) prin Net Service Name BD_AM
-- link_global -> DB1 (ARTGALLERY_GLOBAL pe ORCLPDB)
-- link_bddall -> DB1 (BDDALL pe ORCLPDB) – necesar pentru popularea inițială din sursa centralizată
-- link_eu     -> loopback la DB2 (ARTGALLERY_EU pe ORCLPDB2)
CREATE PUBLIC DATABASE LINK link_am
    CONNECT TO ARTGALLERY_AM IDENTIFIED BY parola_am
    USING 'BD_AM';

CREATE PUBLIC DATABASE LINK link_global
    CONNECT TO ARTGALLERY_GLOBAL IDENTIFIED BY parola_global
    USING 'BD_AM';

CREATE PUBLIC DATABASE LINK link_bddall
    CONNECT TO BDDALL IDENTIFIED BY "1234"
    USING 'BD_AM';

CREATE PUBLIC DATABASE LINK link_eu
    CONNECT TO ARTGALLERY_EU IDENTIFIED BY parola_eu
    USING 'BD_EU';

COMMIT;



---- Drop link-urile create gre?it
--DROP PUBLIC DATABASE LINK link_am;
--DROP PUBLIC DATABASE LINK link_global;
--DROP PUBLIC DATABASE LINK link_bddall;
--DROP PUBLIC DATABASE LINK link_eu;


-- Conectare fara tsnames.ora
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
 
CREATE PUBLIC DATABASE LINK link_bddall
CONNECT TO BDDALL IDENTIFIED BY "1234"
USING '(DESCRIPTION=
           (ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))
           (CONNECT_DATA=(SERVICE_NAME=ORCLPDB))
       )';
 
CREATE PUBLIC DATABASE LINK link_eu
CONNECT TO ARTGALLERY_EU IDENTIFIED BY parola_eu
USING '(DESCRIPTION=
           (ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))
           (CONNECT_DATA=(SERVICE_NAME=ORCLPDB2))
       )';
 
COMMIT;


-- Verificare
 SELECT * FROM dual@link_am;
 SELECT * FROM dual@link_global;
 SELECT * FROM dual@link_bddall;
-- 
-- SELECT OWNER, DB_LINK, USERNAME, HOST, CREATED 
--FROM DBA_DB_LINKS;
