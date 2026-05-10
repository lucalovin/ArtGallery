-- ============================================================
-- MODULUL 2 - CERINTA 6: Constrangeri de integritate
-- Galeria de Arta - Baze de Date Distribuite
-- ============================================================
-- Cerinta 6 (2p): Asigurarea tuturor constrangerilor de integritate
-- folosite in model (atat la nivel local, cat si la nivel global) - OBLIGATORIU
--
-- Tipuri de constrangeri implementate:
--   6.A  Unicitate locala       (PRIMARY KEY pe fiecare statie)
--   6.B  Unicitate globala pe fragmente orizontale (triggere cross-db)
--   6.C  Unicitate globala pe fragmente verticale  (garantata structural)
--   6.D  Cheie primara locala si globala
--   6.E  Cheie externa locala   (FK in cadrul aceleiasi statii)
--   6.F  Cheie externa globala  (FK cross-database via triggere BEFORE)
--   6.G  Validare               (CHECK constraints + triggere validare globala)
-- ============================================================


-- ============================================================
-- 6.A  UNICITATE LOCALA
-- ============================================================
-- Unicitatea locala este asigurata prin PRIMARY KEY definite la
-- crearea tabelelor in scripturile 3_artgallery_am.sql,
-- 4_artgallery_eu.sql si 5_artgallery_global.sql.
--
-- ARTGALLERY_AM (DB1 / ORCLPDB):
--   EXHIBITOR_AM          PRIMARY KEY (exhibitor_id)
--   EXHIBITION_AM         PRIMARY KEY (exhibition_id)
--   ARTWORK_DETAILS       PRIMARY KEY (artwork_id)
--   ARTWORK_EXHIBITION_AM PRIMARY KEY (artwork_id, exhibition_id)
--   LOAN_AM               PRIMARY KEY (loan_id)
--   GALLERY_REVIEW_AM     PRIMARY KEY (review_id)
--   ARTIST_AM             PRIMARY KEY (artist_id)
--   COLLECTION_AM         PRIMARY KEY (collection_id)
--
-- ARTGALLERY_EU (DB2 / ORCLPDB2):
--   EXHIBITOR_EU          PRIMARY KEY (exhibitor_id)
--   EXHIBITION_EU         PRIMARY KEY (exhibition_id)
--   ARTWORK_CORE          PRIMARY KEY (artwork_id)
--   ARTWORK_EXHIBITION_EU PRIMARY KEY (artwork_id, exhibition_id)
--   LOAN_EU               PRIMARY KEY (loan_id)
--   GALLERY_REVIEW_EU     PRIMARY KEY (review_id)
--   ARTIST_EU             PRIMARY KEY (artist_id)
--   COLLECTION_EU         PRIMARY KEY (collection_id)
--
-- ARTGALLERY_GLOBAL (DB1 / ORCLPDB):
--   LOCATION              PRIMARY KEY (location_id)
--   VISITOR               PRIMARY KEY (visitor_id)
--   STAFF                 PRIMARY KEY (staff_id)
--   INSURANCE_POLICY      PRIMARY KEY (policy_id)
--   INSURANCE             PRIMARY KEY (insurance_id)
--   RESTORATION           PRIMARY KEY (restoration_id)
--   ACQUISITION           PRIMARY KEY (acquisition_id), UNIQUE (artwork_id)
-- ============================================================

-- Verificare constrangeri de unicitate locale existente
-- (Rulat pe ARTGALLERY_AM / ORCLPDB)
SELECT constraint_name, table_name, constraint_type
FROM   user_constraints
WHERE  constraint_type IN ('P', 'U')
ORDER BY table_name, constraint_type;

-- Test violare PRIMARY KEY locala (returneaza ORA-00001)
-- INSERT INTO EXHIBITOR_AM (exhibitor_id, name, address, city, contact_info)
-- VALUES (1, 'Duplicate', 'Test', 'New York', 'test@test.com');
-- --> ORA-00001: unique constraint (ARTGALLERY_AM.SYS_C...) violated


