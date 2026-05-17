
------------------------------------------------------------
cerinta 3
------------------------------------------------------------



-- Inserare Date  bddall
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

-- am

INSERT INTO EXHIBITOR_AM SELECT * FROM BDDALL.Exhibitor WHERE city = 'New York';
INSERT INTO EXHIBITION_AM SELECT * FROM BDDALL.Exhibition WHERE exhibitor_id IN (SELECT exhibitor_id FROM EXHIBITOR_AM);
INSERT INTO ARTWORK_DETAILS (artwork_id, location_id, estimated_value) SELECT artwork_id, location_id, estimated_value FROM BDDALL.Artwork; 
INSERT INTO ARTWORK_EXHIBITION_AM SELECT * FROM BDDALL.Artwork_Exhibition WHERE exhibition_id IN (SELECT exhibition_id FROM EXHIBITION_AM);
INSERT INTO LOAN_AM SELECT * FROM BDDALL.Loan WHERE exhibitor_id IN (SELECT exhibitor_id FROM EXHIBITOR_AM);
INSERT INTO GALLERY_REVIEW_AM SELECT * FROM BDDALL.Gallery_Review WHERE exhibition_id IN (SELECT exhibition_id FROM EXHIBITION_AM);
INSERT INTO ARTIST_AM SELECT * FROM BDDALL.Artist;
INSERT INTO COLLECTION_AM SELECT * FROM BDDALL.Collection;


COMMIT;

-- eu

INSERT INTO EXHIBITOR_EU SELECT * FROM Exhibitor@link_bddall WHERE city IN ('Paris', 'London', 'Madrid');
INSERT INTO ARTWORK_CORE (artwork_id, title, artist_id, year_created, medium, collection_id)
SELECT artwork_id, title, artist_id, year_created, medium, collection_id FROM Artwork@link_bddall;
INSERT INTO EXHIBITION_EU SELECT * FROM Exhibition@link_bddall WHERE exhibitor_id IN (SELECT exhibitor_id FROM EXHIBITOR_EU);
INSERT INTO ARTWORK_EXHIBITION_EU SELECT * FROM Artwork_Exhibition@link_bddall WHERE exhibition_id IN (SELECT exhibition_id FROM EXHIBITION_EU);
INSERT INTO LOAN_EU SELECT * FROM Loan@link_bddall WHERE exhibitor_id IN (SELECT exhibitor_id FROM EXHIBITOR_EU);
INSERT INTO GALLERY_REVIEW_EU SELECT * FROM Gallery_Review@link_bddall WHERE exhibition_id IN (SELECT exhibition_id FROM EXHIBITION_EU);
INSERT INTO ARTIST_EU SELECT * FROM Artist@link_bddall;
INSERT INTO COLLECTION_EU SELECT * FROM Collection@link_bddall;

COMMIT;

-- global

INSERT INTO LOCATION SELECT * FROM BDDALL.Location;
INSERT INTO VISITOR SELECT * FROM BDDALL.Visitor;
INSERT INTO STAFF SELECT * FROM BDDALL.Staff;
INSERT INTO INSURANCE_POLICY SELECT * FROM BDDALL.Insurance_Policy;
INSERT INTO INSURANCE SELECT * FROM BDDALL.Insurance;
INSERT INTO RESTORATION SELECT * FROM BDDALL.Restoration;
INSERT INTO ACQUISITION SELECT * FROM BDDALL.Acquisition;
COMMIT;


