-- ============================================================================
-- SINCRONIZARE BIDIRECTIONALA REPLICI ARTIST + COLLECTION
-- Galeria de Arta - Baze de Date Distribuite
-- ============================================================================
-- RELATII REPLICATE:
--   ARTIST     -> ARTIST_AM     (DB1: ORCLPDB,  user: ARTGALLERY_AM)
--                 ARTIST_EU     (DB2: ORCLPDB2, user: ARTGALLERY_EU)
--   COLLECTION -> COLLECTION_AM (DB1)
--                 COLLECTION_EU (DB2)
--
-- ARHITECTURA:
--   - Pachet PKG_REPL_CTRL pe fiecare statie (flag v_replicating per-sesiune)
--   - Pachet PKG_REPL_REMOTE pe fiecare statie (proceduri DML "remote-callable")
--   - Triggere AFTER INSERT/UPDATE/DELETE care apeleaza procedurile remote
--
-- DE CE PROCEDURI REMOTE, NU DML DIRECT PRIN DB LINK?
--   Cand DB1 face DML direct prin link_eu, se deschide sesiune noua pe DB2
--   cu PKG_REPL_CTRL.v_replicating = FALSE. Triggerul de pe DB2 se va declansa
--   si va incerca sa propage inapoi spre DB1 -> bucla infinita / unique
--   constraint violation. Solutia: triggerul AM apeleaza o PROCEDURA pe EU,
--   care seteaza v_replicating = TRUE in sesiunea remote INAINTE de DML,
--   astfel triggerul EU vede flag-ul si nu mai propaga inapoi.
-- ============================================================================


-- ============================================================================
-- PARTEA I - DB1 (ORCLPDB)
-- Conectare: sqlplus ARTGALLERY_AM/parola_am@//localhost:1521/ORCLPDB
-- (sau in SQL Developer: conexiune ARTGALLERY_AM @ ORCLPDB)
-- ============================================================================

ALTER SESSION SET CONTAINER = ORCLPDB;

-- ----------------------------------------------------------------------------
-- PAS 1.1: Pachetul de control al replicarii (flag anti-bucla)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE PACKAGE PKG_REPL_CTRL AS
    v_replicating BOOLEAN := FALSE;
END PKG_REPL_CTRL;
/

-- ----------------------------------------------------------------------------
-- PAS 1.2: Pachetul cu proceduri "remote-callable"
-- Aceste proceduri vor fi apelate de pe DB2 prin link_am.
-- Fiecare seteaza v_replicating=TRUE inainte de DML, astfel triggerul
-- corespunzator de pe AM nu mai propaga inapoi spre EU.
-- ----------------------------------------------------------------------------
CREATE OR REPLACE PACKAGE PKG_REPL_REMOTE AS
    PROCEDURE insert_artist(p_id NUMBER, p_name VARCHAR2, p_nat VARCHAR2,
                            p_birth NUMBER, p_death NUMBER);
    PROCEDURE update_artist(p_id NUMBER, p_name VARCHAR2, p_nat VARCHAR2,
                            p_birth NUMBER, p_death NUMBER);
    PROCEDURE delete_artist(p_id NUMBER);

    PROCEDURE insert_collection(p_id NUMBER, p_name VARCHAR2,
                                p_desc VARCHAR2, p_date DATE);
    PROCEDURE update_collection(p_id NUMBER, p_name VARCHAR2,
                                p_desc VARCHAR2, p_date DATE);
    PROCEDURE delete_collection(p_id NUMBER);
END PKG_REPL_REMOTE;
/

