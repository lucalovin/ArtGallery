-- Creare Tabele Comune
CREATE TABLE LOCATION (location_id NUMBER PRIMARY KEY, name VARCHAR2(128), gallery_room VARCHAR2(32), type VARCHAR2(32), capacity NUMBER);
CREATE TABLE VISITOR (visitor_id NUMBER PRIMARY KEY, name VARCHAR2(128), email VARCHAR2(128), phone VARCHAR2(32), membership_type VARCHAR2(32), join_date DATE);
CREATE TABLE STAFF (staff_id NUMBER PRIMARY KEY, name VARCHAR2(128), role VARCHAR2(64), hire_date DATE, certification_level VARCHAR2(32));
CREATE TABLE INSURANCE_POLICY (policy_id NUMBER PRIMARY KEY, provider VARCHAR2(128), start_date DATE, end_date DATE, total_coverage_amount NUMBER(14,2));

CREATE TABLE INSURANCE (
    insurance_id   NUMBER PRIMARY KEY,
    artwork_id     NUMBER NOT NULL,
    policy_id      NUMBER NOT NULL REFERENCES INSURANCE_POLICY(policy_id),
    insured_amount NUMBER(14,2) NOT NULL
);

CREATE TABLE RESTORATION (
    restoration_id NUMBER PRIMARY KEY,
    artwork_id     NUMBER NOT NULL,
    staff_id       NUMBER NOT NULL REFERENCES STAFF(staff_id),
    start_date     DATE NOT NULL,
    end_date       DATE,
    description    VARCHAR2(512)
);

CREATE TABLE ACQUISITION (
    acquisition_id   NUMBER PRIMARY KEY,
    artwork_id       NUMBER NOT NULL UNIQUE,
    acquisition_date DATE NOT NULL,
    acquisition_type VARCHAR2(32) NOT NULL,
    price            NUMBER(12,2),
    staff_id         NUMBER NOT NULL REFERENCES STAFF(staff_id)
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

-- Creare Vizualizări de Transparență (Reconstrucție)
CREATE OR REPLACE VIEW GLOBAL_ARTWORK AS
SELECT c.artwork_id, c.title, c.artist_id, c.year_created, c.medium, c.collection_id, 
       d.location_id, d.estimated_value
FROM ARTWORK_CORE@link_eu c
JOIN ARTWORK_DETAILS@link_am d ON c.artwork_id = d.artwork_id;

CREATE OR REPLACE VIEW GLOBAL_EXHIBITOR AS
SELECT * FROM EXHIBITOR_EU@link_eu
UNION ALL
SELECT * FROM EXHIBITOR_AM@link_am;