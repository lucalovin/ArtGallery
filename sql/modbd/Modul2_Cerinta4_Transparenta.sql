-- ============================================================
-- MODULUL 2 – CERINȚA 4: Formele de transparență
-- Galeria de Artă – Baze de Date Distribuite
-- Schema globală: ARTGALLERY_GLOBAL (bdglobal)
-- ============================================================
-- Transparența localizării datelor = utilizatorii accesează
-- obiectele din BD distribuită ca și când ar fi locale.
--
-- Tehnici implementate:
--   4.A  VIEW + INSTEAD OF trigger → fragmente ORIZONTALE (EXHIBITOR, EXHIBITION etc.)
--   4.B  VIEW + INSTEAD OF trigger → fragmente VERTICALE  (ARTWORK)
--   4.C  SINONIME locale              → tabele remote individuale
--   4.D  PROCEDURI stocate            → operații distribuite cu logică de rutare
-- ============================================================
-- PRECONDIȚIE: DB Links existente pe bdglobal:
--   CREATE PUBLIC DATABASE LINK bdam CONNECT TO ARTGALLERY_AM IDENTIFIED BY parola USING bdam;
--   CREATE PUBLIC DATABASE LINK bdeu CONNECT TO ARTGALLERY_EU IDENTIFIED BY parola USING bdeu;
-- și drepturi acordate din bdam/bdeu către bdglobal (GRANT SELECT,INSERT,UPDATE,DELETE ON ...)
-- ============================================================


-- ===========================================================
-- PASUL 0: ACORDARE DREPTURI
-- ===========================================================
-- ATENȚIE: GRANT este DDL → NU poate fi executat printr-un DB link
--          (eroare ORA-02021). Granturile trebuie rulate LOCAL pe
--          fiecare bază de date, conectat ca ownerul tabelelor.
--
-- În arhitectura noastră (ARTGALLERY_GLOBAL trăiește pe DB1/bdam):
--   • Granturile AM se rulează pe DB1, conectat ca ARTGALLERY_AM,
--     către userul local ARTGALLERY_GLOBAL.
--   • Granturile EU NU sunt necesare, pentru că link_eu (definit pe
--     DB1) se conectează la DB2 chiar ca ARTGALLERY_EU – ownerul
--     tabelelor *_EU are deja toate drepturile pe propriile obiecte.
-- ===========================================================

-- === RULAT PE DB1 (ORCLPDB), conectat ca ARTGALLERY_AM ===
GRANT SELECT, INSERT, UPDATE, DELETE ON EXHIBITOR_AM           TO ARTGALLERY_GLOBAL;
GRANT SELECT, INSERT, UPDATE, DELETE ON EXHIBITION_AM          TO ARTGALLERY_GLOBAL;
GRANT SELECT, INSERT, UPDATE, DELETE ON ARTWORK_EXHIBITION_AM  TO ARTGALLERY_GLOBAL;
GRANT SELECT, INSERT, UPDATE, DELETE ON LOAN_AM                TO ARTGALLERY_GLOBAL;
GRANT SELECT, INSERT, UPDATE, DELETE ON GALLERY_REVIEW_AM      TO ARTGALLERY_GLOBAL;
GRANT SELECT, INSERT, UPDATE, DELETE ON ARTWORK_DETAILS        TO ARTGALLERY_GLOBAL;
GRANT SELECT, INSERT, UPDATE, DELETE ON ARTIST_AM              TO ARTGALLERY_GLOBAL;
GRANT SELECT, INSERT, UPDATE, DELETE ON COLLECTION_AM          TO ARTGALLERY_GLOBAL;

-- === pe DB2 (bdeu) NU se rulează nimic ===
-- link_eu se autentifică deja ca ARTGALLERY_EU (vezi 1_sys_db1.sql).
-- Dacă vrei totuși să dai grant explicit (ex. pentru un alt link care
-- nu se conectează ca owner), trebuie să te conectezi la DB2:
--   sqlplus ARTGALLERY_EU/parola_eu@//localhost:1521/ORCLPDB2
-- și să rulezi grant-urile către un user care există LOCAL pe DB2.


-- ===========================================================
-- 4.A  TRANSPARENȚĂ PENTRU FRAGMENTE ORIZONTALE
--      (VIEW UNION ALL + INSTEAD OF trigger INSERT/UPDATE/DELETE)
-- ===========================================================
-- Toate comenzile de mai jos se execută conectat la ARTGALLERY_GLOBAL (bdglobal)
-- ===========================================================


-- -----------------------------------------------------------
-- 4.A.1  VIEW global: EXHIBITOR
-- Reconstruiește relația globală din EXHIBITOR_AM@link_am + EXHIBITOR_EU@link_eu
-- -----------------------------------------------------------

CREATE OR REPLACE VIEW EXHIBITOR AS
    SELECT exhibitor_id, name, address, city, contact_info
    FROM   ARTGALLERY_AM.EXHIBITOR_AM@link_am
    UNION ALL
    SELECT exhibitor_id, name, address, city, contact_info
    FROM   ARTGALLERY_EU.EXHIBITOR_EU@link_eu;