CREATE OR REPLACE PACKAGE BODY PKG_REPL_REMOTE AS

    PROCEDURE insert_artist(p_id NUMBER, p_name VARCHAR2, p_nat VARCHAR2,
                            p_birth NUMBER, p_death NUMBER) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        INSERT INTO ARTIST_AM(artist_id, name, nationality, birth_year, death_year)
        VALUES (p_id, p_name, p_nat, p_birth, p_death);
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE update_artist(p_id NUMBER, p_name VARCHAR2, p_nat VARCHAR2,
                            p_birth NUMBER, p_death NUMBER) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        UPDATE ARTIST_AM
        SET name=p_name, nationality=p_nat, birth_year=p_birth, death_year=p_death
        WHERE artist_id = p_id;
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE delete_artist(p_id NUMBER) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        DELETE FROM ARTIST_AM WHERE artist_id = p_id;
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE insert_collection(p_id NUMBER, p_name VARCHAR2,
                                p_desc VARCHAR2, p_date DATE) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        INSERT INTO COLLECTION_AM(collection_id, name, description, created_date)
        VALUES (p_id, p_name, p_desc, p_date);
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE update_collection(p_id NUMBER, p_name VARCHAR2,
                                p_desc VARCHAR2, p_date DATE) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        UPDATE COLLECTION_AM
        SET name=p_name, description=p_desc, created_date=p_date
        WHERE collection_id = p_id;
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE delete_collection(p_id NUMBER) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        DELETE FROM COLLECTION_AM WHERE collection_id = p_id;
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

END PKG_REPL_REMOTE;
/


-- ----------------------------------------------------------------------------
-- PAS 1.3: GRANT EXECUTE pentru pachet
-- Userul folosit de link_am (de pe DB2) trebuie sa poata apela pachetul.
-- Daca link_am se conecteaza ca ARTGALLERY_AM (cazul comun), grant-ul nu e
-- strict necesar (apelezi propriul pachet), dar il punem pentru claritate.
-- Daca link_am foloseste alt user, schimba ARTGALLERY_AM cu acel user.
-- ----------------------------------------------------------------------------
GRANT EXECUTE ON PKG_REPL_REMOTE TO ARTGALLERY_AM;
-- Daca exista un user de comunicatie distinct, decomenteaza si adapteaza:
-- GRANT EXECUTE ON PKG_REPL_REMOTE TO <user_link>;




-- ============================================================================
-- PARTEA II - DB2 (ORCLPDB2)
-- Conectare: sqlplus ARTGALLERY_EU/parola_eu@//localhost:1521/ORCLPDB2
-- (sau in SQL Developer: conexiune ARTGALLERY_EU @ ORCLPDB2)
-- ============================================================================

ALTER SESSION SET CONTAINER = ORCLPDB2;


-- ----------------------------------------------------------------------------
-- PAS 2.1: Pachetul de control al replicarii (instanta separata pe DB2)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE PACKAGE PKG_REPL_CTRL AS
    v_replicating BOOLEAN := FALSE;
END PKG_REPL_CTRL;
/


-- ----------------------------------------------------------------------------
-- PAS 2.2: Pachetul cu proceduri "remote-callable" pe DB2
-- ----------------------------------------------------------------------------
CREATE OR REPLACE PACKAGE PKG_REPL_REMOTE AS
    PROCEDURE insert_artist(p_id NUMBER, p_name VARCHAR2, p_nat VARCHAR2,
                            p_birth NUMBER, p_death NUMBER);
    PROCEDURE update_artist(p_id NUMBER, p_name VARCHAR2, p_nat VARCHAR2,
                            p_birth NUMBER, p_death NUMBER);
    PROCEDURE delete_artist(p_id NUMBER);

    PROCEDURE insert_collection(p_id NUMBER, p_name VARCHAR2,
                                p_desc VARCHAR2, p_date DATE);
    PROCEDURE update_collection(p_id NUMBER, p_name VARCHAR2,
                                p_desc VARCHAR2, p_date DATE);
    PROCEDURE delete_collection(p_id NUMBER);
END PKG_REPL_REMOTE;
/

