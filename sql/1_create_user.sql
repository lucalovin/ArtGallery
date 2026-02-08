
alter session set container = orclpdb;
alter pluggable database orclpdb open;


CREATE USER ART_GALLERY_DW
  IDENTIFIED BY ART_GALLERY_DW
  DEFAULT TABLESPACE USERS
  TEMPORARY TABLESPACE TEMP
  QUOTA UNLIMITED ON USERS;

GRANT CONNECT, RESOURCE TO ART_GALLERY_DW;


ALTER USER ART_GALLERY_DW IDENTIFIED BY ART_GALLERY_DW ACCOUNT UNLOCK;


SELECT username, account_status
FROM dba_users
WHERE username = 'ART_GALLERY_DW';


ALTER USER ART_GALLERY_DW IDENTIFIED BY ART_GALLERY_DW;

GRANT CREATE SESSION TO ART_GALLERY_DW;
GRANT CREATE TABLE, CREATE VIEW, CREATE SEQUENCE TO ART_GALLERY_DW;
GRANT UNLIMITED TABLESPACE TO ART_GALLERY_DW



CREATE USER ART_GALLERY_OLTP
  IDENTIFIED BY ART_GALLERY_OLTP
  DEFAULT TABLESPACE USERS
  TEMPORARY TABLESPACE TEMP
  QUOTA UNLIMITED ON USERS;
  
GRANT CONNECT, RESOURCE TO ART_GALLERY_OLTP;
ALTER USER ART_GALLERY_OLTP IDENTIFIED BY ART_GALLERY_OLTP ACCOUNT UNLOCK;
GRANT CREATE SESSION TO ART_GALLERY_OLTP;
GRANT CREATE TABLE, CREATE VIEW, CREATE SEQUENCE TO ART_GALLERY_OLTP;
GRANT UNLIMITED TABLESPACE TO ART_GALLERY_OLTP;
GRANT CREATE MATERIALIZED VIEW TO ART_GALLERY_OLTP;




GRANT CREATE DIMENSION TO ART_GALLERY_DW;


GRANT SELECT ON ART_GALLERY_OLTP.ARTIST     TO ART_GALLERY_DW;
GRANT SELECT ON ART_GALLERY_OLTP.ARTWORK    TO ART_GALLERY_DW;
GRANT SELECT ON ART_GALLERY_OLTP.EXHIBITION TO ART_GALLERY_DW;
GRANT SELECT ON ART_GALLERY_OLTP.LOCATION   TO ART_GALLERY_DW;
GRANT SELECT ON ART_GALLERY_OLTP.GALLERY_REVIEW TO ART_GALLERY_DW;

SELECT username, account_status
FROM dba_users
WHERE username = 'ART_GALLERY_OLTP';


--CREATE
CREATE TABLE Artist (
    artist_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_artist PRIMARY KEY,
    name VARCHAR2(128) NOT NULL,
    nationality VARCHAR2(64),
    birth_year NUMBER(4),
    death_year NUMBER(4)
);

CREATE TABLE Collection (
    collection_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_collection PRIMARY KEY,
    name VARCHAR2(128) NOT NULL,
    description VARCHAR2(512),
    created_date DATE
);

CREATE TABLE Location (
    location_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_location PRIMARY KEY,
    name VARCHAR2(128) NOT NULL,
    gallery_room VARCHAR2(32),
    type VARCHAR2(32),
    capacity NUMBER,
    CONSTRAINT ck_location_capacity_pos
    CHECK (capacity IS NULL OR capacity > 0)
);

CREATE TABLE Artwork (
    artwork_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_artwork PRIMARY KEY,
    title VARCHAR2(128) NOT NULL,
    artist_id NUMBER NOT NULL,
    year_created NUMBER(4),
    medium VARCHAR2(64),
    collection_id NUMBER,
    location_id NUMBER,
    estimated_value NUMBER(12,2),
    CONSTRAINT fk_artwork_artist
    FOREIGN KEY (artist_id) REFERENCES Artist(artist_id),
    CONSTRAINT fk_artwork_collection
    FOREIGN KEY (collection_id) REFERENCES Collection(collection_id),
    CONSTRAINT fk_artwork_location
    FOREIGN KEY (location_id) REFERENCES Location(location_id)
);

CREATE TABLE Visitor (
    visitor_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_visitor PRIMARY KEY,
    name VARCHAR2(128) NOT NULL,
    email VARCHAR2(128),
    phone VARCHAR2(32),
    membership_type VARCHAR2(32),
    join_date DATE
);

