-- ============================================================
-- MODULUL 2 - CERINTA 5: Sincronizarea datelor pentru relatiile replicate
-- Galeria de Arta - Baze de Date Distribuite
-- ============================================================
-- Cerinta 5 (1p): Asigurarea sincronizarii datelor pentru relatiile replicate
--
-- RELATII REPLICATE (date identice pe ambele statii):
--   ARTIST     -> ARTIST_AM    (ARTGALLERY_AM / ORCLPDB / DB1)
--                ARTIST_EU    (ARTGALLERY_EU / ORCLPDB2 / DB2)
--   COLLECTION -> COLLECTION_AM (ARTGALLERY_AM / ORCLPDB / DB1)
--                COLLECTION_EU (ARTGALLERY_EU / ORCLPDB2 / DB2)
--
-- MECANISM: Triggere AFTER INSERT/UPDATE/DELETE pe fiecare replica
--   propaga modificarea spre cealalta replica prin DB Link.
--
-- PREVENIRE BUCLE INFINITE:
--   Triggerul AM propaga spre EU -> triggerul EU se declanseaza ->
--   incearca sa propaga inapoi spre AM -> triggerul AM s-ar declansa din nou!
--   Solutie: Pachet PL/SQL cu variabila de sesiune (v_replicating).
--   Variabila ramane TRUE pe DB1 chiar cand DB2 apeleaza inapoi DB1
--   in aceeasi tranzactie distribuita -> a doua propagare se opreste automat.
-- ============================================================


-- ============================================================
-- SECTIUNEA 1: Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_AM
-- ============================================================
-- Conectare:
--   sqlplus ARTGALLERY_AM/parola_am@//localhost:1521/ORCLPDB
-- ============================================================

-- Pachet pentru controlul replicarii pe statia AM
-- v_replicating = TRUE inseamna ca suntem deja in mijlocul unei propagari
CREATE OR REPLACE PACKAGE PKG_REPL_CTRL AS
    v_replicating BOOLEAN := FALSE;
END PKG_REPL_CTRL;
/

-- ----------------------------------------------------------
-- TRIGGER: Sincronizare ARTIST AM -> EU
-- Propaga orice modificare din ARTIST_AM spre ARTIST_EU pe DB2
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_SYNC_ARTIST_AM_TO_EU
AFTER INSERT OR UPDATE OR DELETE ON ARTIST_AM
FOR EACH ROW
BEGIN
    -- Daca suntem deja in replicare, nu mai propagam (prevenim bucla)
    IF NOT PKG_REPL_CTRL.v_replicating THEN
        PKG_REPL_CTRL.v_replicating := TRUE;
        BEGIN
            IF INSERTING THEN
                -- Artist nou pe AM -> inseram si pe EU
                INSERT INTO ARTGALLERY_EU.ARTIST_EU@link_eu
                    (artist_id, name, nationality, birth_year, death_year)
                VALUES
                    (:NEW.artist_id, :NEW.name, :NEW.nationality,
                     :NEW.birth_year, :NEW.death_year);

            ELSIF UPDATING THEN
                -- Artist modificat pe AM -> actualizam si pe EU
                UPDATE ARTGALLERY_EU.ARTIST_EU@link_eu
                SET    name        = :NEW.name,
                       nationality = :NEW.nationality,
                       birth_year  = :NEW.birth_year,
                       death_year  = :NEW.death_year
                WHERE  artist_id   = :OLD.artist_id;

            ELSIF DELETING THEN
                -- Artist sters de pe AM -> stergem si de pe EU
                DELETE FROM ARTGALLERY_EU.ARTIST_EU@link_eu
                WHERE  artist_id = :OLD.artist_id;
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                -- Resetam flag-ul chiar si in caz de eroare, apoi re-aruncam
                PKG_REPL_CTRL.v_replicating := FALSE;
                RAISE;
        END;
        PKG_REPL_CTRL.v_replicating := FALSE;
    END IF;
END TRG_SYNC_ARTIST_AM_TO_EU;
/