-- ============================================================
-- 6.B  UNICITATE GLOBALA PE FRAGMENTE ORIZONTALE
-- ============================================================
-- Problema: EXHIBITOR_AM si EXHIBITOR_EU pot primi acelasi exhibitor_id
-- daca inserturile se fac independent pe fiecare statie.
-- Solutie: Triggere BEFORE INSERT care verifica absenta ID-ului
-- in fragmentul corespondent de pe cealalta statie.
--
-- Relatii fragmentate orizontal:
--   EXHIBITOR     = EXHIBITOR_AM (city='New York') U EXHIBITOR_EU (city EU)
--   EXHIBITION    = EXHIBITION_AM U EXHIBITION_EU
--   LOAN          = LOAN_AM U LOAN_EU
--   GALLERY_REVIEW= GALLERY_REVIEW_AM U GALLERY_REVIEW_EU
-- ============================================================

-- ----------------------------------------------------------
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_AM
-- Conectare: sqlplus ARTGALLERY_AM/parola_am@//localhost:1521/ORCLPDB
-- ----------------------------------------------------------

-- Trigger: unicitate globala exhibitor_id la INSERT pe AM
-- Verifica ca ID-ul nu exista deja pe fragmentul EU
CREATE OR REPLACE TRIGGER TRG_UNIQ_GLOBAL_EXHIBITOR_AM
BEFORE INSERT ON EXHIBITOR_AM
FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    -- Cautam exhibitor_id-ul in fragmentul de pe cealalta statie (EU)
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_EU.EXHIBITOR_EU@link_eu
    WHERE  exhibitor_id = :NEW.exhibitor_id;

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(-20010,
            'Unicitate globala incalcata: exhibitor_id = ' || :NEW.exhibitor_id
            || ' exista deja pe statia EU.');
    END IF;
END TRG_UNIQ_GLOBAL_EXHIBITOR_AM;
/

-- Trigger: unicitate globala exhibition_id la INSERT pe AM
CREATE OR REPLACE TRIGGER TRG_UNIQ_GLOBAL_EXHIBITION_AM
BEFORE INSERT ON EXHIBITION_AM
FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_EU.EXHIBITION_EU@link_eu
    WHERE  exhibition_id = :NEW.exhibition_id;

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(-20011,
            'Unicitate globala incalcata: exhibition_id = ' || :NEW.exhibition_id
            || ' exista deja pe statia EU.');
    END IF;
END TRG_UNIQ_GLOBAL_EXHIBITION_AM;
/

-- Trigger: unicitate globala loan_id la INSERT pe AM
CREATE OR REPLACE TRIGGER TRG_UNIQ_GLOBAL_LOAN_AM
BEFORE INSERT ON LOAN_AM
FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_EU.LOAN_EU@link_eu
    WHERE  loan_id = :NEW.loan_id;

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(-20012,
            'Unicitate globala incalcata: loan_id = ' || :NEW.loan_id
            || ' exista deja pe statia EU.');
    END IF;
END TRG_UNIQ_GLOBAL_LOAN_AM;
/

-- Trigger: unicitate globala review_id la INSERT pe AM
CREATE OR REPLACE TRIGGER TRG_UNIQ_GLOBAL_REVIEW_AM
BEFORE INSERT ON GALLERY_REVIEW_AM
FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_EU.GALLERY_REVIEW_EU@link_eu
    WHERE  review_id = :NEW.review_id;

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(-20013,
            'Unicitate globala incalcata: review_id = ' || :NEW.review_id
            || ' exista deja pe statia EU.');
    END IF;
END TRG_UNIQ_GLOBAL_REVIEW_AM;
/

-- ----------------------------------------------------------
-- Rulat pe DB2 (ORCLPDB2) ca ARTGALLERY_EU
-- Conectare: sqlplus ARTGALLERY_EU/parola_eu@//localhost:1521/ORCLPDB2
-- ----------------------------------------------------------