CREATE TABLE Exhibitor (
    exhibitor_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_exhibitor PRIMARY KEY,
    name VARCHAR2(128) NOT NULL,
    address VARCHAR2(256),
    city VARCHAR2(64),
    contact_info VARCHAR2(256)
);

CREATE TABLE Exhibition (
    exhibition_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_exhibition PRIMARY KEY,
    title VARCHAR2(128) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    exhibitor_id NUMBER NOT NULL,
    description VARCHAR2(512),
    CONSTRAINT fk_exhibition_exhibitor
    FOREIGN KEY (exhibitor_id) REFERENCES Exhibitor(exhibitor_id),
    CONSTRAINT ck_exhibition_dates
    CHECK (end_date >= start_date)
);

CREATE TABLE Staff (
    staff_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_staff PRIMARY KEY,
    name VARCHAR2(128) NOT NULL,
    role VARCHAR2(64) NOT NULL,
    hire_date DATE NOT NULL,
    certification_level VARCHAR2(32)
);

CREATE TABLE Acquisition (
    acquisition_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_acquisition PRIMARY KEY,
    artwork_id NUMBER NOT NULL,
    acquisition_date DATE NOT NULL,
    acquisition_type VARCHAR2(32) NOT NULL,
    price NUMBER(12,2),
    staff_id NUMBER NOT NULL,
    CONSTRAINT fk_acquisition_artwork
    FOREIGN KEY (artwork_id) REFERENCES Artwork(artwork_id),
    CONSTRAINT fk_acquisition_staff
    FOREIGN KEY (staff_id) REFERENCES Staff(staff_id),
    CONSTRAINT ck_acquisition_price_pos
    CHECK (price IS NULL OR price >= 0),
    CONSTRAINT uq_acquisition_artwork UNIQUE (artwork_id)
);

CREATE TABLE Loan (
    loan_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_loan PRIMARY KEY,
    artwork_id NUMBER NOT NULL,
    exhibitor_id NUMBER NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    conditions VARCHAR2(512),
    CONSTRAINT fk_loan_artwork
    FOREIGN KEY (artwork_id) REFERENCES Artwork(artwork_id),
    CONSTRAINT fk_loan_exhibitor
    FOREIGN KEY (exhibitor_id) REFERENCES Exhibitor(exhibitor_id),
    CONSTRAINT ck_loan_dates
    CHECK (end_date IS NULL OR end_date >= start_date)
);

CREATE TABLE Restoration (
    restoration_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_restoration PRIMARY KEY,
    artwork_id NUMBER NOT NULL,
    staff_id NUMBER NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    description VARCHAR2(512),
    CONSTRAINT fk_restoration_artwork
    FOREIGN KEY (artwork_id) REFERENCES Artwork(artwork_id),
    CONSTRAINT fk_restoration_staff
    FOREIGN KEY (staff_id) REFERENCES Staff(staff_id),
    CONSTRAINT ck_restoration_dates
    CHECK (end_date IS NULL OR end_date >= start_date)
);

CREATE TABLE Insurance_Policy (
    policy_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_insurance_policy PRIMARY KEY,
    provider VARCHAR2(128) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_coverage_amount NUMBER(14,2),
    CONSTRAINT ck_policy_dates
    CHECK (end_date >= start_date),
    CONSTRAINT ck_policy_total_cov_pos
    CHECK (total_coverage_amount IS NULL OR total_coverage_amount >= 0)
);

CREATE TABLE Insurance (
    insurance_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_insurance PRIMARY KEY,
    artwork_id NUMBER NOT NULL,
    policy_id NUMBER NOT NULL,
    insured_amount NUMBER(14,2) NOT NULL,
    CONSTRAINT fk_insurance_artwork
    FOREIGN KEY (artwork_id) REFERENCES Artwork(artwork_id),
    CONSTRAINT fk_insurance_policy
    FOREIGN KEY (policy_id) REFERENCES Insurance_Policy(policy_id),
    CONSTRAINT ck_insured_amount_pos
    CHECK (insured_amount > 0)
);