-- Verificare coloane updatabile
SELECT column_name, updatable
FROM   USER_UPDATABLE_COLUMNS
WHERE  table_name = UPPER('EXHIBITOR');
-- UNION ALL → coloanele NU sunt updatabile direct → necesită INSTEAD OF trigger

-- Trigger INSTEAD OF INSERT/UPDATE/DELETE pe VIEW EXHIBITOR
CREATE OR REPLACE TRIGGER t_exhibitor
    INSTEAD OF INSERT OR UPDATE OR DELETE
    ON EXHIBITOR
    FOR EACH ROW
BEGIN
    IF INSERTING THEN
        -- Rutare pe baza predicatelor de fragmentare
        IF :NEW.city = 'New York' THEN
            INSERT INTO ARTGALLERY_AM.EXHIBITOR_AM@link_am
                (exhibitor_id, name, address, city, contact_info)
            VALUES
                (:NEW.exhibitor_id, :NEW.name, :NEW.address,
                 :NEW.city, :NEW.contact_info);
        ELSIF :NEW.city IN ('Paris','London','Madrid','Florence') THEN
            INSERT INTO ARTGALLERY_EU.EXHIBITOR_EU@link_eu
                (exhibitor_id, name, address, city, contact_info)
            VALUES
                (:NEW.exhibitor_id, :NEW.name, :NEW.address,
                 :NEW.city, :NEW.contact_info);
        ELSE
            -- City necunoscută: stație default Americas
            INSERT INTO ARTGALLERY_AM.EXHIBITOR_AM@link_am
                (exhibitor_id, name, address, city, contact_info)
            VALUES
                (:NEW.exhibitor_id, :NEW.name, :NEW.address,
                 :NEW.city, :NEW.contact_info);
        END IF;

    ELSIF UPDATING THEN
        -- UPDATE pe fragmentul corect (identificat după city VECHE sau exhibitor_id)
        -- Cazul 1: rândul existia pe AM
        UPDATE ARTGALLERY_AM.EXHIBITOR_AM@link_am
        SET    name         = :NEW.name,
               address      = :NEW.address,
               city         = :NEW.city,
               contact_info = :NEW.contact_info
        WHERE  exhibitor_id = :OLD.exhibitor_id;

        IF SQL%ROWCOUNT = 0 THEN
            -- Rândul nu era pe AM, încearcă EU
            UPDATE ARTGALLERY_EU.EXHIBITOR_EU@link_eu
            SET    name         = :NEW.name,
                   address      = :NEW.address,
                   city         = :NEW.city,
                   contact_info = :NEW.contact_info
            WHERE  exhibitor_id = :OLD.exhibitor_id;
        END IF;

    ELSIF DELETING THEN
        -- DELETE din ambele fragmente (PK garantează că există doar în unul)
        DELETE FROM ARTGALLERY_AM.EXHIBITOR_AM@link_am
        WHERE  exhibitor_id = :OLD.exhibitor_id;

        DELETE FROM ARTGALLERY_EU.EXHIBITOR_EU@link_eu
        WHERE  exhibitor_id = :OLD.exhibitor_id;
    END IF;
END t_exhibitor;
/

-- Test INSERT prin view (verificare trigger)
INSERT INTO EXHIBITOR (exhibitor_id, name, address, city, contact_info)
VALUES (100, 'Guggenheim', '1071 5th Ave', 'New York', 'info@guggenheim.org');

INSERT INTO EXHIBITOR (exhibitor_id, name, address, city, contact_info)
VALUES (101, 'Uffizi Gallery', 'Piazzale degli Uffizi', 'Florence', 'info@uffizi.it');

-- Verificare că s-au inserat pe stațiile corecte
SELECT * FROM ARTGALLERY_AM.EXHIBITOR_AM@link_am WHERE exhibitor_id IN (100,101);
SELECT * FROM ARTGALLERY_EU.EXHIBITOR_EU@link_eu WHERE exhibitor_id IN (100,101);

-- Test UPDATE prin view
UPDATE EXHIBITOR SET contact_info = 'contact@guggenheim.org' WHERE exhibitor_id = 100;

-- Test DELETE prin view
DELETE FROM EXHIBITOR WHERE exhibitor_id IN (100, 101);

ROLLBACK; -- anulare test


-- -----------------------------------------------------------
-- 4.A.2  VIEW global: EXHIBITION
-- -----------------------------------------------------------

CREATE OR REPLACE VIEW EXHIBITION AS
    SELECT exhibition_id, title, start_date, end_date, exhibitor_id, description
    FROM   ARTGALLERY_AM.EXHIBITION_AM@link_am
    UNION ALL
    SELECT exhibition_id, title, start_date, end_date, exhibitor_id, description
    FROM   ARTGALLERY_EU.EXHIBITION_EU@link_eu;

CREATE OR REPLACE TRIGGER t_exhibition
    INSTEAD OF INSERT OR UPDATE OR DELETE
    ON EXHIBITION
    FOR EACH ROW