CREATE OR REPLACE PACKAGE BODY PKG_REPL_REMOTE AS

    PROCEDURE insert_artist(p_id NUMBER, p_name VARCHAR2, p_nat VARCHAR2,
                            p_birth NUMBER, p_death NUMBER) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        INSERT INTO ARTIST_EU(artist_id, name, nationality, birth_year, death_year)
        VALUES (p_id, p_name, p_nat, p_birth, p_death);
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE update_artist(p_id NUMBER, p_name VARCHAR2, p_nat VARCHAR2,
                            p_birth NUMBER, p_death NUMBER) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        UPDATE ARTIST_EU
        SET name=p_name, nationality=p_nat, birth_year=p_birth, death_year=p_death
        WHERE artist_id = p_id;
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE delete_artist(p_id NUMBER) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        DELETE FROM ARTIST_EU WHERE artist_id = p_id;
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE insert_collection(p_id NUMBER, p_name VARCHAR2,
                                p_desc VARCHAR2, p_date DATE) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        INSERT INTO COLLECTION_EU(collection_id, name, description, created_date)
        VALUES (p_id, p_name, p_desc, p_date);
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE update_collection(p_id NUMBER, p_name VARCHAR2,
                                p_desc VARCHAR2, p_date DATE) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        UPDATE COLLECTION_EU
        SET name=p_name, description=p_desc, created_date=p_date
        WHERE collection_id = p_id;
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

    PROCEDURE delete_collection(p_id NUMBER) IS
    BEGIN
        PKG_REPL_CTRL.v_replicating := TRUE;
        DELETE FROM COLLECTION_EU WHERE collection_id = p_id;
        PKG_REPL_CTRL.v_replicating := FALSE;
    EXCEPTION
        WHEN OTHERS THEN
            PKG_REPL_CTRL.v_replicating := FALSE;
            RAISE;
    END;

END PKG_REPL_REMOTE;
/


-- ----------------------------------------------------------------------------
-- PAS 2.3: GRANT EXECUTE pentru pachet
-- ----------------------------------------------------------------------------
GRANT EXECUTE ON PKG_REPL_REMOTE TO ARTGALLERY_EU;
-- Daca link_eu foloseste alt user, decomenteaza si adapteaza:
-- GRANT EXECUTE ON PKG_REPL_REMOTE TO <user_link>;


-- ----------------------------------------------------------------------------
-- PAS 2.4: Triggerele de sincronizare EU -> AM
-- ----------------------------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_SYNC_ARTIST_EU_TO_AM
AFTER INSERT OR UPDATE OR DELETE ON ARTIST_EU
FOR EACH ROW
BEGIN
    IF NOT PKG_REPL_CTRL.v_replicating THEN
        IF INSERTING THEN
            ARTGALLERY_AM.PKG_REPL_REMOTE.insert_artist@link_am(
                :NEW.artist_id, :NEW.name, :NEW.nationality,
                :NEW.birth_year, :NEW.death_year);
        ELSIF UPDATING THEN
            ARTGALLERY_AM.PKG_REPL_REMOTE.update_artist@link_am(
                :OLD.artist_id, :NEW.name, :NEW.nationality,
                :NEW.birth_year, :NEW.death_year);
        ELSIF DELETING THEN
            ARTGALLERY_AM.PKG_REPL_REMOTE.delete_artist@link_am(:OLD.artist_id);
        END IF;
    END IF;
END;
/

CREATE OR REPLACE TRIGGER TRG_SYNC_COLLECTION_EU_TO_AM
AFTER INSERT OR UPDATE OR DELETE ON COLLECTION_EU
FOR EACH ROW
BEGIN
    IF NOT PKG_REPL_CTRL.v_replicating THEN
        IF INSERTING THEN
            ARTGALLERY_AM.PKG_REPL_REMOTE.insert_collection@link_am(
                :NEW.collection_id, :NEW.name, :NEW.description, :NEW.created_date);
        ELSIF UPDATING THEN
            ARTGALLERY_AM.PKG_REPL_REMOTE.update_collection@link_am(
                :OLD.collection_id, :NEW.name, :NEW.description, :NEW.created_date);
        ELSIF DELETING THEN
            ARTGALLERY_AM.PKG_REPL_REMOTE.delete_collection@link_am(:OLD.collection_id);
        END IF;
    END IF;