CREATE TABLE GALLERY_REVIEW (
    review_id NUMBER GENERATED ALWAYS AS IDENTITY
    CONSTRAINT pk_gallery_review PRIMARY KEY,
    visitor_id NUMBER NOT NULL,
    artwork_id NUMBER,
    exhibition_id NUMBER,
    rating NUMBER(1) NOT NULL,
    review_text VARCHAR2(256),
    review_date DATE NOT NULL,
    CONSTRAINT fk_review_visitor
    FOREIGN KEY (visitor_id) REFERENCES Visitor(visitor_id),
    CONSTRAINT fk_review_artwork
    FOREIGN KEY (artwork_id) REFERENCES Artwork(artwork_id),
    CONSTRAINT fk_review_exhibition
    FOREIGN KEY (exhibition_id) REFERENCES Exhibition(exhibition_id),
    CONSTRAINT ck_review_rating
    CHECK (rating BETWEEN 1 AND 5)
);

CREATE TABLE ARTWORK_EXHIBITION (
    artwork_id NUMBER NOT NULL,
    exhibition_id NUMBER NOT NULL,
    position_in_gallery VARCHAR2(64),
    featured_status VARCHAR2(16),
    CONSTRAINT pk_artwork_exhibition
    PRIMARY KEY (artwork_id, exhibition_id),
    CONSTRAINT fk_ax_artwork
    FOREIGN KEY (artwork_id) REFERENCES Artwork(artwork_id),
    CONSTRAINT fk_ax_exhibition
    FOREIGN KEY (exhibition_id) REFERENCES Exhibition(exhibition_id)
);
/



GRANT SELECT ON Artist           TO ART_GALLERY_DW;
GRANT SELECT ON Collection       TO ART_GALLERY_DW;
GRANT SELECT ON Location         TO ART_GALLERY_DW;
GRANT SELECT ON Exhibitor        TO ART_GALLERY_DW;
GRANT SELECT ON Exhibition       TO ART_GALLERY_DW;
GRANT SELECT ON Artwork          TO ART_GALLERY_DW;
GRANT SELECT ON Insurance_Policy TO ART_GALLERY_DW;
GRANT SELECT ON Insurance        TO ART_GALLERY_DW;
GRANT SELECT ON Loan             TO ART_GALLERY_DW;
GRANT SELECT ON Restoration      TO ART_GALLERY_DW;
GRANT SELECT ON Gallery_Review   TO ART_GALLERY_DW;
GRANT SELECT ON Artwork_Exhibition TO ART_GALLERY_DW;


-- ARTIST
INSERT INTO Artist (name, nationality, birth_year, death_year) VALUES
('Pablo Picasso', 'Spanish', 1881, 1973);
INSERT INTO Artist (name, nationality, birth_year, death_year) VALUES
('Vincent van Gogh', 'Dutch', 1853, 1890);
INSERT INTO Artist (name, nationality, birth_year, death_year) VALUES
('Claude Monet', 'French', 1840, 1926);
INSERT INTO Artist (name, nationality, birth_year, death_year) VALUES
('Salvador Dali', 'Spanish', 1904, 1989);

-- COLLECTION
INSERT INTO Collection (name, description, created_date) VALUES
('Modern Masters', 'Key works of modern art', DATE '2020-01-15');
INSERT INTO Collection (name, description, created_date) VALUES
('Impressionist Highlights', 'Selected impressionist paintings', DATE '2021-03-10');
INSERT INTO Collection (name, description, created_date) VALUES
('Surreal Visions', 'Surrealist paintings and objects', DATE '2022-05-20');
INSERT INTO Collection (name, description, created_date) VALUES
('Permanent Collection', 'Core museum holdings', DATE '2019-09-01');

-- LOCATION
INSERT INTO Location (name, gallery_room, type, capacity) VALUES
('Main Hall', 'R1', 'Exhibit', 50);
INSERT INTO Location (name, gallery_room, type, capacity) VALUES
('East Wing', 'R2', 'Exhibit', 40);
INSERT INTO Location (name, gallery_room, type, capacity) VALUES
('Storage A', 'S1', 'Storage', 200);
INSERT INTO Location (name, gallery_room, type, capacity) VALUES
('Storage B', 'S2', 'Storage', 150);

-- VISITOR
INSERT INTO Visitor (name, email, phone, membership_type, join_date) VALUES
('Alice Johnson', 'alice@example.com', '+40111111111', 'Standard', DATE '2023-01-10');
INSERT INTO Visitor (name, email, phone, membership_type, join_date) VALUES
('Bob Smith', 'bob@example.com', '+40111111112', 'VIP', DATE '2023-02-15');
INSERT INTO Visitor (name, email, phone, membership_type, join_date) VALUES
('Carla Pop', 'carla@example.com', '+40111111113', 'Student', DATE '2023-03-20');
INSERT INTO Visitor (name, email, phone, membership_type, join_date) VALUES
('Dan Ionescu', 'dan@example.com', '+40111111114', 'Standard', DATE '2023-04-05');