DECLARE
    v_city ARTGALLERY_AM.EXHIBITOR_AM.city%TYPE;
BEGIN
    IF INSERTING THEN
        BEGIN
            SELECT city INTO v_city
            FROM   ARTGALLERY_AM.EXHIBITOR_AM@link_am
            WHERE  exhibitor_id = :NEW.exhibitor_id;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                BEGIN
                    SELECT city INTO v_city
                    FROM   ARTGALLERY_EU.EXHIBITOR_EU@link_eu
                    WHERE  exhibitor_id = :NEW.exhibitor_id;
                EXCEPTION
                    WHEN NO_DATA_FOUND THEN
                        v_city := 'New York'; -- fallback AM
                END;
        END;

        IF v_city = 'New York' THEN
            INSERT INTO ARTGALLERY_AM.EXHIBITION_AM@link_am
                (exhibition_id, title, start_date, end_date, exhibitor_id, description)
            VALUES
                (:NEW.exhibition_id, :NEW.title, :NEW.start_date,
                 :NEW.end_date, :NEW.exhibitor_id, :NEW.description);
        ELSE
            INSERT INTO ARTGALLERY_EU.EXHIBITION_EU@link_eu
                (exhibition_id, title, start_date, end_date, exhibitor_id, description)
            VALUES
                (:NEW.exhibition_id, :NEW.title, :NEW.start_date,
                 :NEW.end_date, :NEW.exhibitor_id, :NEW.description);
        END IF;

    ELSIF UPDATING THEN
        UPDATE ARTGALLERY_AM.EXHIBITION_AM@link_am
        SET    title = :NEW.title, start_date = :NEW.start_date,
               end_date = :NEW.end_date, description = :NEW.description
        WHERE  exhibition_id = :OLD.exhibition_id;

        IF SQL%ROWCOUNT = 0 THEN
            UPDATE ARTGALLERY_EU.EXHIBITION_EU@link_eu
            SET    title = :NEW.title, start_date = :NEW.start_date,
                   end_date = :NEW.end_date, description = :NEW.description
            WHERE  exhibition_id = :OLD.exhibition_id;
        END IF;

    ELSIF DELETING THEN
        DELETE FROM ARTGALLERY_AM.EXHIBITION_AM@link_am
        WHERE  exhibition_id = :OLD.exhibition_id;

        DELETE FROM ARTGALLERY_EU.EXHIBITION_EU@link_eu
        WHERE  exhibition_id = :OLD.exhibition_id;
    END IF;
END t_exhibition;
/


-- -----------------------------------------------------------
-- 4.A.3  VIEW global: ARTWORK_EXHIBITION
-- -----------------------------------------------------------

CREATE OR REPLACE VIEW ARTWORK_EXHIBITION AS
    SELECT artwork_id, exhibition_id, position_in_gallery, featured_status
    FROM   ARTGALLERY_AM.ARTWORK_EXHIBITION_AM@link_am
    UNION ALL
    SELECT artwork_id, exhibition_id, position_in_gallery, featured_status
    FROM   ARTGALLERY_EU.ARTWORK_EXHIBITION_EU@link_eu;

CREATE OR REPLACE TRIGGER t_artwork_exhibition
    INSTEAD OF INSERT OR UPDATE OR DELETE
    ON ARTWORK_EXHIBITION
    FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    IF INSERTING THEN
        -- Verifică pe ce stație există expoziția
        SELECT COUNT(*) INTO v_cnt
        FROM   ARTGALLERY_AM.EXHIBITION_AM@link_am
        WHERE  exhibition_id = :NEW.exhibition_id;

        IF v_cnt > 0 THEN
            INSERT INTO ARTGALLERY_AM.ARTWORK_EXHIBITION_AM@link_am
                (artwork_id, exhibition_id, position_in_gallery, featured_status)
            VALUES
                (:NEW.artwork_id, :NEW.exhibition_id,
                 :NEW.position_in_gallery, :NEW.featured_status);
        ELSE
            INSERT INTO ARTGALLERY_EU.ARTWORK_EXHIBITION_EU@link_eu
                (artwork_id, exhibition_id, position_in_gallery, featured_status)
            VALUES
                (:NEW.artwork_id, :NEW.exhibition_id,
                 :NEW.position_in_gallery, :NEW.featured_status);
        END IF;

    ELSIF UPDATING THEN
        UPDATE ARTGALLERY_AM.ARTWORK_EXHIBITION_AM@link_am
        SET    position_in_gallery = :NEW.position_in_gallery,
               featured_status     = :NEW.featured_status
        WHERE  artwork_id = :OLD.artwork_id AND exhibition_id = :OLD.exhibition_id;

        IF SQL%ROWCOUNT = 0 THEN
            UPDATE ARTGALLERY_EU.ARTWORK_EXHIBITION_EU@link_eu
            SET    position_in_gallery = :NEW.position_in_gallery,
                   featured_status     = :NEW.featured_status
            WHERE  artwork_id = :OLD.artwork_id AND exhibition_id = :OLD.exhibition_id;
        END IF;

    ELSIF DELETING THEN
        DELETE FROM ARTGALLERY_AM.ARTWORK_EXHIBITION_AM@link_am
        WHERE  artwork_id = :OLD.artwork_id AND exhibition_id = :OLD.exhibition_id;

        DELETE FROM ARTGALLERY_EU.ARTWORK_EXHIBITION_EU@link_eu
        WHERE  artwork_id = :OLD.artwork_id AND exhibition_id = :OLD.exhibition_id;
    END IF;