-- Trigger: unicitate globala exhibitor_id la INSERT pe EU
-- Verifica ca ID-ul nu exista deja pe fragmentul AM
CREATE OR REPLACE TRIGGER TRG_UNIQ_GLOBAL_EXHIBITOR_EU
BEFORE INSERT ON EXHIBITOR_EU
FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_AM.EXHIBITOR_AM@link_am
    WHERE  exhibitor_id = :NEW.exhibitor_id;

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(-20010,
            'Unicitate globala incalcata: exhibitor_id = ' || :NEW.exhibitor_id
            || ' exista deja pe statia AM.');
    END IF;
END TRG_UNIQ_GLOBAL_EXHIBITOR_EU;
/

-- Trigger: unicitate globala exhibition_id la INSERT pe EU
CREATE OR REPLACE TRIGGER TRG_UNIQ_GLOBAL_EXHIBITION_EU
BEFORE INSERT ON EXHIBITION_EU
FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_AM.EXHIBITION_AM@link_am
    WHERE  exhibition_id = :NEW.exhibition_id;

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(-20011,
            'Unicitate globala incalcata: exhibition_id = ' || :NEW.exhibition_id
            || ' exista deja pe statia AM.');
    END IF;
END TRG_UNIQ_GLOBAL_EXHIBITION_EU;
/

-- Trigger: unicitate globala loan_id la INSERT pe EU
CREATE OR REPLACE TRIGGER TRG_UNIQ_GLOBAL_LOAN_EU
BEFORE INSERT ON LOAN_EU
FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_AM.LOAN_AM@link_am
    WHERE  loan_id = :NEW.loan_id;

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(-20012,
            'Unicitate globala incalcata: loan_id = ' || :NEW.loan_id
            || ' exista deja pe statia AM.');
    END IF;
END TRG_UNIQ_GLOBAL_LOAN_EU;
/

-- Trigger: unicitate globala review_id la INSERT pe EU
CREATE OR REPLACE TRIGGER TRG_UNIQ_GLOBAL_REVIEW_EU
BEFORE INSERT ON GALLERY_REVIEW_EU
FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_AM.GALLERY_REVIEW_AM@link_am
    WHERE  review_id = :NEW.review_id;

    IF v_cnt > 0 THEN
        RAISE_APPLICATION_ERROR(-20013,
            'Unicitate globala incalcata: review_id = ' || :NEW.review_id
            || ' exista deja pe statia AM.');
    END IF;
END TRG_UNIQ_GLOBAL_REVIEW_EU;
/


-- ============================================================
-- 6.C  UNICITATE GLOBALA PE FRAGMENTE VERTICALE (ARTWORK)
-- ============================================================
-- Fragmentarea verticala a ARTWORK:
--   ARTWORK_CORE    (EU) = artwork_id, title, artist_id, year_created, medium, collection_id
--   ARTWORK_DETAILS (AM) = artwork_id, location_id, estimated_value
--
-- Unicitatea artwork_id este garantata structural prin:
--   1. PRIMARY KEY pe ARTWORK_CORE (EU)  -> unicitate locala pe EU
--   2. PRIMARY KEY pe ARTWORK_DETAILS (AM) -> unicitate locala pe AM
--   3. Triggerul t_artwork din Cerinta4 insereaza SIMULTAN in ambele fragmente
--      -> nu pot exista ID-uri diferite intre cele doua jumatati
--
-- Verificare consistenta: artwork_id fara corespondent in celalalt fragment
-- (Rulat pe ARTGALLERY_AM sau ARTGALLERY_GLOBAL cu drepturi pe ambele)

-- Artwork_id din EU fara corespondent in AM (trebuie sa returneze 0 randuri)
SELECT c.artwork_id, c.title, 'Lipsa in ARTWORK_DETAILS (AM)' AS problema
FROM   ARTGALLERY_EU.ARTWORK_CORE@link_eu c
WHERE  c.artwork_id NOT IN (
    SELECT d.artwork_id FROM ARTGALLERY_AM.ARTWORK_DETAILS@link_am d
);

