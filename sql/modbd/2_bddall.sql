-- =============================================================
-- Script de rulat pe DB1 (ORCLPDB) ca BDDALL
-- =============================================================
-- Conectare exemplu:
--   sqlplus BDDALL/1234@//localhost:1521/ORCLPDB

-- Creare Tabele
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

-- Inserare Date
INSERT INTO Artist (name, nationality, birth_year, death_year) VALUES ('Pablo Picasso', 'Spanish', 1881, 1973);
INSERT INTO Artist (name, nationality, birth_year, death_year) VALUES ('Vincent van Gogh', 'Dutch', 1853, 1890);
INSERT INTO Artist (name, nationality, birth_year, death_year) VALUES ('Claude Monet', 'French', 1840, 1926);
INSERT INTO Artist (name, nationality, birth_year, death_year) VALUES ('Salvador Dali', 'Spanish', 1904, 1989);

INSERT INTO Collection (name, description, created_date) VALUES ('Modern Masters', 'Key works of modern art', DATE '2020-01-15');
INSERT INTO Collection (name, description, created_date) VALUES ('Impressionist Highlights', 'Selected impressionist paintings', DATE '2021-03-10');
INSERT INTO Collection (name, description, created_date) VALUES ('Surreal Visions', 'Surrealist paintings and objects', DATE '2022-05-20');

INSERT INTO Location (name, gallery_room, type, capacity) VALUES ('Main Hall', 'R1', 'Exhibit', 50);
INSERT INTO Location (name, gallery_room, type, capacity) VALUES ('East Wing', 'R2', 'Exhibit', 40);
INSERT INTO Location (name, gallery_room, type, capacity) VALUES ('Storage A', 'S1', 'Storage', 200);

INSERT INTO Visitor (name, email, phone, membership_type, join_date) VALUES ('Alice Johnson', 'alice@example.com', '+40111111111', 'Standard', DATE '2023-01-10');
INSERT INTO Visitor (name, email, phone, membership_type, join_date) VALUES ('Bob Smith', 'bob@example.com', '+40111111112', 'VIP', DATE '2023-02-15');

INSERT INTO Exhibitor (name, address, city, contact_info) VALUES ('Louvre Museum', 'Rue de Rivoli', 'Paris', 'contact@louvre.fr');
INSERT INTO Exhibitor (name, address, city, contact_info) VALUES ('MoMA', '11 W 53rd St', 'New York', 'info@moma.org');
INSERT INTO Exhibitor (name, address, city, contact_info) VALUES ('Tate Modern', 'Bankside', 'London', 'contact@tate.org.uk');
INSERT INTO Exhibitor (name, address, city, contact_info) VALUES ('Reina Sofia', 'Calle de Santa Isabel', 'Madrid', 'info@museoreinasofia.es');

INSERT INTO Staff (name, role, hire_date, certification_level) VALUES ('Elena Curator', 'Curator', DATE '2020-01-01', 'Level 2');
INSERT INTO Staff (name, role, hire_date, certification_level) VALUES ('Mihai Restorer', 'Restorer', DATE '2021-05-10', 'Level 3');

INSERT INTO Insurance_Policy (provider, start_date, end_date, total_coverage_amount) VALUES ('Global Insurance', DATE '2022-01-01', DATE '2025-01-01', 3000000);

INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value) 
VALUES ('Les Demoiselles d''Avignon', 1, 1907, 'Oil on Canvas', 1, 1, 1000000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value) 
VALUES ('Guernica', 1, 1937, 'Oil on Canvas', 1, 2, 1500000);
INSERT INTO Artwork (title, artist_id, year_created, medium, collection_id, location_id, estimated_value) 
VALUES ('Starry Night', 2, 1889, 'Oil on Canvas', 2, 1, 1200000);

INSERT INTO Exhibition (title, start_date, end_date, exhibitor_id, description) 
VALUES ('Modern Icons', DATE '2024-01-10', DATE '2024-03-10', 1, 'Masterpieces of modern art');
INSERT INTO Exhibition (title, start_date, end_date, exhibitor_id, description) 
VALUES ('Impressionist Seasons', DATE '2024-04-01', DATE '2024-06-30', 2, 'Key impressionist works');

INSERT INTO Acquisition (artwork_id, acquisition_date, acquisition_type, price, staff_id) VALUES (1, DATE '2019-01-10', 'Purchase', 800000, 1);
INSERT INTO Loan (artwork_id, exhibitor_id, start_date, end_date, conditions) VALUES (2, 2, DATE '2023-01-01', DATE '2023-03-01', 'Standard insurance');
INSERT INTO Restoration (artwork_id, staff_id, start_date, end_date, description) VALUES (1, 2, DATE '2022-01-10', DATE '2022-02-10', 'Varnish cleaning');
INSERT INTO Insurance (artwork_id, policy_id, insured_amount) VALUES (1, 1, 900000);
INSERT INTO Gallery_Review (visitor_id, artwork_id, exhibition_id, rating, review_text, review_date) VALUES (1, 1, 1, 5, 'Amazing!', DATE '2024-01-20');
INSERT INTO Artwork_Exhibition (artwork_id, exhibition_id, position_in_gallery, featured_status) VALUES (1, 1, 'Room 1', 'Featured');

COMMIT;