END t_artwork_exhibition;
/


-- -----------------------------------------------------------
-- 4.A.4  VIEW global: LOAN
-- -----------------------------------------------------------

CREATE OR REPLACE VIEW LOAN AS
    SELECT loan_id, artwork_id, exhibitor_id, start_date, end_date, conditions
    FROM   ARTGALLERY_AM.LOAN_AM@link_am
    UNION ALL
    SELECT loan_id, artwork_id, exhibitor_id, start_date, end_date, conditions
    FROM   ARTGALLERY_EU.LOAN_EU@link_eu;

CREATE OR REPLACE TRIGGER t_loan
    INSTEAD OF INSERT OR UPDATE OR DELETE
    ON LOAN
    FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    IF INSERTING THEN
        SELECT COUNT(*) INTO v_cnt
        FROM   ARTGALLERY_AM.EXHIBITOR_AM@link_am
        WHERE  exhibitor_id = :NEW.exhibitor_id;

        IF v_cnt > 0 THEN
            INSERT INTO ARTGALLERY_AM.LOAN_AM@link_am
                (loan_id, artwork_id, exhibitor_id, start_date, end_date, conditions)
            VALUES (:NEW.loan_id, :NEW.artwork_id, :NEW.exhibitor_id,
                    :NEW.start_date, :NEW.end_date, :NEW.conditions);
        ELSE
            INSERT INTO ARTGALLERY_EU.LOAN_EU@link_eu
                (loan_id, artwork_id, exhibitor_id, start_date, end_date, conditions)
            VALUES (:NEW.loan_id, :NEW.artwork_id, :NEW.exhibitor_id,
                    :NEW.start_date, :NEW.end_date, :NEW.conditions);
        END IF;

    ELSIF UPDATING THEN
        UPDATE ARTGALLERY_AM.LOAN_AM@link_am
        SET    start_date = :NEW.start_date, end_date = :NEW.end_date,
               conditions = :NEW.conditions
        WHERE  loan_id = :OLD.loan_id;

        IF SQL%ROWCOUNT = 0 THEN
            UPDATE ARTGALLERY_EU.LOAN_EU@link_eu
            SET    start_date = :NEW.start_date, end_date = :NEW.end_date,
                   conditions = :NEW.conditions
            WHERE  loan_id = :OLD.loan_id;
        END IF;

    ELSIF DELETING THEN
        DELETE FROM ARTGALLERY_AM.LOAN_AM@link_am  WHERE loan_id = :OLD.loan_id;
        DELETE FROM ARTGALLERY_EU.LOAN_EU@link_eu  WHERE loan_id = :OLD.loan_id;
    END IF;
END t_loan;
/


-- -----------------------------------------------------------
-- 4.A.5  VIEW global: GALLERY_REVIEW
-- -----------------------------------------------------------

CREATE OR REPLACE VIEW GALLERY_REVIEW AS
    SELECT review_id, visitor_id, artwork_id, exhibition_id,
           rating, review_text, review_date
    FROM   ARTGALLERY_AM.GALLERY_REVIEW_AM@link_am
    UNION ALL
    SELECT review_id, visitor_id, artwork_id, exhibition_id,
           rating, review_text, review_date
    FROM   ARTGALLERY_EU.GALLERY_REVIEW_EU@link_eu;

CREATE OR REPLACE TRIGGER t_gallery_review
    INSTEAD OF INSERT OR UPDATE OR DELETE
    ON GALLERY_REVIEW
    FOR EACH ROW
DECLARE
    v_cnt NUMBER;