-- Artwork_id din AM fara corespondent in EU (trebuie sa returneze 0 randuri)
SELECT d.artwork_id, 'Lipsa in ARTWORK_CORE (EU)' AS problema
FROM   ARTGALLERY_AM.ARTWORK_DETAILS@link_am d
WHERE  d.artwork_id NOT IN (
    SELECT c.artwork_id FROM ARTGALLERY_EU.ARTWORK_CORE@link_eu c
);


-- ============================================================
-- 6.D  CHEIE PRIMARA LOCALA SI GLOBALA
-- ============================================================
-- La nivel LOCAL: PRIMARY KEY sunt definite in scripturile de creare tabele.
-- La nivel GLOBAL: unicitatea globala = PK locale + triggerele din 6.B.
-- Un INSERT care respecta PK locala dar violeaza unicitatea globala
-- este blocat de triggerele TRG_UNIQ_GLOBAL_*.

-- Demonstrare violare PK locala (ORA-00001):
-- (Rulat pe ARTGALLERY_AM)
-- INSERT INTO EXHIBITOR_AM (exhibitor_id, name, address, city, contact_info)
-- VALUES (1, 'Duplicate MoMA', '11 W 53rd St', 'New York', 'dup@moma.org');
-- --> ORA-00001: unique constraint violated

-- Demonstrare violare unicitate globala (prin trigger 6.B):
-- Presupunem ca exhibitor_id=1 exista pe EU (Louvre, Paris).
-- Incercam sa inseram exhibitor_id=1 pe AM:
-- INSERT INTO EXHIBITOR_AM (exhibitor_id, name, address, city, contact_info)
-- VALUES (1, 'Fake Louvre', '5th Ave', 'New York', 'fake@louvre.fr');
-- --> ORA-20010: Unicitate globala incalcata: exhibitor_id = 1 exista deja pe statia EU.

-- Verificare constrangeri PK definite pe AM:
SELECT constraint_name, table_name, constraint_type
FROM   user_constraints
WHERE  constraint_type = 'P'
ORDER BY table_name;


-- ============================================================
-- 6.E  CHEIE EXTERNA LOCALA
-- ============================================================
-- FK-urile locale (in cadrul aceleiasi statii) sunt definite la crearea tabelelor.
-- Oracle le verifica automat la INSERT/UPDATE/DELETE.
--
-- Pe ARTGALLERY_AM (ORCLPDB):
--   EXHIBITION_AM.exhibitor_id       -> EXHIBITOR_AM.exhibitor_id
--   ARTWORK_EXHIBITION_AM.artwork_id -> ARTWORK_DETAILS.artwork_id
--   ARTWORK_EXHIBITION_AM.exhibition_id -> EXHIBITION_AM.exhibition_id
--   LOAN_AM.artwork_id               -> ARTWORK_DETAILS.artwork_id
--   LOAN_AM.exhibitor_id             -> EXHIBITOR_AM.exhibitor_id
--   GALLERY_REVIEW_AM.artwork_id     -> ARTWORK_DETAILS.artwork_id
--   GALLERY_REVIEW_AM.exhibition_id  -> EXHIBITION_AM.exhibition_id
--
-- Pe ARTGALLERY_EU (ORCLPDB2):
--   EXHIBITION_EU.exhibitor_id       -> EXHIBITOR_EU.exhibitor_id
--   ARTWORK_EXHIBITION_EU.artwork_id -> ARTWORK_CORE.artwork_id
--   ARTWORK_EXHIBITION_EU.exhibition_id -> EXHIBITION_EU.exhibition_id
--   LOAN_EU.artwork_id               -> ARTWORK_CORE.artwork_id
--   LOAN_EU.exhibitor_id             -> EXHIBITOR_EU.exhibitor_id
--   GALLERY_REVIEW_EU.artwork_id     -> ARTWORK_CORE.artwork_id
--   GALLERY_REVIEW_EU.exhibition_id  -> EXHIBITION_EU.exhibition_id
--
-- Pe ARTGALLERY_GLOBAL (ORCLPDB):
--   INSURANCE.policy_id              -> INSURANCE_POLICY.policy_id
--   RESTORATION.staff_id             -> STAFF.staff_id
--   ACQUISITION.staff_id             -> STAFF.staff_id