-- ----------------------------------------------------------
-- TRIGGER: Sincronizare COLLECTION AM -> EU
-- Propaga orice modificare din COLLECTION_AM spre COLLECTION_EU pe DB2
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_SYNC_COLLECTION_AM_TO_EU
AFTER INSERT OR UPDATE OR DELETE ON COLLECTION_AM
FOR EACH ROW
BEGIN
    IF NOT PKG_REPL_CTRL.v_replicating THEN
        PKG_REPL_CTRL.v_replicating := TRUE;
        BEGIN
            IF INSERTING THEN
                -- Colectie noua pe AM -> inseram si pe EU
                INSERT INTO ARTGALLERY_EU.COLLECTION_EU@link_eu
                    (collection_id, name, description, created_date)
                VALUES
                    (:NEW.collection_id, :NEW.name,
                     :NEW.description, :NEW.created_date);

            ELSIF UPDATING THEN
                -- Colectie modificata pe AM -> actualizam si pe EU
                UPDATE ARTGALLERY_EU.COLLECTION_EU@link_eu
                SET    name         = :NEW.name,
                       description  = :NEW.description,
                       created_date = :NEW.created_date
                WHERE  collection_id = :OLD.collection_id;

            ELSIF DELETING THEN
                -- Colectie stearsa de pe AM -> stergem si de pe EU
                DELETE FROM ARTGALLERY_EU.COLLECTION_EU@link_eu
                WHERE  collection_id = :OLD.collection_id;
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                PKG_REPL_CTRL.v_replicating := FALSE;
                RAISE;
        END;
        PKG_REPL_CTRL.v_replicating := FALSE;
    END IF;
END TRG_SYNC_COLLECTION_AM_TO_EU;
/


-- ============================================================
-- SECTIUNEA 2: Rulat pe DB2 (ORCLPDB2) ca ARTGALLERY_EU
-- ============================================================
-- Conectare:
--   sqlplus ARTGALLERY_EU/parola_eu@//localhost:1521/ORCLPDB2
-- ============================================================

-- Pachet anti-bucla pe DB2 (instanta separata, logica identica)
CREATE OR REPLACE PACKAGE PKG_REPL_CTRL AS
    v_replicating BOOLEAN := FALSE;
END PKG_REPL_CTRL;
/

-- ----------------------------------------------------------
-- TRIGGER: Sincronizare ARTIST EU -> AM
-- Propaga modificarile din ARTIST_EU spre ARTIST_AM pe DB1
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_SYNC_ARTIST_EU_TO_AM
AFTER INSERT OR UPDATE OR DELETE ON ARTIST_EU
FOR EACH ROW
BEGIN
    IF NOT PKG_REPL_CTRL.v_replicating THEN
        PKG_REPL_CTRL.v_replicating := TRUE;
        BEGIN
            IF INSERTING THEN
                -- Artist nou pe EU -> inseram si pe AM
                INSERT INTO ARTGALLERY_AM.ARTIST_AM@link_am
                    (artist_id, name, nationality, birth_year, death_year)
                VALUES
                    (:NEW.artist_id, :NEW.name, :NEW.nationality,
                     :NEW.birth_year, :NEW.death_year);

            ELSIF UPDATING THEN
                -- Artist modificat pe EU -> actualizam si pe AM
                UPDATE ARTGALLERY_AM.ARTIST_AM@link_am
                SET    name        = :NEW.name,
                       nationality = :NEW.nationality,
                       birth_year  = :NEW.birth_year,
                       death_year  = :NEW.death_year
                WHERE  artist_id   = :OLD.artist_id;

            ELSIF DELETING THEN
                -- Artist sters de pe EU -> stergem si de pe AM
                DELETE FROM ARTGALLERY_AM.ARTIST_AM@link_am
                WHERE  artist_id = :OLD.artist_id;
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                PKG_REPL_CTRL.v_replicating := FALSE;
                RAISE;
        END;
        PKG_REPL_CTRL.v_replicating := FALSE;
    END IF;
END TRG_SYNC_ARTIST_EU_TO_AM;
/

