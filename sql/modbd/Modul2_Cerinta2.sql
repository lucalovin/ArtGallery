------------------------------------------------------------
cerinta 2
------------------------------------------------------------


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
	
	

-- DB Links definite de pe DB2
-- link_am     -> DB1 (ARTGALLERY_AM pe ORCLPDB) prin Net Service Name BD_AM
-- link_global -> DB1 (ARTGALLERY_GLOBAL pe ORCLPDB)
-- link_bddall -> DB1 (BDDALL pe ORCLPDB) necesar pentru popularea initiala din sursa centralizata
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


-- schema sursa bddall
CREATE TABLE Artist (
    artist_id    NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name         VARCHAR2(128) NOT NULL,
    nationality  VARCHAR2(64),
    birth_year   NUMBER(4),
    death_year   NUMBER(4)
);

CREATE TABLE Collection (
    collection_id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name          VARCHAR2(128) NOT NULL,
    description   VARCHAR2(512),
    created_date  DATE
);

CREATE TABLE Location (
    location_id  NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name         VARCHAR2(128) NOT NULL,
    gallery_room VARCHAR2(32),
    type         VARCHAR2(32),
    capacity     NUMBER,
    CONSTRAINT ck_location_capacity_pos CHECK (capacity IS NULL OR capacity > 0)
);

CREATE TABLE Artwork (
    artwork_id      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title           VARCHAR2(128) NOT NULL,
    artist_id       NUMBER NOT NULL REFERENCES Artist(artist_id),
    year_created    NUMBER(4),
    medium          VARCHAR2(64),
    collection_id   NUMBER REFERENCES Collection(collection_id),
    location_id     NUMBER REFERENCES Location(location_id),
    estimated_value NUMBER(12,2)
);

CREATE TABLE Visitor (
    visitor_id      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            VARCHAR2(128) NOT NULL,
    email           VARCHAR2(128),
    phone           VARCHAR2(32),
    membership_type VARCHAR2(32),
    join_date       DATE
);

CREATE TABLE Exhibitor (
    exhibitor_id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name         VARCHAR2(128) NOT NULL,
    address      VARCHAR2(256),
    city         VARCHAR2(64),
    contact_info VARCHAR2(256)
);

CREATE TABLE Exhibition (
    exhibition_id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title         VARCHAR2(128) NOT NULL,
    start_date    DATE NOT NULL,
    end_date      DATE NOT NULL,
    exhibitor_id  NUMBER NOT NULL REFERENCES Exhibitor(exhibitor_id),
    description   VARCHAR2(512),
    CONSTRAINT ck_exhibition_dates CHECK (end_date >= start_date)
);

CREATE TABLE Staff (
    staff_id            NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name                VARCHAR2(128) NOT NULL,
    role                VARCHAR2(64) NOT NULL,
    hire_date           DATE NOT NULL,
    certification_level VARCHAR2(32)
);

CREATE TABLE Acquisition (
    acquisition_id   NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    artwork_id       NUMBER NOT NULL UNIQUE REFERENCES Artwork(artwork_id),
    acquisition_date DATE NOT NULL,
    acquisition_type VARCHAR2(32) NOT NULL,
    price            NUMBER(12,2),
    staff_id         NUMBER NOT NULL REFERENCES Staff(staff_id),
    CONSTRAINT ck_acquisition_price_pos CHECK (price IS NULL OR price >= 0)
);

CREATE TABLE Loan (
    loan_id      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    artwork_id   NUMBER NOT NULL REFERENCES Artwork(artwork_id),
    exhibitor_id NUMBER NOT NULL REFERENCES Exhibitor(exhibitor_id),
    start_date   DATE NOT NULL,
    end_date     DATE,
    conditions   VARCHAR2(512),
    CONSTRAINT ck_loan_dates CHECK (end_date IS NULL OR end_date >= start_date)
);

CREATE TABLE Restoration (
    restoration_id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    artwork_id     NUMBER NOT NULL REFERENCES Artwork(artwork_id),
    staff_id       NUMBER NOT NULL REFERENCES Staff(staff_id),
    start_date     DATE NOT NULL,
    end_date       DATE,
    description    VARCHAR2(512),
    CONSTRAINT ck_restoration_dates CHECK (end_date IS NULL OR end_date >= start_date)
);

CREATE TABLE Insurance_Policy (
    policy_id             NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    provider              VARCHAR2(128) NOT NULL,
    start_date            DATE NOT NULL,
    end_date              DATE NOT NULL,
    total_coverage_amount NUMBER(14,2),
    CONSTRAINT ck_policy_dates CHECK (end_date >= start_date)
);

CREATE TABLE Insurance (
    insurance_id   NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    artwork_id     NUMBER NOT NULL REFERENCES Artwork(artwork_id),
    policy_id      NUMBER NOT NULL REFERENCES Insurance_Policy(policy_id),
    insured_amount NUMBER(14,2) NOT NULL,
    CONSTRAINT ck_insured_amount_pos CHECK (insured_amount > 0)
);

CREATE TABLE GALLERY_REVIEW (
    review_id     NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    visitor_id    NUMBER NOT NULL REFERENCES Visitor(visitor_id),
    artwork_id    NUMBER REFERENCES Artwork(artwork_id),
    exhibition_id NUMBER NOT NULL REFERENCES Exhibition(exhibition_id),
    rating        NUMBER(1) NOT NULL CHECK (rating BETWEEN 1 AND 5),
    review_text   VARCHAR2(256),
    review_date   DATE NOT NULL
);

CREATE TABLE ARTWORK_EXHIBITION (
    artwork_id          NUMBER NOT NULL REFERENCES Artwork(artwork_id),
    exhibition_id       NUMBER NOT NULL REFERENCES Exhibition(exhibition_id),
    position_in_gallery VARCHAR2(64),
    featured_status     VARCHAR2(16),
    CONSTRAINT pk_artwork_exhibition PRIMARY KEY (artwork_id, exhibition_id)
);

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
    description   VARCHAR2(512),
    CONSTRAINT ck_exhibition_eu_dates CHECK (end_date >= start_date)
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
    conditions   VARCHAR2(512),
    CONSTRAINT ck_loan_eu_dates CHECK (end_date IS NULL OR end_date >= start_date)
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

CREATE TABLE ARTIST_EU (artist_id NUMBER PRIMARY KEY, name VARCHAR2(128) NOT NULL, nationality VARCHAR2(64), birth_year NUMBER(4), death_year NUMBER(4));
CREATE TABLE COLLECTION_EU (collection_id NUMBER PRIMARY KEY, name VARCHAR2(128) NOT NULL, description VARCHAR2(512), created_date DATE);


-- Creare Tabele comune (schema global)
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