-- Verificare FK-uri locale definite pe statie:
-- (Rulat pe oricare statie)
SELECT constraint_name, table_name, r_constraint_name
FROM   user_constraints
WHERE  constraint_type = 'R'
ORDER BY table_name;


-- ============================================================
-- 6.F  CHEIE EXTERNA GLOBALA (cross-database via triggere)
-- ============================================================
-- Oracle NU suporta FOREIGN KEY catre tabele din alta baza de date.
-- Solutie: triggere BEFORE INSERT/UPDATE care verifica referinta
--          prin DB Link inainte de a permite operatia.
--
-- Constrangeri FK globale implementate:
--   F1: GALLERY_REVIEW_AM.visitor_id   -> VISITOR (ARTGALLERY_GLOBAL/DB1)
--   F2: GALLERY_REVIEW_EU.visitor_id   -> VISITOR (ARTGALLERY_GLOBAL/DB1)
--   F3: ARTWORK_DETAILS.location_id    -> LOCATION (ARTGALLERY_GLOBAL/DB1)
--   F4: INSURANCE.artwork_id           -> ARTWORK_CORE (ARTGALLERY_EU/DB2)
--   F5: RESTORATION.artwork_id         -> ARTWORK_CORE (ARTGALLERY_EU/DB2)
--   F6: ACQUISITION.artwork_id         -> ARTWORK_CORE (ARTGALLERY_EU/DB2)
-- ============================================================

-- ----------------------------------------------------------
-- F1: GALLERY_REVIEW_AM.visitor_id -> VISITOR@link_global
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_AM
-- NOTA: Definit deja in triggere.sql ca TRG_FK_REVIEW_VISITOR_GLOBAL.
-- Redefinit explicit aici pentru documentatie completa.
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_FK_REVIEW_VISITOR_GLOBAL
BEFORE INSERT OR UPDATE OF visitor_id ON GALLERY_REVIEW_AM
FOR EACH ROW
DECLARE
    v_exists NUMBER;
BEGIN
    -- Verificam ca vizitatorul exista in tabela VISITOR de pe statie globala
    SELECT COUNT(*) INTO v_exists
    FROM   ARTGALLERY_GLOBAL.VISITOR@link_global
    WHERE  visitor_id = :NEW.visitor_id;

    IF v_exists = 0 THEN
        RAISE_APPLICATION_ERROR(-20020,
            'FK global incalcat: visitor_id = ' || :NEW.visitor_id
            || ' nu exista in VISITOR (ARTGALLERY_GLOBAL).');
    END IF;
END TRG_FK_REVIEW_VISITOR_GLOBAL;
/

-- ----------------------------------------------------------
-- F2: GALLERY_REVIEW_EU.visitor_id -> VISITOR@link_global
-- Rulat pe DB2 (ORCLPDB2) ca ARTGALLERY_EU
-- link_global conecteaza DB2 la ARTGALLERY_GLOBAL pe DB1
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_FK_REVIEW_EU_VISITOR
BEFORE INSERT OR UPDATE OF visitor_id ON GALLERY_REVIEW_EU
FOR EACH ROW
DECLARE
    v_exists NUMBER;
BEGIN
    -- Verificam ca vizitatorul exista pe statie globala (accesata via link_global)
    SELECT COUNT(*) INTO v_exists
    FROM   VISITOR@link_global
    WHERE  visitor_id = :NEW.visitor_id;

    IF v_exists = 0 THEN
        RAISE_APPLICATION_ERROR(-20020,
            'FK global incalcat: visitor_id = ' || :NEW.visitor_id
            || ' nu exista in VISITOR (ARTGALLERY_GLOBAL).');
    END IF;