END;
/


-- ----------------------------------------------------------------------------
-- PAS 2.5: Verificare obiecte create pe DB2
-- ----------------------------------------------------------------------------
SELECT object_name, object_type, status
FROM   USER_OBJECTS
WHERE  object_name IN ('PKG_REPL_CTRL', 'PKG_REPL_REMOTE',
                       'TRG_SYNC_ARTIST_EU_TO_AM', 'TRG_SYNC_COLLECTION_EU_TO_AM')
ORDER BY object_type, object_name;

-- Toate trebuie sa aiba STATUS = VALID






-- ============================================================================
-- PARTEA III - DB1 (ORCLPDB)
-- Conectare: sqlplus ARTGALLERY_AM/parola_am@//localhost:1521/ORCLPDB
-- (sau in SQL Developer: conexiune ARTGALLERY_AM @ ORCLPDB)
-- ============================================================================

ALTER SESSION SET CONTAINER = ORCLPDB;

-- ----------------------------------------------------------------------------
-- PAS 3.1: Triggerele de sincronizare AM -> EU
-- Apeleaza procedurile remote pe DB2; triggerele EU se vor inhiba prin flag.
-- ----------------------------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_SYNC_ARTIST_AM_TO_EU
AFTER INSERT OR UPDATE OR DELETE ON ARTIST_AM
FOR EACH ROW
BEGIN
    IF NOT PKG_REPL_CTRL.v_replicating THEN
        IF INSERTING THEN
            ARTGALLERY_EU.PKG_REPL_REMOTE.insert_artist@link_eu(
                :NEW.artist_id, :NEW.name, :NEW.nationality,
                :NEW.birth_year, :NEW.death_year);
        ELSIF UPDATING THEN
            ARTGALLERY_EU.PKG_REPL_REMOTE.update_artist@link_eu(
                :OLD.artist_id, :NEW.name, :NEW.nationality,
                :NEW.birth_year, :NEW.death_year);
        ELSIF DELETING THEN
            ARTGALLERY_EU.PKG_REPL_REMOTE.delete_artist@link_eu(:OLD.artist_id);
        END IF;
    END IF;
END;
/

CREATE OR REPLACE TRIGGER TRG_SYNC_COLLECTION_AM_TO_EU
AFTER INSERT OR UPDATE OR DELETE ON COLLECTION_AM
FOR EACH ROW
BEGIN
    IF NOT PKG_REPL_CTRL.v_replicating THEN
        IF INSERTING THEN
            ARTGALLERY_EU.PKG_REPL_REMOTE.insert_collection@link_eu(
                :NEW.collection_id, :NEW.name, :NEW.description, :NEW.created_date);
        ELSIF UPDATING THEN
            ARTGALLERY_EU.PKG_REPL_REMOTE.update_collection@link_eu(
                :OLD.collection_id, :NEW.name, :NEW.description, :NEW.created_date);
        ELSIF DELETING THEN
            ARTGALLERY_EU.PKG_REPL_REMOTE.delete_collection@link_eu(:OLD.collection_id);
        END IF;
    END IF;
END;
/


-- ----------------------------------------------------------------------------
-- PAS 3.2: Verificare obiecte create pe DB1
-- ----------------------------------------------------------------------------
SELECT object_name, object_type, status
FROM   USER_OBJECTS
WHERE  object_name IN ('PKG_REPL_CTRL', 'PKG_REPL_REMOTE',
                       'TRG_SYNC_ARTIST_AM_TO_EU', 'TRG_SYNC_COLLECTION_AM_TO_EU')
ORDER BY object_type, object_name;

-- Toate trebuie sa aiba STATUS = VALID