BEGIN
    IF INSERTING THEN
        -- Dacă are exhibition_id, verificăm pe ce stație e expoziția
        IF :NEW.exhibition_id IS NOT NULL THEN
            SELECT COUNT(*) INTO v_cnt
            FROM   ARTGALLERY_AM.EXHIBITION_AM@link_am
            WHERE  exhibition_id = :NEW.exhibition_id;

            IF v_cnt > 0 THEN
                INSERT INTO ARTGALLERY_AM.GALLERY_REVIEW_AM@link_am
                    (review_id, visitor_id, artwork_id, exhibition_id,
                     rating, review_text, review_date)
                VALUES (:NEW.review_id, :NEW.visitor_id, :NEW.artwork_id,
                        :NEW.exhibition_id, :NEW.rating,
                        :NEW.review_text, :NEW.review_date);
            ELSE
                INSERT INTO ARTGALLERY_EU.GALLERY_REVIEW_EU@link_eu
                    (review_id, visitor_id, artwork_id, exhibition_id,
                     rating, review_text, review_date)
                VALUES (:NEW.review_id, :NEW.visitor_id, :NEW.artwork_id,
                        :NEW.exhibition_id, :NEW.rating,
                        :NEW.review_text, :NEW.review_date);
            END IF;
        ELSE
            -- Fără expoziție → stație default: AM
            INSERT INTO ARTGALLERY_AM.GALLERY_REVIEW_AM@link_am
                (review_id, visitor_id, artwork_id, exhibition_id,
                 rating, review_text, review_date)
            VALUES (:NEW.review_id, :NEW.visitor_id, :NEW.artwork_id,
                    :NEW.exhibition_id, :NEW.rating,
                    :NEW.review_text, :NEW.review_date);
        END IF;

    ELSIF UPDATING THEN
        UPDATE ARTGALLERY_AM.GALLERY_REVIEW_AM@link_am
        SET    rating = :NEW.rating, review_text = :NEW.review_text,
               review_date = :NEW.review_date
        WHERE  review_id = :OLD.review_id;

        IF SQL%ROWCOUNT = 0 THEN
            UPDATE ARTGALLERY_EU.GALLERY_REVIEW_EU@link_eu
            SET    rating = :NEW.rating, review_text = :NEW.review_text,
                   review_date = :NEW.review_date
            WHERE  review_id = :OLD.review_id;
        END IF;

    ELSIF DELETING THEN
        DELETE FROM ARTGALLERY_AM.GALLERY_REVIEW_AM@link_am WHERE review_id = :OLD.review_id;
        DELETE FROM ARTGALLERY_EU.GALLERY_REVIEW_EU@link_eu WHERE review_id = :OLD.review_id;
    END IF;
END t_gallery_review;
/


-- ===========================================================
-- 4.B  TRANSPARENȚĂ PENTRU FRAGMENTE VERTICALE: ARTWORK
--      VIEW pe ARTWORK_CORE@link_eu JOIN ARTWORK_DETAILS@link_am
-- ===========================================================

CREATE OR REPLACE VIEW ARTWORK AS
    SELECT
        ac.artwork_id,
        ac.title,
        ac.artist_id,
        ac.year_created,
        ac.medium,
        ac.collection_id,
        ad.location_id,
        ad.estimated_value
    FROM   ARTGALLERY_EU.ARTWORK_CORE@link_eu    ac
    JOIN   ARTGALLERY_AM.ARTWORK_DETAILS@link_am ad
        ON ac.artwork_id = ad.artwork_id;

-- Verificare coloane updatabile (JOIN → blocate implicit, necesită INSTEAD OF)
SELECT column_name, updatable
FROM   USER_UPDATABLE_COLUMNS
WHERE  table_name = UPPER('ARTWORK');

-- INSTEAD OF trigger pentru VIEW ARTWORK (fragmentare verticală)
CREATE OR REPLACE TRIGGER t_artwork
    INSTEAD OF INSERT OR UPDATE OR DELETE
    ON ARTWORK
    FOR EACH ROW
BEGIN
    IF INSERTING THEN
        -- INSERT simultan în AMBELE fragmente verticale
        INSERT INTO ARTGALLERY_EU.ARTWORK_CORE@link_eu
            (artwork_id, title, artist_id, year_created, medium, collection_id)
        VALUES
            (:NEW.artwork_id, :NEW.title, :NEW.artist_id,
             :NEW.year_created, :NEW.medium, :NEW.collection_id);

        INSERT INTO ARTGALLERY_AM.ARTWORK_DETAILS@link_am
            (artwork_id, location_id, estimated_value)
        VALUES
            (:NEW.artwork_id, :NEW.location_id, :NEW.estimated_value);

    ELSIF UPDATING THEN
        -- UPDATE pe fragmentul corespunzător fiecărui atribut modificat
        UPDATE ARTGALLERY_EU.ARTWORK_CORE@link_eu
        SET    title         = :NEW.title,
               artist_id     = :NEW.artist_id,
               year_created  = :NEW.year_created,
               medium        = :NEW.medium,
               collection_id = :NEW.collection_id
        WHERE  artwork_id    = :OLD.artwork_id;

        UPDATE ARTGALLERY_AM.ARTWORK_DETAILS@link_am
        SET    location_id     = :NEW.location_id,
               estimated_value = :NEW.estimated_value
        WHERE  artwork_id      = :OLD.artwork_id;

    ELSIF DELETING THEN
        -- DELETE din AMBELE fragmente verticale
        DELETE FROM ARTGALLERY_EU.ARTWORK_CORE@link_eu
        WHERE  artwork_id = :OLD.artwork_id;

        DELETE FROM ARTGALLERY_AM.ARTWORK_DETAILS@link_am
        WHERE  artwork_id = :OLD.artwork_id;
    END IF;
END t_artwork;
/

-- Test INSERT prin view ARTWORK
INSERT INTO ARTWORK (artwork_id, title, artist_id, year_created, medium,
                     collection_id, location_id, estimated_value)
VALUES (99, 'Test Painting', 1, 2020, 'Oil on Canvas', 1, 1, 50000);