END TRG_FK_REVIEW_EU_VISITOR;
/

-- ----------------------------------------------------------
-- F3: ARTWORK_DETAILS.location_id -> LOCATION@link_global
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_AM
-- NOTA: Definit deja in triggere.sql ca TRG_FK_ARTWORK_LOCATION_GLOBAL.
-- Redefinit explicit aici pentru documentatie completa.
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_FK_ARTWORK_LOCATION_GLOBAL
BEFORE INSERT OR UPDATE OF location_id ON ARTWORK_DETAILS
FOR EACH ROW
DECLARE
    v_exists NUMBER;
BEGIN
    IF :NEW.location_id IS NOT NULL THEN
        -- Verificam ca locatia exista in LOCATION de pe statie globala
        SELECT COUNT(*) INTO v_exists
        FROM   ARTGALLERY_GLOBAL.LOCATION@link_global
        WHERE  location_id = :NEW.location_id;

        IF v_exists = 0 THEN
            RAISE_APPLICATION_ERROR(-20021,
                'FK global incalcat: location_id = ' || :NEW.location_id
                || ' nu exista in LOCATION (ARTGALLERY_GLOBAL).');
        END IF;
    END IF;
END TRG_FK_ARTWORK_LOCATION_GLOBAL;
/

-- ----------------------------------------------------------
-- F4: INSURANCE.artwork_id -> ARTWORK_CORE@link_eu
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_GLOBAL
-- Conectare: sqlplus ARTGALLERY_GLOBAL/parola_global@//localhost:1521/ORCLPDB
-- INSURANCE este pe ARTGALLERY_GLOBAL, dar artwork_id trebuie sa existe
-- in ARTWORK_CORE care e pe ARTGALLERY_EU (DB2) - fragment vertical
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_FK_INSURANCE_ARTWORK
BEFORE INSERT OR UPDATE OF artwork_id ON INSURANCE
FOR EACH ROW
DECLARE
    v_exists NUMBER;
BEGIN
    -- Verificam ca opera de arta exista in fragmentul vertical ARTWORK_CORE (EU)
    SELECT COUNT(*) INTO v_exists
    FROM   ARTGALLERY_EU.ARTWORK_CORE@link_eu
    WHERE  artwork_id = :NEW.artwork_id;

    IF v_exists = 0 THEN
        RAISE_APPLICATION_ERROR(-20022,
            'FK global incalcat: artwork_id = ' || :NEW.artwork_id
            || ' nu exista in ARTWORK_CORE (ARTGALLERY_EU).');
    END IF;
END TRG_FK_INSURANCE_ARTWORK;
/

-- ----------------------------------------------------------
-- F5: RESTORATION.artwork_id -> ARTWORK_CORE@link_eu
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_GLOBAL
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_FK_RESTORATION_ARTWORK
BEFORE INSERT OR UPDATE OF artwork_id ON RESTORATION
FOR EACH ROW
DECLARE
    v_exists NUMBER;
BEGIN
    -- Verificam ca opera de arta supusa restaurarii exista in ARTWORK_CORE (EU)
    SELECT COUNT(*) INTO v_exists
    FROM   ARTGALLERY_EU.ARTWORK_CORE@link_eu
    WHERE  artwork_id = :NEW.artwork_id;

    IF v_exists = 0 THEN
        RAISE_APPLICATION_ERROR(-20023,
            'FK global incalcat: artwork_id = ' || :NEW.artwork_id
            || ' nu exista in ARTWORK_CORE (ARTGALLERY_EU).');
    END IF;
END TRG_FK_RESTORATION_ARTWORK;
/

-- ----------------------------------------------------------
-- F6: ACQUISITION.artwork_id -> ARTWORK_CORE@link_eu
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_GLOBAL
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_FK_ACQUISITION_ARTWORK
BEFORE INSERT OR UPDATE OF artwork_id ON ACQUISITION
FOR EACH ROW
DECLARE
    v_exists NUMBER;
