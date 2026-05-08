-- =============================================================
-- Script de rulat pe DB1 (ORCLPDB) ca ARTGALLERY_AM
-- =============================================================
-- Conectare exemplu:
--   sqlplus ARTGALLERY_AM/parola_am@//localhost:1521/ORCLPDB

-- Creare Tabele Localizate
CREATE TABLE EXHIBITOR_AM (
    exhibitor_id NUMBER PRIMARY KEY,
    name         VARCHAR2(128) NOT NULL,
    address      VARCHAR2(256),
    city         VARCHAR2(64),
    contact_info VARCHAR2(256)
);

CREATE TABLE EXHIBITION_AM (
    exhibition_id NUMBER PRIMARY KEY,
    title         VARCHAR2(128) NOT NULL,
    start_date    DATE NOT NULL,
    end_date      DATE NOT NULL,
    exhibitor_id  NUMBER NOT NULL REFERENCES EXHIBITOR_AM(exhibitor_id),
    description   VARCHAR2(512),
    CONSTRAINT ck_exhibition_am_dates CHECK (end_date >= start_date)
);

CREATE TABLE ARTWORK_DETAILS (
    artwork_id      NUMBER PRIMARY KEY,
    location_id     NUMBER,
    estimated_value NUMBER(12,2)
);

CREATE TABLE ARTWORK_EXHIBITION_AM (
    artwork_id          NUMBER NOT NULL REFERENCES ARTWORK_DETAILS(artwork_id),
    exhibition_id       NUMBER NOT NULL REFERENCES EXHIBITION_AM(exhibition_id),
    position_in_gallery VARCHAR2(64),
    featured_status     VARCHAR2(16),
    CONSTRAINT pk_ax_am PRIMARY KEY (artwork_id, exhibition_id)
);

CREATE TABLE LOAN_AM (
    loan_id      NUMBER PRIMARY KEY,
    artwork_id   NUMBER NOT NULL REFERENCES ARTWORK_DETAILS(artwork_id),
    exhibitor_id NUMBER NOT NULL REFERENCES EXHIBITOR_AM(exhibitor_id),
    start_date   DATE NOT NULL,
    end_date     DATE,
    conditions   VARCHAR2(512),
    CONSTRAINT ck_loan_am_dates CHECK (end_date IS NULL OR end_date >= start_date)
);

CREATE TABLE GALLERY_REVIEW_AM (
    review_id     NUMBER PRIMARY KEY,
    visitor_id    NUMBER NOT NULL,
    artwork_id    NUMBER REFERENCES ARTWORK_DETAILS(artwork_id),
    exhibition_id NUMBER NOT NULL REFERENCES EXHIBITION_AM(exhibition_id),
    rating        NUMBER(1) NOT NULL CHECK (rating BETWEEN 1 AND 5),
    review_text   VARCHAR2(256),
    review_date   DATE NOT NULL
);

CREATE TABLE ARTIST_AM (artist_id NUMBER PRIMARY KEY, name VARCHAR2(128) NOT NULL, nationality VARCHAR2(64), birth_year NUMBER(4), death_year NUMBER(4));
CREATE TABLE COLLECTION_AM (collection_id NUMBER PRIMARY KEY, name VARCHAR2(128) NOT NULL, description VARCHAR2(512), created_date DATE);

-- Distribuire Date din Sursă
INSERT INTO EXHIBITOR_AM SELECT * FROM BDDALL.Exhibitor WHERE city = 'New York';
INSERT INTO EXHIBITION_AM SELECT * FROM BDDALL.Exhibition WHERE exhibitor_id IN (SELECT exhibitor_id FROM EXHIBITOR_AM);
INSERT INTO ARTWORK_DETAILS (artwork_id, location_id, estimated_value) SELECT artwork_id, location_id, estimated_value FROM BDDALL.Artwork; 
INSERT INTO ARTWORK_EXHIBITION_AM SELECT * FROM BDDALL.Artwork_Exhibition WHERE exhibition_id IN (SELECT exhibition_id FROM EXHIBITION_AM);
INSERT INTO LOAN_AM SELECT * FROM BDDALL.Loan WHERE exhibitor_id IN (SELECT exhibitor_id FROM EXHIBITOR_AM);
INSERT INTO GALLERY_REVIEW_AM SELECT * FROM BDDALL.Gallery_Review WHERE exhibition_id IN (SELECT exhibition_id FROM EXHIBITION_AM);
INSERT INTO ARTIST_AM SELECT * FROM BDDALL.Artist;
INSERT INTO COLLECTION_AM SELECT * FROM BDDALL.Collection;

COMMIT;