-- ----------------------------------------------------------
-- TRIGGER: Sincronizare COLLECTION EU -> AM
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_SYNC_COLLECTION_EU_TO_AM
AFTER INSERT OR UPDATE OR DELETE ON COLLECTION_EU
FOR EACH ROW
BEGIN
    IF NOT PKG_REPL_CTRL.v_replicating THEN
        PKG_REPL_CTRL.v_replicating := TRUE;
        BEGIN
            IF INSERTING THEN
                -- Colectie noua pe EU -> inseram si pe AM
                INSERT INTO ARTGALLERY_AM.COLLECTION_AM@link_am
                    (collection_id, name, description, created_date)
                VALUES
                    (:NEW.collection_id, :NEW.name,
                     :NEW.description, :NEW.created_date);

            ELSIF UPDATING THEN
                -- Colectie modificata pe EU -> actualizam si pe AM
                UPDATE ARTGALLERY_AM.COLLECTION_AM@link_am
                SET    name         = :NEW.name,
                       description  = :NEW.description,
                       created_date = :NEW.created_date
                WHERE  collection_id = :OLD.collection_id;

            ELSIF DELETING THEN
                -- Colectie stearsa de pe EU -> stergem si de pe AM
                DELETE FROM ARTGALLERY_AM.COLLECTION_AM@link_am
                WHERE  collection_id = :OLD.collection_id;
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                PKG_REPL_CTRL.v_replicating := FALSE;
                RAISE;
        END;
        PKG_REPL_CTRL.v_replicating := FALSE;
    END IF;
END TRG_SYNC_COLLECTION_EU_TO_AM;
/


-- ============================================================
-- SECTIUNEA 3: TESTE SINCRONIZARE
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_AM
-- ============================================================

-- TEST 1: INSERT pe AM -> trebuie sa apara automat si pe EU
INSERT INTO ARTIST_AM (artist_id, name, nationality, birth_year, death_year)
VALUES (99, 'Constantin Brancusi', 'Romanian', 1876, 1957);
COMMIT;

-- Verificare pe EU prin DB link (trebuie sa returneze 1 rand)
SELECT artist_id, name, nationality
FROM   ARTGALLERY_EU.ARTIST_EU@link_eu
WHERE  artist_id = 99;

-- TEST 2: UPDATE pe AM -> trebuie sa se actualizeze si pe EU
UPDATE ARTIST_AM SET nationality = 'Romanian-French' WHERE artist_id = 99;
COMMIT;

-- Verificare (trebuie sa returneze 'Romanian-French')
SELECT nationality FROM ARTGALLERY_EU.ARTIST_EU@link_eu WHERE artist_id = 99;

-- TEST 3: DELETE pe AM -> trebuie sa dispara si de pe EU
DELETE FROM ARTIST_AM WHERE artist_id = 99;
COMMIT;

-- Verificare (trebuie sa returneze 0)
SELECT COUNT(*) AS nr_pe_eu
FROM   ARTGALLERY_EU.ARTIST_EU@link_eu
WHERE  artist_id = 99;

-- ============================================================
-- Rulat pe DB2 (ORCLPDB2) ca ARTGALLERY_EU
-- ============================================================

-- TEST 4: INSERT pe EU -> trebuie sa apara automat si pe AM
INSERT INTO ARTIST_EU (artist_id, name, nationality, birth_year, death_year)
VALUES (98, 'Michelangelo', 'Italian', 1475, 1564);
COMMIT;

-- Verificare pe AM (trebuie sa returneze 1 rand)
SELECT artist_id, name FROM ARTGALLERY_AM.ARTIST_AM@link_am WHERE artist_id = 98;

-- Curatare test
DELETE FROM ARTIST_EU WHERE artist_id = 98;
COMMIT;

-- TEST 5: Sincronizare COLLECTION - INSERT pe EU -> apare pe AM
INSERT INTO COLLECTION_EU (collection_id, name, description, created_date)
VALUES (99, 'Test Collection EU', 'Colectie test sincronizare', SYSDATE);
COMMIT;

-- Verificare pe AM (trebuie sa returneze 1 rand)
SELECT collection_id, name FROM ARTGALLERY_AM.COLLECTION_AM@link_am WHERE collection_id = 99;

-- Curatare test
DELETE FROM COLLECTION_EU WHERE collection_id = 99;
COMMIT;


-- ============================================================
-- SECTIUNEA 4: VERIFICARE STARE TRIGGERE DE SINCRONIZARE
-- ============================================================
-- Rulat pe oricare statie (afiseaza triggerele statiei curente)

-- Lista triggerelor de sincronizare si statusul lor
SELECT trigger_name, table_name, triggering_event, status
FROM   USER_TRIGGERS
WHERE  trigger_name LIKE 'TRG_SYNC_%'
ORDER BY trigger_name;

-- Verificare pachet de control al replicarii
SELECT object_name, object_type, status
FROM   USER_OBJECTS
WHERE  object_name = 'PKG_REPL_CTRL';