-- Verificare că s-a inserat în ambele fragmente
SELECT * FROM ARTGALLERY_EU.ARTWORK_CORE@link_eu    WHERE artwork_id = 99;
SELECT * FROM ARTGALLERY_AM.ARTWORK_DETAILS@link_am WHERE artwork_id = 99;

-- Test UPDATE
UPDATE ARTWORK SET estimated_value = 75000, title = 'Test Painting Updated'
WHERE artwork_id = 99;

-- Test DELETE
DELETE FROM ARTWORK WHERE artwork_id = 99;

ROLLBACK; -- anulare test


-- ===========================================================
-- 4.C  TRANSPARENȚĂ PRIN SINONIME
--      Sinonime locale pe ARTGALLERY_GLOBAL pentru accesul
--      direct la fragmente individuale, fără a specifica @link
-- ===========================================================

-- Sinonime pentru fragmentele de pe bdam (AM)
CREATE OR REPLACE SYNONYM EXHIBITOR_AM          FOR ARTGALLERY_AM.EXHIBITOR_AM@link_am;
CREATE OR REPLACE SYNONYM EXHIBITION_AM         FOR ARTGALLERY_AM.EXHIBITION_AM@link_am;
CREATE OR REPLACE SYNONYM ARTWORK_EXHIBITION_AM FOR ARTGALLERY_AM.ARTWORK_EXHIBITION_AM@link_am;
CREATE OR REPLACE SYNONYM LOAN_AM               FOR ARTGALLERY_AM.LOAN_AM@link_am;
CREATE OR REPLACE SYNONYM GALLERY_REVIEW_AM     FOR ARTGALLERY_AM.GALLERY_REVIEW_AM@link_am;
CREATE OR REPLACE SYNONYM ARTWORK_DETAILS       FOR ARTGALLERY_AM.ARTWORK_DETAILS@link_am;
CREATE OR REPLACE SYNONYM ARTIST_AM             FOR ARTGALLERY_AM.ARTIST_AM@link_am;
CREATE OR REPLACE SYNONYM COLLECTION_AM         FOR ARTGALLERY_AM.COLLECTION_AM@link_am;

-- Sinonime pentru fragmentele de pe bdeu (EU)
CREATE OR REPLACE SYNONYM EXHIBITOR_EU          FOR ARTGALLERY_EU.EXHIBITOR_EU@link_eu;
CREATE OR REPLACE SYNONYM EXHIBITION_EU         FOR ARTGALLERY_EU.EXHIBITION_EU@link_eu;
CREATE OR REPLACE SYNONYM ARTWORK_EXHIBITION_EU FOR ARTGALLERY_EU.ARTWORK_EXHIBITION_EU@link_eu;
CREATE OR REPLACE SYNONYM LOAN_EU               FOR ARTGALLERY_EU.LOAN_EU@link_eu;
CREATE OR REPLACE SYNONYM GALLERY_REVIEW_EU     FOR ARTGALLERY_EU.GALLERY_REVIEW_EU@link_eu;
CREATE OR REPLACE SYNONYM ARTWORK_CORE          FOR ARTGALLERY_EU.ARTWORK_CORE@link_eu;
CREATE OR REPLACE SYNONYM ARTIST_EU             FOR ARTGALLERY_EU.ARTIST_EU@link_eu;
CREATE OR REPLACE SYNONYM COLLECTION_EU         FOR ARTGALLERY_EU.COLLECTION_EU@link_eu;

-- Verificare sinonime definite
SELECT synonym_name, table_owner, table_name, db_link
FROM   USER_SYNONYMS
ORDER BY synonym_name;

-- Utilizare sinonime (fără @link în cereri)
SELECT * FROM EXHIBITOR_AM;   -- echivalent cu: SELECT * FROM ARTGALLERY_AM.EXHIBITOR_AM@link_am
SELECT * FROM EXHIBITOR_EU;   -- echivalent cu: SELECT * FROM ARTGALLERY_EU.EXHIBITOR_EU@link_eu


-- ===========================================================
-- 4.D  TRANSPARENȚĂ PRIN PROCEDURI STOCATE
--      Proceduri care ascund complet localizarea datelor
--      și implementează logica de rutare
-- ===========================================================

-- -----------------------------------------------------------
-- 4.D.1  Procedură: insert_exhibitor
--         Inserează un organizator pe stația corectă fără
--         ca apelantul să știe structura distribuită
-- -----------------------------------------------------------

CREATE OR REPLACE PROCEDURE insert_exhibitor (
    p_exhibitor_id  IN NUMBER,
    p_name          IN VARCHAR2,
    p_address       IN VARCHAR2,
    p_city          IN VARCHAR2,
    p_contact_info  IN VARCHAR2
) AS
BEGIN
    IF p_city = 'New York' THEN
        INSERT INTO ARTGALLERY_AM.EXHIBITOR_AM@link_am
            (exhibitor_id, name, address, city, contact_info)
        VALUES (p_exhibitor_id, p_name, p_address, p_city, p_contact_info);
    ELSIF p_city IN ('Paris','London','Madrid') THEN
        INSERT INTO ARTGALLERY_EU.EXHIBITOR_EU@link_eu
            (exhibitor_id, name, address, city, contact_info)
        VALUES (p_exhibitor_id, p_name, p_address, p_city, p_contact_info);
    ELSE
        -- Oraș necunoscut → implicit AM
        INSERT INTO ARTGALLERY_AM.EXHIBITOR_AM@link_am
            (exhibitor_id, name, address, city, contact_info)
        VALUES (p_exhibitor_id, p_name, p_address, p_city, p_contact_info);
    END IF;
    COMMIT;
