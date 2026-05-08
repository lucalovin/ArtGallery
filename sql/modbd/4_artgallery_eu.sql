-- Creare Tabele Localizate
CREATE TABLE EXHIBITOR_EU (
    exhibitor_id NUMBER PRIMARY KEY,
    name         VARCHAR2(128) NOT NULL,
    address      VARCHAR2(256),
    city         VARCHAR2(64),
    contact_info VARCHAR2(256)
);

CREATE TABLE ARTWORK_CORE (
    artwork_id    NUMBER PRIMARY KEY,
    title         VARCHAR2(128) NOT NULL,
    artist_id     NUMBER NOT NULL,
    year_created  NUMBER(4),
    medium        VARCHAR2(64),
    collection_id NUMBER
);

CREATE TABLE EXHIBITION_EU (
    exhibition_id NUMBER PRIMARY KEY,
    title         VARCHAR2(128) NOT NULL,
    start_date    DATE NOT NULL,
    end_date      DATE NOT NULL,
    exhibitor_id  NUMBER NOT NULL REFERENCES EXHIBITOR_EU(exhibitor_id),
    description   VARCHAR2(512)
);

CREATE TABLE ARTWORK_EXHIBITION_EU (
    artwork_id          NUMBER NOT NULL REFERENCES ARTWORK_CORE(artwork_id),
    exhibition_id       NUMBER NOT NULL REFERENCES EXHIBITION_EU(exhibition_id),
    position_in_gallery VARCHAR2(64),
    featured_status     VARCHAR2(16),
    CONSTRAINT pk_ax_eu PRIMARY KEY (artwork_id, exhibition_id)
);

CREATE TABLE LOAN_EU (
    loan_id      NUMBER PRIMARY KEY,
    artwork_id   NUMBER NOT NULL REFERENCES ARTWORK_CORE(artwork_id),
    exhibitor_id NUMBER NOT NULL REFERENCES EXHIBITOR_EU(exhibitor_id),
    start_date   DATE NOT NULL,
    end_date     DATE,
    conditions   VARCHAR2(512)
);

CREATE TABLE GALLERY_REVIEW_EU (
    review_id     NUMBER PRIMARY KEY,
    visitor_id    NUMBER NOT NULL,
    artwork_id    NUMBER REFERENCES ARTWORK_CORE(artwork_id),
    exhibition_id NUMBER NOT NULL REFERENCES EXHIBITION_EU(exhibition_id),
    rating        NUMBER(1) NOT NULL CHECK (rating BETWEEN 1 AND 5),
    review_text   VARCHAR2(256),
    review_date   DATE NOT NULL
);

CREATE TABLE ARTIST_EU (artist_id NUMBER PRIMARY KEY, name VARCHAR2(128), nationality VARCHAR2(64), birth_year NUMBER(4), death_year NUMBER(4));
CREATE TABLE COLLECTION_EU (collection_id NUMBER PRIMARY KEY, name VARCHAR2(128), description VARCHAR2(512), created_date DATE);

-- Distribuire Date din Sursă
INSERT INTO EXHIBITOR_EU SELECT * FROM BDDALL.Exhibitor WHERE city IN ('Paris', 'London', 'Madrid');
INSERT INTO ARTWORK_CORE (artwork_id, title, artist_id, year_created, medium, collection_id) 
SELECT artwork_id, title, artist_id, year_created, medium, collection_id FROM BDDALL.Artwork;
INSERT INTO EXHIBITION_EU SELECT * FROM BDDALL.Exhibition WHERE exhibitor_id IN (SELECT exhibitor_id FROM EXHIBITOR_EU);
INSERT INTO ARTWORK_EXHIBITION_EU SELECT * FROM BDDALL.Artwork_Exhibition WHERE exhibition_id IN (SELECT exhibition_id FROM EXHIBITION_EU);
INSERT INTO LOAN_EU SELECT * FROM BDDALL.Loan WHERE exhibitor_id IN (SELECT exhibitor_id FROM EXHIBITOR_EU);
INSERT INTO GALLERY_REVIEW_EU SELECT * FROM BDDALL.Gallery_Review WHERE exhibition_id IN (SELECT exhibition_id FROM EXHIBITION_EU);
INSERT INTO ARTIST_EU SELECT * FROM BDDALL.Artist;
INSERT INTO COLLECTION_EU SELECT * FROM BDDALL.Collection;

COMMIT;