-- ============================================================================
-- PARTEA IV - TESTE DE SINCRONIZARE
-- ============================================================================
-- Testele alterneaza intre cele doua statii. Atentie la ALTER SESSION
-- si la userul cu care esti conectat in fiecare bloc.
-- ============================================================================


-- ----------------------------------------------------------------------------
-- TEST 1: INSERT pe AM -> trebuie sa apara automat pe EU
-- Conectare: ARTGALLERY_AM @ ORCLPDB
-- ----------------------------------------------------------------------------
ALTER SESSION SET CONTAINER = ORCLPDB;

INSERT INTO ARTIST_AM (artist_id, name, nationality, birth_year, death_year)
VALUES (99, 'Constantin Brancusi', 'Romanian', 1876, 1957);
COMMIT;

-- Verificare locala (trebuie 1 rand)
SELECT artist_id, name, nationality FROM ARTIST_AM WHERE artist_id = 99;

-- Verificare pe EU prin DB link (trebuie 1 rand, identic)
SELECT artist_id, name, nationality
FROM   ARTGALLERY_EU.ARTIST_EU@link_eu
WHERE  artist_id = 99;


-- ----------------------------------------------------------------------------
-- TEST 2: UPDATE pe AM -> trebuie sa se actualizeze si pe EU
-- Conectare: ARTGALLERY_AM @ ORCLPDB
-- ----------------------------------------------------------------------------
UPDATE ARTIST_AM SET nationality = 'Romanian-French' WHERE artist_id = 99;
COMMIT;

-- Verificare pe EU (trebuie 'Romanian-French')
SELECT artist_id, nationality
FROM   ARTGALLERY_EU.ARTIST_EU@link_eu
WHERE  artist_id = 99;


-- ----------------------------------------------------------------------------
-- TEST 3: DELETE pe AM -> trebuie sa dispara si de pe EU
-- Conectare: ARTGALLERY_AM @ ORCLPDB
-- ----------------------------------------------------------------------------
DELETE FROM ARTIST_AM WHERE artist_id = 99;
COMMIT;

-- Verificare pe EU (trebuie 0 randuri pe ambele)
SELECT COUNT(*) AS nr_pe_am FROM ARTIST_AM WHERE artist_id = 99;
SELECT COUNT(*) AS nr_pe_eu
FROM   ARTGALLERY_EU.ARTIST_EU@link_eu WHERE artist_id = 99;


-- ----------------------------------------------------------------------------
-- TEST 4: INSERT pe EU -> trebuie sa apara automat pe AM
-- Conectare: ARTGALLERY_EU @ ORCLPDB2
-- ----------------------------------------------------------------------------
ALTER SESSION SET CONTAINER = ORCLPDB2;

INSERT INTO ARTIST_EU (artist_id, name, nationality, birth_year, death_year)
VALUES (98, 'Michelangelo', 'Italian', 1475, 1564);
COMMIT;

-- Verificare locala (trebuie 1 rand)
SELECT artist_id, name FROM ARTIST_EU WHERE artist_id = 98;

-- Verificare pe AM prin DB link (trebuie 1 rand)
SELECT artist_id, name
FROM   ARTGALLERY_AM.ARTIST_AM@link_am
WHERE  artist_id = 98;


-- ----------------------------------------------------------------------------
-- TEST 5: UPDATE pe EU -> trebuie sa se actualizeze si pe AM
-- Conectare: ARTGALLERY_EU @ ORCLPDB2
-- ----------------------------------------------------------------------------
UPDATE ARTIST_EU SET nationality = 'Italian-Renaissance' WHERE artist_id = 98;
COMMIT;

-- Verificare pe AM
SELECT artist_id, nationality
FROM   ARTGALLERY_AM.ARTIST_AM@link_am
WHERE  artist_id = 98;


-- ----------------------------------------------------------------------------
-- TEST 6: DELETE pe EU -> trebuie sa dispara si de pe AM
-- Conectare: ARTGALLERY_EU @ ORCLPDB2
-- ----------------------------------------------------------------------------
DELETE FROM ARTIST_EU WHERE artist_id = 98;
COMMIT;