END insert_exhibitor;
/

-- Test procedură
EXECUTE insert_exhibitor(200, 'MET Museum', '1000 5th Ave', 'New York', 'info@metmuseum.org');
-- verificare
SELECT * FROM EXHIBITOR WHERE exhibitor_id = 200;
ROLLBACK;


-- -----------------------------------------------------------
-- 4.D.2  Procedură: delete_exhibition
--         Șterge o expoziție din stația corespunzătoare,
--         gestionând mai întâi FK-urile dependente
-- -----------------------------------------------------------

CREATE OR REPLACE PROCEDURE delete_exhibition (
    p_exhibition_id IN NUMBER
) AS
    v_cnt NUMBER;
BEGIN
    -- Verifică pe ce stație există expoziția
    SELECT COUNT(*) INTO v_cnt
    FROM   ARTGALLERY_AM.EXHIBITION_AM@link_am
    WHERE  exhibition_id = p_exhibition_id;

    IF v_cnt > 0 THEN
        -- Șterge mai întâi înregistrările dependente
        DELETE FROM ARTGALLERY_AM.ARTWORK_EXHIBITION_AM@link_am
        WHERE  exhibition_id = p_exhibition_id;

        DELETE FROM ARTGALLERY_AM.GALLERY_REVIEW_AM@link_am
        WHERE  exhibition_id = p_exhibition_id;

        DELETE FROM ARTGALLERY_AM.EXHIBITION_AM@link_am
        WHERE  exhibition_id = p_exhibition_id;
    ELSE
        DELETE FROM ARTGALLERY_EU.ARTWORK_EXHIBITION_EU@link_eu
        WHERE  exhibition_id = p_exhibition_id;

        DELETE FROM ARTGALLERY_EU.GALLERY_REVIEW_EU@link_eu
        WHERE  exhibition_id = p_exhibition_id;

        DELETE FROM ARTGALLERY_EU.EXHIBITION_EU@link_eu
        WHERE  exhibition_id = p_exhibition_id;
    END IF;
    COMMIT;
END delete_exhibition;
/

-- Test procedură
-- expoziție cu exhibitor_id=1 (Louvre=EU)
EXECUTE delete_exhibition(1);
-- nu mai trebuie să existe
SELECT * FROM EXHIBITION WHERE exhibition_id = 1;
ROLLBACK;


-- -----------------------------------------------------------
-- 4.D.3  Procedură: transfer_artwork_between_locations
--         Actualizează location_id dintr-un fragment vertical
--         fără ca aplicația să știe că e în ARTWORK_DETAILS@link_am
-- -----------------------------------------------------------

CREATE OR REPLACE PROCEDURE transfer_artwork (
    p_artwork_id   IN NUMBER,
    p_location_id  IN NUMBER
) AS
BEGIN
    UPDATE ARTGALLERY_AM.ARTWORK_DETAILS@link_am
    SET    location_id = p_location_id
    WHERE  artwork_id  = p_artwork_id;

    IF SQL%ROWCOUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20001,
            'Artwork ' || p_artwork_id || ' not found in ARTWORK_DETAILS.');
    END IF;
    COMMIT;
END transfer_artwork;
/

-- Test
-- mută artwork 1 în sala 2
EXECUTE transfer_artwork(1, 2);
SELECT location_id FROM ARTWORK WHERE artwork_id = 1;
ROLLBACK;


-- -----------------------------------------------------------
-- 4.D.4  Procedură: get_exhibition_summary
--         Returnează statistici globale pentru o expoziție,
--         transparentă față de localizarea fragmentelor
-- -----------------------------------------------------------

CREATE OR REPLACE PROCEDURE get_exhibition_summary (
    p_exhibition_id IN  NUMBER,
    p_title         OUT VARCHAR2,
    p_nr_artwork    OUT NUMBER,
    p_total_value   OUT NUMBER
) AS
BEGIN
    SELECT e.title,
           COUNT(DISTINCT ax.artwork_id),
           SUM(ad.estimated_value)
    INTO   p_title, p_nr_artwork, p_total_value
    FROM   EXHIBITION e            -- VIEW global
    JOIN   ARTWORK_EXHIBITION ax   -- VIEW global
        ON ax.exhibition_id = e.exhibition_id
    JOIN   ARTWORK_DETAILS ad      -- sinonim
        ON ad.artwork_id    = ax.artwork_id
    WHERE  e.exhibition_id = p_exhibition_id
    GROUP BY e.title;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_title       := NULL;
        p_nr_artwork  := 0;
        p_total_value := 0;
END get_exhibition_summary;
/