-- EXHIBITOR
INSERT INTO Exhibitor (name, address, city, contact_info) VALUES
('Louvre Museum', 'Rue de Rivoli', 'Paris', 'contact@louvre.fr');
INSERT INTO Exhibitor (name, address, city, contact_info) VALUES
('MoMA', '11 W 53rd St', 'New York', 'info@moma.org');
INSERT INTO Exhibitor (name, address, city, contact_info) VALUES
('Tate Modern', 'Bankside', 'London', 'contact@tate.org.uk');
INSERT INTO Exhibitor (name, address, city, contact_info) VALUES
('Reina Sofia', 'Calle de Santa Isabel', 'Madrid', 'info@museoreinasofia.es');

-- STAFF
INSERT INTO Staff (name, role, hire_date, certification_level) VALUES
('Elena Curator', 'Curator', DATE '2020-01-01', 'Level 2');
INSERT INTO Staff (name, role, hire_date, certification_level) VALUES
('Mihai Restorer', 'Restorer', DATE '2021-05-10', 'Level 3');
INSERT INTO Staff (name, role, hire_date, certification_level) VALUES
('Ioana Registrar', 'Registrar', DATE '2022-03-15', 'Level 1');
INSERT INTO Staff (name, role, hire_date, certification_level) VALUES
('Andrei Manager', 'Manager', DATE '2019-09-01', 'Level 3');

-- INSURANCE_POLICY
INSERT INTO Insurance_Policy (provider, start_date, end_date, total_coverage_amount) VALUES
('Global Insurance', DATE '2022-01-01', DATE '2025-01-01', 3000000);
INSERT INTO Insurance_Policy (provider, start_date, end_date, total_coverage_amount) VALUES
('ArtSecure', DATE '2023-01-01', DATE '2026-01-01', 1500000);
INSERT INTO Insurance_Policy (provider, start_date, end_date, total_coverage_amount) VALUES
('FineArt Shield', DATE '2023-06-01', DATE '2027-06-01', 2000000);
INSERT INTO Insurance_Policy (provider, start_date, end_date, total_coverage_amount) VALUES
('Museum Protect', DATE '2024-01-01', DATE '2028-01-01', 2500000);

-- ARTWORK
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value)
VALUES ('Les Demoiselles d''Avignon', 1, 1907, 'Oil on Canvas', 1, 1, 1000000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value)
VALUES ('Guernica', 1, 1937, 'Oil on Canvas', 1, 2, 1500000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value)
VALUES ('Starry Night', 2, 1889, 'Oil on Canvas', 2, 1, 1200000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value)
VALUES ('Sunflowers', 2, 1888, 'Oil on Canvas', 2, 2, 800000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value)
VALUES ('Water Lilies', 3, 1916, 'Oil on Canvas', 2, 1, 900000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value)
VALUES ('Impression, Sunrise', 3, 1872, 'Oil on Canvas', 2, 2, 700000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value)
VALUES ('The Persistence of Memory', 4, 1931, 'Oil on Canvas', 3, 1, 600000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value)
VALUES ('Swans Reflecting Elephants', 4, 1937, 'Oil on Canvas', 3, 2, 550000);

-- EXHIBITION
INSERT INTO Exhibition (title, start_date, end_date, exhibitor_id, description)
VALUES ('Modern Icons', DATE '2024-01-10', DATE '2024-03-10', 1, 'Masterpieces of modern art');
INSERT INTO Exhibition (title, start_date, end_date, exhibitor_id, description)
VALUES ('Impressionist Seasons', DATE '2024-04-01', DATE '2024-06-30', 2, 'Key impressionist works');
INSERT INTO Exhibition (title, start_date, end_date, exhibitor_id, description)
VALUES ('Surreal Moments', DATE '2024-07-01', DATE '2024-09-15', 3, 'Surrealist paintings and sculptures');
INSERT INTO Exhibition (title, start_date, end_date, exhibitor_id, description)
VALUES ('Van Gogh Focus', DATE '2024-10-01', DATE '2024-12-31', 4, 'A selection of Van Gogh''s works');

