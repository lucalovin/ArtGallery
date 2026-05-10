-- =============================================================
-- Script de rulat pe DB1 (ORCLPDB) ca ARTGALLERY_GLOBAL
-- =============================================================
-- Conectare exemplu:
--   sqlplus ARTGALLERY_GLOBAL/parola_global@//localhost:1521/ORCLPDB
--
-- link_eu  -> remote la DB2 (ORCLPDB2 / ARTGALLERY_EU)
-- link_am  -> loopback la DB1 (ARTGALLERY_AM)

-- Creare Tabele Comune
CREATE TABLE LOCATION (
    location_id  NUMBER PRIMARY KEY,
    name         VARCHAR2(128) NOT NULL,
    gallery_room VARCHAR2(32),
    type         VARCHAR2(32),
    capacity     NUMBER,
    CONSTRAINT ck_location_capacity_pos CHECK (capacity IS NULL OR capacity > 0)
);

CREATE TABLE VISITOR (
    visitor_id      NUMBER PRIMARY KEY,
    name            VARCHAR2(128) NOT NULL,
    email           VARCHAR2(128),
    phone           VARCHAR2(32),
    membership_type VARCHAR2(32),
    join_date       DATE
);

CREATE TABLE STAFF (
    staff_id            NUMBER PRIMARY KEY,
    name                VARCHAR2(128) NOT NULL,
    role                VARCHAR2(64) NOT NULL,
    hire_date           DATE NOT NULL,
    certification_level VARCHAR2(32)
);

CREATE TABLE INSURANCE_POLICY (
    policy_id             NUMBER PRIMARY KEY,
    provider              VARCHAR2(128) NOT NULL,
    start_date            DATE NOT NULL,
    end_date              DATE NOT NULL,
    total_coverage_amount NUMBER(14,2),
    CONSTRAINT ck_policy_dates CHECK (end_date >= start_date),
    CONSTRAINT ck_policy_total_cov_pos CHECK (total_coverage_amount IS NULL OR total_coverage_amount >= 0)
);

CREATE TABLE INSURANCE (
    insurance_id   NUMBER PRIMARY KEY,
    artwork_id     NUMBER NOT NULL,
    policy_id      NUMBER NOT NULL REFERENCES INSURANCE_POLICY(policy_id),
    insured_amount NUMBER(14,2) NOT NULL,
    CONSTRAINT ck_insured_amount_pos CHECK (insured_amount > 0)
);

CREATE TABLE RESTORATION (
    restoration_id NUMBER PRIMARY KEY,
    artwork_id     NUMBER NOT NULL,
    staff_id       NUMBER NOT NULL REFERENCES STAFF(staff_id),
    start_date     DATE NOT NULL,
    end_date       DATE,
    description    VARCHAR2(512),
    CONSTRAINT ck_restoration_dates CHECK (end_date IS NULL OR end_date >= start_date)
);

CREATE TABLE ACQUISITION (
    acquisition_id   NUMBER PRIMARY KEY,
    artwork_id       NUMBER NOT NULL UNIQUE,
    acquisition_date DATE NOT NULL,
    acquisition_type VARCHAR2(32) NOT NULL,
    price            NUMBER(12,2),
    staff_id         NUMBER NOT NULL REFERENCES STAFF(staff_id),
    CONSTRAINT ck_acquisition_price_pos CHECK (price IS NULL OR price >= 0)
);

-- Distribuire Date din Sursă
INSERT INTO LOCATION SELECT * FROM BDDALL.Location;
INSERT INTO VISITOR SELECT * FROM BDDALL.Visitor;
INSERT INTO STAFF SELECT * FROM BDDALL.Staff;
INSERT INTO INSURANCE_POLICY SELECT * FROM BDDALL.Insurance_Policy;
INSERT INTO INSURANCE SELECT * FROM BDDALL.Insurance;
INSERT INTO RESTORATION SELECT * FROM BDDALL.Restoration;
INSERT INTO ACQUISITION SELECT * FROM BDDALL.Acquisition;

COMMIT;

-- Creare Vizualizari de Transparene (Reconstructie)
CREATE OR REPLACE VIEW GLOBAL_ARTWORK AS
SELECT c.artwork_id, c.title, c.artist_id, c.year_created, c.medium, c.collection_id, 
       d.location_id, d.estimated_value
FROM ARTWORK_CORE@link_eu c
JOIN ARTWORK_DETAILS@link_am d ON c.artwork_id = d.artwork_id;

CREATE OR REPLACE VIEW GLOBAL_EXHIBITOR AS
SELECT * FROM EXHIBITOR_EU@link_eu
UNION ALL
SELECT * FROM EXHIBITOR_AM@link_am;

-- Unified view of all exhibitions across both regional schemas.
CREATE OR REPLACE VIEW GLOBAL_EXHIBITION AS
SELECT exhibition_id, title, start_date, end_date, exhibitor_id, description
  FROM EXHIBITION_EU@link_eu
UNION ALL
SELECT exhibition_id, title, start_date, end_date, exhibitor_id, description
  FROM EXHIBITION_AM@link_am;

-- Unified artwork <-> exhibition junction.
CREATE OR REPLACE VIEW GLOBAL_ARTWORK_EXHIBITION AS
SELECT artwork_id, exhibition_id, position_in_gallery, featured_status
  FROM ARTWORK_EXHIBITION_EU@link_eu
UNION ALL
SELECT artwork_id, exhibition_id, position_in_gallery, featured_status
  FROM ARTWORK_EXHIBITION_AM@link_am;

-- Unified loans across both regional schemas.
CREATE OR REPLACE VIEW GLOBAL_LOAN AS
SELECT loan_id, artwork_id, exhibitor_id, start_date, end_date, conditions
  FROM LOAN_EU@link_eu
UNION ALL
SELECT loan_id, artwork_id, exhibitor_id, start_date, end_date, conditions
  FROM LOAN_AM@link_am;

-- Unified gallery reviews across both regional schemas.
CREATE OR REPLACE VIEW GLOBAL_GALLERY_REVIEW AS
SELECT review_id, visitor_id, artwork_id, exhibition_id, rating, review_text, review_date
  FROM GALLERY_REVIEW_EU@link_eu
UNION ALL
SELECT review_id, visitor_id, artwork_id, exhibition_id, rating, review_text, review_date
  FROM GALLERY_REVIEW_AM@link_am;

-- Artist / Collection are fully replicated on both regional schemas, so we
-- simply expose one of the replicas (EU) as the global view.
CREATE OR REPLACE VIEW GLOBAL_ARTIST AS
SELECT * FROM ARTIST_EU@link_eu;

CREATE OR REPLACE VIEW GLOBAL_COLLECTION AS
SELECT * FROM COLLECTION_EU@link_eu;