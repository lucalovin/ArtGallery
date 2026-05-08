-- Apply additional GLOBAL_* views needed for the global schema to expose
-- all distributed data through a single relational interface.
-- Run as ARTGALLERY_GLOBAL.

CREATE OR REPLACE VIEW GLOBAL_EXHIBITION AS
SELECT exhibition_id, title, start_date, end_date, exhibitor_id, description
  FROM EXHIBITION_EU@link_eu
UNION ALL
SELECT exhibition_id, title, start_date, end_date, exhibitor_id, description
  FROM EXHIBITION_AM@link_am;

CREATE OR REPLACE VIEW GLOBAL_ARTWORK_EXHIBITION AS
SELECT artwork_id, exhibition_id, position_in_gallery, featured_status
  FROM ARTWORK_EXHIBITION_EU@link_eu
UNION ALL
SELECT artwork_id, exhibition_id, position_in_gallery, featured_status
  FROM ARTWORK_EXHIBITION_AM@link_am;

CREATE OR REPLACE VIEW GLOBAL_LOAN AS
SELECT loan_id, artwork_id, exhibitor_id, start_date, end_date, conditions
  FROM LOAN_EU@link_eu
UNION ALL
SELECT loan_id, artwork_id, exhibitor_id, start_date, end_date, conditions
  FROM LOAN_AM@link_am;

CREATE OR REPLACE VIEW GLOBAL_GALLERY_REVIEW AS
SELECT review_id, visitor_id, artwork_id, exhibition_id, rating, review_text, review_date
  FROM GALLERY_REVIEW_EU@link_eu
UNION ALL
SELECT review_id, visitor_id, artwork_id, exhibition_id, rating, review_text, review_date
  FROM GALLERY_REVIEW_AM@link_am;

CREATE OR REPLACE VIEW GLOBAL_ARTIST AS
SELECT * FROM ARTIST_EU@link_eu;

CREATE OR REPLACE VIEW GLOBAL_COLLECTION AS
SELECT * FROM COLLECTION_EU@link_eu;

EXIT;