SELECT COUNT(*) AS nr_pe_eu FROM ARTIST_EU WHERE artist_id = 98;
SELECT COUNT(*) AS nr_pe_am
FROM   ARTGALLERY_AM.ARTIST_AM@link_am WHERE artist_id = 98;


-- ----------------------------------------------------------------------------
-- TEST 7: COLLECTION - INSERT pe EU -> apare pe AM
-- Conectare: ARTGALLERY_EU @ ORCLPDB2
-- ----------------------------------------------------------------------------
INSERT INTO COLLECTION_EU (collection_id, name, description, created_date)
VALUES (99, 'Test Collection EU', 'Colectie test sincronizare', SYSDATE);
COMMIT;

-- Verificare pe AM
SELECT collection_id, name
FROM   ARTGALLERY_AM.COLLECTION_AM@link_am
WHERE  collection_id = 99;

-- Curatare
DELETE FROM COLLECTION_EU WHERE collection_id = 99;
COMMIT;

-- Verificare ambele 0
SELECT COUNT(*) AS nr_pe_eu FROM COLLECTION_EU WHERE collection_id = 99;
SELECT COUNT(*) AS nr_pe_am
FROM   ARTGALLERY_AM.COLLECTION_AM@link_am WHERE collection_id = 99;


-- ----------------------------------------------------------------------------
-- TEST 8: COLLECTION - INSERT + UPDATE + DELETE pe AM
-- Conectare: ARTGALLERY_AM @ ORCLPDB
-- ----------------------------------------------------------------------------
ALTER SESSION SET CONTAINER = ORCLPDB;

INSERT INTO COLLECTION_AM (collection_id, name, description, created_date)
VALUES (88, 'Modern Sculpture', 'Colectie sculpturi moderne', SYSDATE);
COMMIT;

-- Verificare aparitie pe EU
SELECT collection_id, name FROM ARTGALLERY_EU.COLLECTION_EU@link_eu
WHERE collection_id = 88;

UPDATE COLLECTION_AM SET description = 'Updated description' WHERE collection_id = 88;
COMMIT;

-- Verificare update pe EU
SELECT description FROM ARTGALLERY_EU.COLLECTION_EU@link_eu WHERE collection_id = 88;

DELETE FROM COLLECTION_AM WHERE collection_id = 88;
COMMIT;

-- Verificare disparitie pe EU
SELECT COUNT(*) FROM ARTGALLERY_EU.COLLECTION_EU@link_eu WHERE collection_id = 88;



-- ============================================================================
-- PARTEA IV - VERIFICARE FINALA STARE TRIGGERE SI PACHETE
-- ============================================================================
-- Rulat pe fiecare statie pentru a confirma ca tot e in regula.
-- ============================================================================

-- Pe DB1 (ORCLPDB) ca ARTGALLERY_AM
ALTER SESSION SET CONTAINER = ORCLPDB;

SELECT trigger_name, table_name, triggering_event, status
FROM   USER_TRIGGERS
WHERE  trigger_name LIKE 'TRG_SYNC_%'
ORDER BY trigger_name;

SELECT object_name, object_type, status
FROM   USER_OBJECTS
WHERE  object_name IN ('PKG_REPL_CTRL', 'PKG_REPL_REMOTE')
ORDER BY object_name, object_type;


-- Pe DB2 (ORCLPDB2) ca ARTGALLERY_EU
ALTER SESSION SET CONTAINER = ORCLPDB2;

SELECT trigger_name, table_name, triggering_event, status
FROM   USER_TRIGGERS
WHERE  trigger_name LIKE 'TRG_SYNC_%'
ORDER BY trigger_name;

SELECT object_name, object_type, status
FROM   USER_OBJECTS
WHERE  object_name IN ('PKG_REPL_CTRL', 'PKG_REPL_REMOTE')
ORDER BY object_name, object_type;