-- ACQUISITION
INSERT INTO Acquisition (artwork_id, acquisition_date, acquisition_type, price, staff_id)
VALUES (1, DATE '2019-01-10', 'Purchase', 800000, 3);
INSERT INTO Acquisition (artwork_id, acquisition_date, acquisition_type, price, staff_id)
VALUES (3, DATE '2018-06-05', 'Purchase', 900000, 3);
INSERT INTO Acquisition (artwork_id, acquisition_date, acquisition_type, price, staff_id)
VALUES (5, DATE '2017-09-20', 'Donation', NULL, 4);
INSERT INTO Acquisition (artwork_id, acquisition_date, acquisition_type, price, staff_id)
VALUES (7, DATE '2020-11-30', 'Purchase', 500000, 1);

-- LOAN
INSERT INTO Loan (artwork_id, exhibitor_id, start_date, end_date, conditions)
VALUES (2, 2, DATE '2023-01-01', DATE '2023-03-01', 'Standard insurance');
INSERT INTO Loan (artwork_id, exhibitor_id, start_date, end_date, conditions)
VALUES (4, 1, DATE '2023-02-15', DATE '2023-04-15', 'Climate control required');
INSERT INTO Loan (artwork_id, exhibitor_id, start_date, end_date, conditions)
VALUES (6, 3, DATE '2023-05-01', DATE '2023-07-01', 'Framed transport');
INSERT INTO Loan (artwork_id, exhibitor_id, start_date, end_date, conditions)
VALUES (8, 4, DATE '2023-08-01', DATE '2023-10-01', 'Handle with care');

--RESTORATION
INSERT INTO Restoration (artwork_id, staff_id, start_date, end_date, description)
VALUES (1, 2, DATE '2022-01-10', DATE '2022-02-10', 'Varnish cleaning');
INSERT INTO Restoration (artwork_id, staff_id, start_date, end_date, description)
VALUES (3, 2, DATE '2021-03-01', DATE '2021-04-01', 'Canvas stabilization');
INSERT INTO Restoration (artwork_id, staff_id, start_date, end_date, description)
VALUES (5, 2, DATE '2020-09-15', DATE '2020-10-15', 'Color retouching');
INSERT INTO Restoration (artwork_id, staff_id, start_date, end_date, description)
VALUES (7, 2, DATE '2023-01-05', DATE '2023-02-05', 'Frame repair');

--INSURANCE
INSERT INTO Insurance (artwork_id, policy_id, insured_amount)
VALUES (1, 1, 900000);
INSERT INTO Insurance (artwork_id, policy_id, insured_amount)
VALUES (2, 1, 1200000);
INSERT INTO Insurance (artwork_id, policy_id, insured_amount)
VALUES (3, 2, 1000000);
INSERT INTO Insurance (artwork_id, policy_id, insured_amount)
VALUES (4, 2, 700000);
INSERT INTO Insurance (artwork_id, policy_id, insured_amount)
VALUES (5, 3, 800000);
INSERT INTO Insurance (artwork_id, policy_id, insured_amount)
VALUES (6, 3, 600000);
INSERT INTO Insurance (artwork_id, policy_id, insured_amount)
VALUES (7, 4, 500000);
INSERT INTO Insurance (artwork_id, policy_id, insured_amount)
VALUES (8, 4, 450000);

--GALLERY_REVIEW
INSERT INTO Gallery_Review (visitor_id, artwork_id, exhibition_id, rating, review_text, review_date)
VALUES (1, 1, 1, 5, 'Amazing piece!', DATE '2024-01-20');
INSERT INTO Gallery_Review (visitor_id, artwork_id, exhibition_id, rating, review_text, review_date)
VALUES (2, 3, 2, 4, 'Great impressionist selection.', DATE '2024-04-15');
INSERT INTO Gallery_Review (visitor_id, artwork_id, exhibition_id, rating, review_text, review_date)
VALUES (3, NULL, 3, 4, 'Interesting surrealist show.', DATE '2024-07-10');
INSERT INTO Gallery_Review (visitor_id, artwork_id, exhibition_id, rating, review_text, review_date)
VALUES (4, 7, 3, 5, 'Loved the Dali works.', DATE '2024-07-12');

--ARTWORK_EXHIBITION
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (1, 1, 'Room 1 - Center', 'Featured');
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (2, 1, 'Room 1 - Left', 'Regular');
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (3, 2, 'Room 2 - Center', 'Featured');
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (4, 2, 'Room 2 - Right', 'Regular');
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (5, 2, 'Room 3 - Left', 'Regular');
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (7, 3, 'Room 4 - Center', 'Featured');
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (8, 3, 'Room 4 - Right', 'Regular');
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (3, 4, 'Room 5 - Center', 'Featured');