-- Test
DECLARE
    v_title VARCHAR2(128);
    v_nr    NUMBER;
    v_val   NUMBER;
BEGIN
    get_exhibition_summary(1, v_title, v_nr, v_val);
    DBMS_OUTPUT.PUT_LINE('Titlu: '       || v_title);
    DBMS_OUTPUT.PUT_LINE('Nr. lucrari: ' || v_nr);
    DBMS_OUTPUT.PUT_LINE('Val. totala: ' || v_val);
END;
/


-- ===========================================================
-- 4.E  VERIFICAREA TRANSPARENȚEI – scenarii demonstrative
-- ===========================================================

-- -----------------------------------------------------------
-- 4.E.1  Verificare transparență localizare: utilizatorul
--         vede EXHIBITOR ca tabel unic, fără să știe de
--         fragmentele EXHIBITOR_AM și EXHIBITOR_EU
-- -----------------------------------------------------------

-- Cerere globală prin VIEW (fără să se specifice @link_am/@link_eu)
SELECT exhibitor_id, name, city
FROM   EXHIBITOR
ORDER BY city;

-- Cerere care join-uiește VIEW-uri globale
SELECT
    eh.name                  AS organizator,
    eh.city                  AS oras,
    COUNT(e.exhibition_id)   AS nr_expozitii
FROM   EXHIBITOR  eh
JOIN   EXHIBITION e ON e.exhibitor_id = eh.exhibitor_id
GROUP BY eh.name, eh.city
ORDER BY nr_expozitii DESC;


-- -----------------------------------------------------------
-- 4.E.2  Verificare transparență LMD: INSERT prin VIEW global
--         → apare automat pe stația corectă
-- -----------------------------------------------------------

-- INSERT global → trigger rutează automat pe bdeu (London = Europe)
INSERT INTO EXHIBITOR (exhibitor_id, name, address, city, contact_info)
VALUES (102, 'V&A Museum', 'Cromwell Rd', 'London', 'info@vam.ac.uk');

-- Verificare că a apărut pe EU, nu AM
SELECT * FROM ARTGALLERY_EU.EXHIBITOR_EU@link_eu WHERE exhibitor_id = 102;   -- trebuie găsit
SELECT * FROM ARTGALLERY_AM.EXHIBITOR_AM@link_am WHERE exhibitor_id = 102;   -- trebuie 0 rânduri

-- Prin VIEW global apare direct
SELECT * FROM EXHIBITOR WHERE exhibitor_id = 102;

ROLLBACK;


-- -----------------------------------------------------------
-- 4.E.3  Verificare transparență comenzi (III.B din laborator):
--         SELECT, INSERT, UPDATE, DELETE prin VIEW → tabel distant
-- -----------------------------------------------------------

-- SELECT cu JOIN inter-stații prin VIEW-uri globale (nici un @link vizibil)
SELECT
    ar.title             AS titlu_lucrare,
    e.title              AS expozitie,
    eh.name              AS organizator,
    eh.city              AS oras
FROM   ARTWORK           ar     -- VIEW: ARTWORK_CORE@link_eu JOIN ARTWORK_DETAILS@link_am
JOIN   ARTWORK_EXHIBITION ax    -- VIEW: UNION ALL AM + EU
    ON ax.artwork_id    = ar.artwork_id
JOIN   EXHIBITION e             -- VIEW: UNION ALL AM + EU
    ON e.exhibition_id  = ax.exhibition_id
JOIN   EXHIBITOR eh             -- VIEW: UNION ALL AM + EU
    ON eh.exhibitor_id  = e.exhibitor_id
ORDER BY eh.city, e.title, ar.title;


-- ===========================================================
-- 4.F  VIZUALIZARE STARE OBIECTE TRANSPARENȚĂ DEFINITE
-- ===========================================================

-- Lista view-urilor globale create pe ARTGALLERY_GLOBAL
SELECT view_name, text_length
FROM   USER_VIEWS
ORDER BY view_name;

-- Lista triggerelor INSTEAD OF
SELECT trigger_name, triggering_event, status
FROM   USER_TRIGGERS
WHERE  trigger_type = 'INSTEAD OF'
ORDER BY trigger_name;

-- Lista sinonimelor create
SELECT synonym_name, db_link
FROM   USER_SYNONYMS
ORDER BY synonym_name;

-- Lista procedurilor create
SELECT object_name, object_type, status
FROM   USER_OBJECTS
WHERE  object_type = 'PROCEDURE'
ORDER BY object_name;

-- Verificare coloane updatabile pe fiecare VIEW global
SELECT table_name, column_name, updatable
FROM   USER_UPDATABLE_COLUMNS
WHERE  table_name IN ('EXHIBITOR','EXHIBITION','ARTWORK_EXHIBITION',
                      'LOAN','GALLERY_REVIEW','ARTWORK')
ORDER BY table_name, column_name;
-- După definirea triggerelor INSTEAD OF, coloanele devin efectiv updatabile
-- prin mecanismul triggerului, chiar dacă USER_UPDATABLE_COLUMNS arată NO pentru UNION ALL