BEGIN
    -- Verificam ca opera de arta achizitionata exista in ARTWORK_CORE (EU)
    SELECT COUNT(*) INTO v_exists
    FROM   ARTGALLERY_EU.ARTWORK_CORE@link_eu
    WHERE  artwork_id = :NEW.artwork_id;

    IF v_exists = 0 THEN
        RAISE_APPLICATION_ERROR(-20024,
            'FK global incalcat: artwork_id = ' || :NEW.artwork_id
            || ' nu exista in ARTWORK_CORE (ARTGALLERY_EU).');
    END IF;
END TRG_FK_ACQUISITION_ARTWORK;
/


-- ============================================================
-- 6.G  VALIDARE (CHECK CONSTRAINTS + TRIGGERE VALIDARE GLOBALA)
-- ============================================================
-- CHECK constraints locale sunt definite la crearea tabelelor:
--   EXHIBITION_AM/EU:     end_date >= start_date
--   LOAN_AM/EU:           end_date IS NULL OR end_date >= start_date
--   GALLERY_REVIEW_AM/EU: rating BETWEEN 1 AND 5
--   INSURANCE_POLICY:     end_date >= start_date, total_coverage_amount >= 0
--   INSURANCE:            insured_amount > 0
--   RESTORATION:          end_date IS NULL OR end_date >= start_date
--   ACQUISITION:          price IS NULL OR price >= 0
--   LOCATION:             capacity IS NULL OR capacity > 0
--
-- Validari globale (cross-database) implementate prin triggere:
--   G1: insured_amount <= estimated_value * 1.5 (INSURANCE vs ARTWORK_DETAILS)
--   G2: start_date restaurare nu poate fi in viitor (RESTORATION)
-- ============================================================

-- Verificare CHECK constraints locale definite pe statie:
-- (Rulat pe oricare statie)
SELECT constraint_name, table_name, search_condition
FROM   user_constraints
WHERE  constraint_type = 'C'
  AND  constraint_name NOT LIKE 'SYS_%'
ORDER BY table_name;

-- ----------------------------------------------------------
-- G1: Validare globala - suma asigurata nu poate depasi
--     150% din valoarea estimata a operei de arta
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_GLOBAL
-- Implica date din: INSURANCE (GLOBAL) + ARTWORK_DETAILS (AM, via link_am)
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_VALIDATE_INSURANCE_AMOUNT
BEFORE INSERT OR UPDATE OF insured_amount, artwork_id ON INSURANCE
FOR EACH ROW
DECLARE
    v_estimated NUMBER;
BEGIN
    -- Preluam valoarea estimata din fragmentul vertical ARTWORK_DETAILS (AM)
    SELECT estimated_value INTO v_estimated
    FROM   ARTGALLERY_AM.ARTWORK_DETAILS@link_am
    WHERE  artwork_id = :NEW.artwork_id;

    -- Regula de afaceri: suma asigurata nu poate depasi 150% din valoarea estimata
    IF :NEW.insured_amount > v_estimated * 1.5 THEN
        RAISE_APPLICATION_ERROR(-20030,
            'Validare globala: suma asigurata (' || :NEW.insured_amount
            || ') depaseste 150% din valoarea estimata (' || v_estimated || ').');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20031,
            'Validare: artwork_id = ' || :NEW.artwork_id
            || ' nu are valoare estimata in ARTWORK_DETAILS.');
END TRG_VALIDATE_INSURANCE_AMOUNT;
/

-- ----------------------------------------------------------
-- G2: Validare - data de start a restaurarii nu poate fi in viitor
-- Rulat pe DB1 (ORCLPDB) ca ARTGALLERY_GLOBAL
-- ----------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_VALIDATE_RESTORATION_DATE
BEFORE INSERT OR UPDATE OF start_date ON RESTORATION
FOR EACH ROW
BEGIN
    -- Data de inceput a restaurarii trebuie sa fie in trecut sau prezent
    IF :NEW.start_date > SYSDATE THEN
        RAISE_APPLICATION_ERROR(-20032,
            'Validare: data de start a restaurarii (' ||
            TO_CHAR(:NEW.start_date, 'DD-MON-YYYY') ||
            ') nu poate fi in viitor.');
    END IF;