CREATE TABLE DIM_DATE (
  DATE_KEY        NUMBER(8)      PRIMARY KEY,          
  CALENDAR_DATE   DATE           NOT NULL,
  CALENDAR_YEAR   NUMBER(4)      NOT NULL,
  CALENDAR_MONTH  NUMBER(2)      NOT NULL,
  CALENDAR_DAY    NUMBER(2)      NOT NULL,
  MONTH_NAME      VARCHAR2(20),
  QUARTER         NUMBER(1),
  IS_WEEKEND      CHAR(1)
);--


CREATE TABLE DIM_ARTIST (
  ARTIST_KEY      NUMBER          PRIMARY KEY,
  ARTIST_ID_OLTP  NUMBER,
  NAME            VARCHAR2(128)   NOT NULL,
  NATIONALITY     VARCHAR2(64),
  BIRTH_YEAR      NUMBER(4),
  DEATH_YEAR      NUMBER(4)
);

CREATE TABLE DIM_COLLECTION (
  COLLECTION_KEY      NUMBER          PRIMARY KEY,
  COLLECTION_ID_OLTP  NUMBER,
  NAME                VARCHAR2(128)   NOT NULL,
  DESCRIPTION         VARCHAR2(512),
  CREATED_DATE_KEY    NUMBER(8)      
);--no

CREATE TABLE DIM_LOCATION (
  LOCATION_KEY      NUMBER          PRIMARY KEY,
  LOCATION_ID_OLTP  NUMBER,
  NAME              VARCHAR2(128)   NOT NULL,
  GALLERY_ROOM      VARCHAR2(32),
  TYPE              VARCHAR2(32),
  CAPACITY          NUMBER
);--

CREATE TABLE DIM_EXHIBITOR (
  EXHIBITOR_KEY      NUMBER          PRIMARY KEY,
  EXHIBITOR_ID_OLTP  NUMBER,
  NAME               VARCHAR2(128)   NOT NULL,
  ADDRESS            VARCHAR2(256),
  CITY               VARCHAR2(64),
  CONTACT_INFO       VARCHAR2(256)
);--no

CREATE TABLE DIM_EXHIBITION (
  EXHIBITION_KEY      NUMBER          PRIMARY KEY,
  EXHIBITION_ID_OLTP  NUMBER,
  TITLE               VARCHAR2(128)   NOT NULL,
  START_DATE_KEY      NUMBER(8),
  END_DATE_KEY        NUMBER(8),
  EXHIBITOR_KEY       NUMBER,
  DESCRIPTION         VARCHAR2(512)
);--
--exhibition.description nu e in analiza

CREATE TABLE DIM_ARTWORK (
  ARTWORK_KEY       NUMBER          PRIMARY KEY,
  ARTWORK_ID_OLTP   NUMBER,
  TITLE             VARCHAR2(128)   NOT NULL,
  ARTIST_KEY        NUMBER          NOT NULL,
  YEAR_CREATED      NUMBER(4),
  MEDIUM            VARCHAR2(64),
  COLLECTION_KEY    NUMBER,
  LOCATION_KEY      NUMBER,
  ESTIMATED_VALUE   NUMBER(12,2)
);--

CREATE TABLE DIM_POLICY (
  POLICY_KEY          NUMBER          PRIMARY KEY,
  POLICY_ID_OLTP      NUMBER,
  PROVIDER            VARCHAR2(128)   NOT NULL,
  START_DATE_KEY      NUMBER(8),
  END_DATE_KEY        NUMBER(8),
  TOTAL_COVERAGE_AMT  NUMBER(14,2)
);--no

CREATE TABLE FACT_EXHIBITION_ACTIVITY (
  FACT_KEY               NUMBER        PRIMARY KEY,
  DATE_KEY               NUMBER(8)     NOT NULL,  
  EXHIBITION_KEY         NUMBER        NOT NULL,
  EXHIBITOR_KEY          NUMBER        NOT NULL,
  ARTWORK_KEY            NUMBER        NOT NULL,
  ARTIST_KEY             NUMBER        NOT NULL,
  COLLECTION_KEY         NUMBER,
  LOCATION_KEY           NUMBER,
  POLICY_KEY             NUMBER,

  ESTIMATED_VALUE        NUMBER(12,2),
  INSURED_AMOUNT         NUMBER(14,2),
  LOAN_FLAG              NUMBER(1),      
  RESTORATION_COUNT      NUMBER(10),     
  REVIEW_COUNT           NUMBER(10),
  AVG_RATING             NUMBER(5,2)
);