END TRG_VALIDATE_RESTORATION_DATE;
/


-- ============================================================
-- TESTE CONSTRANGERI
-- ============================================================

-- Test 6.A - violare PK locala (ORA-00001)
-- (Rulat pe ARTGALLERY_AM; exhibitor_id=1 = MoMA, exista deja)
-- INSERT INTO EXHIBITOR_AM (exhibitor_id, name, address, city, contact_info)
-- VALUES (1, 'Duplicate', '11 W 53rd St', 'New York', 'dup@moma.org');
-- Rezultat asteptat: ORA-00001: unique constraint violated

-- Test 6.B - violare unicitate globala orizontala
-- (Rulat pe ARTGALLERY_AM; presupunem ca exhibitor_id=1 exista pe EU)
-- INSERT INTO EXHIBITOR_AM (exhibitor_id, name, address, city, contact_info)
-- VALUES (1, 'Louvre Copy', 'Rue Test', 'New York', 'copy@louvre.fr');
-- Rezultat asteptat: ORA-20010: Unicitate globala incalcata: exhibitor_id = 1 exista deja pe statia EU.

-- Test 6.E - violare FK locala
-- (Rulat pe ARTGALLERY_AM; exhibition_id=999 nu exista in EXHIBITOR_AM)
-- INSERT INTO EXHIBITION_AM (exhibition_id, title, start_date, end_date, exhibitor_id)
-- VALUES (99, 'Test', DATE '2024-01-01', DATE '2024-02-01', 9999);
-- Rezultat asteptat: ORA-02291: integrity constraint violated - parent key not found

-- Test 6.F - violare FK global visitor
-- (Rulat pe ARTGALLERY_AM; visitor_id=9999 nu exista in VISITOR/GLOBAL)
-- INSERT INTO GALLERY_REVIEW_AM (review_id, visitor_id, exhibition_id, rating, review_date)
-- VALUES (999, 9999, 1, 3, SYSDATE);
-- Rezultat asteptat: ORA-20020: FK global incalcat: visitor_id = 9999 nu exista in VISITOR.

-- Test 6.G - violare validare globala
-- (Rulat pe ARTGALLERY_GLOBAL; artwork_id=1 are estimated_value=1000000)
-- INSERT INTO INSURANCE (insurance_id, artwork_id, policy_id, insured_amount)
-- VALUES (999, 1, 1, 9000000);
-- Rezultat asteptat: ORA-20030: suma asigurata (9000000) depaseste 150% din valoarea estimata.

-- Test 6.G - violare CHECK local rating
-- (Rulat pe ARTGALLERY_AM)
-- INSERT INTO GALLERY_REVIEW_AM (review_id, visitor_id, exhibition_id, rating, review_date)
-- VALUES (999, 1, 1, 6, SYSDATE);
-- Rezultat asteptat: ORA-02290: check constraint violated (rating BETWEEN 1 AND 5)


-- ============================================================
-- VERIFICARE STARE GENERALA CONSTRANGERI
-- ============================================================

-- Lista tuturor triggerelor de integritate create
SELECT trigger_name, table_name, triggering_event, status
FROM   user_triggers
WHERE  trigger_name LIKE 'TRG_UNIQ_%'
    OR trigger_name LIKE 'TRG_FK_%'
    OR trigger_name LIKE 'TRG_VALIDATE_%'
ORDER BY trigger_name;

-- Lista constrangerilor declarative active pe statie
SELECT constraint_name, constraint_type, table_name, status
FROM   user_constraints
WHERE  status = 'ENABLED'
ORDER BY constraint_type, table_name;
