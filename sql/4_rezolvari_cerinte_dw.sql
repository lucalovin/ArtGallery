

------4

DECLARE
  v_start_date DATE := DATE '2015-01-01';
  v_end_date   DATE := DATE '2030-12-31';
BEGIN
  FOR d IN 0 .. (v_end_date - v_start_date) LOOP
    INSERT INTO DIM_DATE (
      DATE_KEY,
      CALENDAR_DATE,
      CALENDAR_YEAR,
      CALENDAR_MONTH,
      CALENDAR_DAY,
      MONTH_NAME,
      QUARTER,
      IS_WEEKEND
    )
    VALUES (
      TO_NUMBER(TO_CHAR(v_start_date + d, 'YYYYMMDD')),
      v_start_date + d,
      TO_NUMBER(TO_CHAR(v_start_date + d, 'YYYY')),
      TO_NUMBER(TO_CHAR(v_start_date + d, 'MM')),
      TO_NUMBER(TO_CHAR(v_start_date + d, 'DD')),
      TO_CHAR(v_start_date + d, 'MONTH'),
      TO_NUMBER(TO_CHAR(v_start_date + d, 'Q')),
      CASE
        WHEN TO_CHAR(v_start_date + d, 'DY', 'NLS_DATE_LANGUAGE=ENGLISH') IN ('SAT','SUN')
        THEN 'Y'
        ELSE 'N'
      END
    );
  END LOOP;
  COMMIT;
END;
/


INSERT INTO DIM_ARTIST (
  ARTIST_KEY,
  ARTIST_ID_OLTP,
  NAME,
  NATIONALITY,
  BIRTH_YEAR,
  DEATH_YEAR
)
SELECT
  artist_id         AS ARTIST_KEY,
  artist_id         AS ARTIST_ID_OLTP,
  name,
  nationality,
  birth_year,
  death_year
FROM art_gallery_oltp.Artist;

COMMIT;

INSERT INTO DIM_COLLECTION (
  COLLECTION_KEY,
  COLLECTION_ID_OLTP,
  NAME,
  DESCRIPTION,
  CREATED_DATE_KEY
)
SELECT
  c.collection_id                 AS COLLECTION_KEY,
  c.collection_id                 AS COLLECTION_ID_OLTP,
  c.name,
  c.description,
  CASE
    WHEN c.created_date IS NOT NULL
    THEN TO_NUMBER(TO_CHAR(c.created_date, 'YYYYMMDD'))
    ELSE NULL
  END                             AS CREATED_DATE_KEY
FROM art_gallery_oltp.Collection c;

COMMIT;


INSERT INTO DIM_LOCATION (
  LOCATION_KEY,
  LOCATION_ID_OLTP,
  NAME,
  GALLERY_ROOM,
  TYPE,
  CAPACITY
)
SELECT
  l.location_id       AS LOCATION_KEY,
  l.location_id       AS LOCATION_ID_OLTP,
  l.name,
  l.gallery_room,
  l.type,
  l.capacity
FROM art_gallery_oltp.Location l;

COMMIT;

INSERT INTO DIM_EXHIBITOR (
  EXHIBITOR_KEY,
  EXHIBITOR_ID_OLTP,
  NAME,
  ADDRESS,
  CITY,
  CONTACT_INFO
)
SELECT
  e.exhibitor_id      AS EXHIBITOR_KEY,
  e.exhibitor_id      AS EXHIBITOR_ID_OLTP,
  e.name,
  e.address,
  e.city,
  e.contact_info
FROM art_gallery_oltp.Exhibitor e;

COMMIT;


INSERT INTO DIM_EXHIBITION (
  EXHIBITION_KEY,
  EXHIBITION_ID_OLTP,
  TITLE,
  START_DATE_KEY,
  END_DATE_KEY,
  EXHIBITOR_KEY,
  DESCRIPTION
)
SELECT
  ex.exhibition_id                        AS EXHIBITION_KEY,
  ex.exhibition_id                        AS EXHIBITION_ID_OLTP,
  ex.title,
  TO_NUMBER(TO_CHAR(ex.start_date, 'YYYYMMDD')) AS START_DATE_KEY,
  TO_NUMBER(TO_CHAR(ex.end_date,   'YYYYMMDD')) AS END_DATE_KEY,
  ex.exhibitor_id                        AS EXHIBITOR_KEY,
  ex.description
FROM art_gallery_oltp.Exhibition ex;

COMMIT;

INSERT INTO DIM_ARTWORK (
  ARTWORK_KEY,
  ARTWORK_ID_OLTP,
  TITLE,
  ARTIST_KEY,
  YEAR_CREATED,
  MEDIUM,
  COLLECTION_KEY,
  LOCATION_KEY,
  ESTIMATED_VALUE
)
SELECT
  aw.artwork_id       AS ARTWORK_KEY,
  aw.artwork_id       AS ARTWORK_ID_OLTP,
  aw.title,
  aw.artist_id        AS ARTIST_KEY,
  aw.year_created,
  aw.medium,
  aw.collection_id    AS COLLECTION_KEY,
  aw.location_id      AS LOCATION_KEY,
  aw.estimated_value
FROM art_gallery_oltp.Artwork aw;

COMMIT;


INSERT INTO DIM_POLICY (
  POLICY_KEY,
  POLICY_ID_OLTP,
  PROVIDER,
  START_DATE_KEY,
  END_DATE_KEY,
  TOTAL_COVERAGE_AMT
)
SELECT
  p.policy_id                         AS POLICY_KEY,
  p.policy_id                         AS POLICY_ID_OLTP,
  p.provider,
  TO_NUMBER(TO_CHAR(p.start_date, 'YYYYMMDD')) AS START_DATE_KEY,
  TO_NUMBER(TO_CHAR(p.end_date,   'YYYYMMDD')) AS END_DATE_KEY,
  p.total_coverage_amount
FROM art_gallery_oltp.Insurance_Policy p;

COMMIT;


INSERT INTO FACT_EXHIBITION_ACTIVITY (
  FACT_KEY,
  DATE_KEY,
  EXHIBITION_KEY,
  EXHIBITOR_KEY,
  ARTWORK_KEY,
  ARTIST_KEY,
  COLLECTION_KEY,
  LOCATION_KEY,
  POLICY_KEY,
  ESTIMATED_VALUE,
  INSURED_AMOUNT,
  LOAN_FLAG,
  RESTORATION_COUNT,
  REVIEW_COUNT,
  AVG_RATING
)
SELECT
  ROW_NUMBER() OVER (ORDER BY ex.exhibition_id, aw.artwork_id) AS FACT_KEY,
  TO_NUMBER(TO_CHAR(ex.start_date, 'YYYYMMDD'))                AS DATE_KEY,
  ex.exhibition_id                                             AS EXHIBITION_KEY,
  ex.exhibitor_id                                              AS EXHIBITOR_KEY,
  aw.artwork_id                                                AS ARTWORK_KEY,
  aw.artist_id                                                 AS ARTIST_KEY,
  aw.collection_id                                             AS COLLECTION_KEY,
  aw.location_id                                               AS LOCATION_KEY,
  pol.policy_id                                                AS POLICY_KEY,
  aw.estimated_value                                           AS ESTIMATED_VALUE,
  NVL(ins.total_insured, 0)                                    AS INSURED_AMOUNT,
  CASE WHEN ln.loan_count > 0 THEN 1 ELSE 0 END                AS LOAN_FLAG,
  NVL(res.restoration_count, 0)                                AS RESTORATION_COUNT,
  NVL(rv.review_count, 0)                                      AS REVIEW_COUNT,
  NVL(rv.avg_rating, 0)                                        AS AVG_RATING
FROM art_gallery_oltp.Artwork_Exhibition ax
JOIN art_gallery_oltp.Artwork aw
  ON aw.artwork_id = ax.artwork_id
JOIN art_gallery_oltp.Exhibition ex
  ON ex.exhibition_id = ax.exhibition_id
LEFT JOIN (
  SELECT i.artwork_id,
         SUM(i.insured_amount) AS total_insured,
         MIN(i.policy_id)      AS policy_id
  FROM art_gallery_oltp.Insurance i
  GROUP BY i.artwork_id
) ins
  ON ins.artwork_id = aw.artwork_id
LEFT JOIN art_gallery_oltp.Insurance_Policy pol
  ON pol.policy_id = ins.policy_id
LEFT JOIN (
  SELECT l.artwork_id,
         COUNT(*) AS loan_count
  FROM art_gallery_oltp.Loan l
  GROUP BY l.artwork_id
) ln
  ON ln.artwork_id = aw.artwork_id
LEFT JOIN (
  SELECT r.artwork_id,
         COUNT(*)      AS restoration_count
  FROM art_gallery_oltp.Restoration r
  GROUP BY r.artwork_id
) res
  ON res.artwork_id = aw.artwork_id
LEFT JOIN (
  SELECT gr.exhibition_id,
         gr.artwork_id,
         COUNT(*)          AS review_count,
         AVG(gr.rating)    AS avg_rating
  FROM art_gallery_oltp.Gallery_Review gr
  GROUP BY gr.exhibition_id, gr.artwork_id
) rv
  ON rv.exhibition_id = ex.exhibition_id
 AND (rv.artwork_id = aw.artwork_id OR rv.artwork_id IS NULL);

COMMIT;



------5

ALTER TABLE DIM_DATE
  ADD CONSTRAINT ck_dim_date_year
  CHECK (CALENDAR_YEAR BETWEEN 1900 AND 2100);

ALTER TABLE DIM_COLLECTION
  ADD CONSTRAINT fk_dim_collection_created_date
  FOREIGN KEY (CREATED_DATE_KEY)
  REFERENCES DIM_DATE (DATE_KEY);
  
  
ALTER TABLE DIM_EXHIBITION
  ADD CONSTRAINT fk_dim_exhibition_start_date
  FOREIGN KEY (START_DATE_KEY)
  REFERENCES DIM_DATE (DATE_KEY);

ALTER TABLE DIM_EXHIBITION
  ADD CONSTRAINT fk_dim_exhibition_end_date
  FOREIGN KEY (END_DATE_KEY)
  REFERENCES DIM_DATE (DATE_KEY);

ALTER TABLE DIM_EXHIBITION
  ADD CONSTRAINT fk_dim_exhibition_exhibitor
  FOREIGN KEY (EXHIBITOR_KEY)
  REFERENCES DIM_EXHIBITOR (EXHIBITOR_KEY);




ALTER TABLE DIM_ARTWORK
  ADD CONSTRAINT fk_dim_artwork_artist
  FOREIGN KEY (ARTIST_KEY)
  REFERENCES DIM_ARTIST (ARTIST_KEY);

ALTER TABLE DIM_ARTWORK
  ADD CONSTRAINT fk_dim_artwork_collection
  FOREIGN KEY (COLLECTION_KEY)
  REFERENCES DIM_COLLECTION (COLLECTION_KEY);

ALTER TABLE DIM_ARTWORK
  ADD CONSTRAINT fk_dim_artwork_location
  FOREIGN KEY (LOCATION_KEY)
  REFERENCES DIM_LOCATION (LOCATION_KEY);
  
  
  
  
  ALTER TABLE DIM_POLICY
  ADD CONSTRAINT fk_dim_policy_start_date
  FOREIGN KEY (START_DATE_KEY)
  REFERENCES DIM_DATE (DATE_KEY);

ALTER TABLE DIM_POLICY
  ADD CONSTRAINT fk_dim_policy_end_date
  FOREIGN KEY (END_DATE_KEY)
  REFERENCES DIM_DATE (DATE_KEY);
  
  
  
ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT fk_fact_date
  FOREIGN KEY (DATE_KEY)
  REFERENCES DIM_DATE (DATE_KEY);

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT fk_fact_exhibition
  FOREIGN KEY (EXHIBITION_KEY)
  REFERENCES DIM_EXHIBITION (EXHIBITION_KEY);

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT fk_fact_exhibitor
  FOREIGN KEY (EXHIBITOR_KEY)
  REFERENCES DIM_EXHIBITOR (EXHIBITOR_KEY);

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT fk_fact_artwork
  FOREIGN KEY (ARTWORK_KEY)
  REFERENCES DIM_ARTWORK (ARTWORK_KEY);

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT fk_fact_artist
  FOREIGN KEY (ARTIST_KEY)
  REFERENCES DIM_ARTIST (ARTIST_KEY);

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT fk_fact_collection
  FOREIGN KEY (COLLECTION_KEY)
  REFERENCES DIM_COLLECTION (COLLECTION_KEY);

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT fk_fact_location
  FOREIGN KEY (LOCATION_KEY)
  REFERENCES DIM_LOCATION (LOCATION_KEY);

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT fk_fact_policy
  FOREIGN KEY (POLICY_KEY)
  REFERENCES DIM_POLICY (POLICY_KEY);

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT ck_fact_loan_flag
  CHECK (LOAN_FLAG IN (0, 1));

ALTER TABLE FACT_EXHIBITION_ACTIVITY
  ADD CONSTRAINT ck_fact_rating_range
  CHECK (AVG_RATING BETWEEN 0 AND 5);


-----6
CREATE INDEX idx_fact_date_key
  ON FACT_EXHIBITION_ACTIVITY (DATE_KEY);


EXPLAIN PLAN FOR
SELECT
  d.CALENDAR_YEAR,
  d.CALENDAR_MONTH,
  SUM(f.ESTIMATED_VALUE) AS total_value,
  SUM(f.REVIEW_COUNT)    AS total_reviews
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR = 2024
GROUP BY d.CALENDAR_YEAR, d.CALENDAR_MONTH
ORDER BY d.CALENDAR_YEAR, d.CALENDAR_MONTH;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);




CREATE INDEX idx_fact_exhibition_key
  ON FACT_EXHIBITION_ACTIVITY (EXHIBITION_KEY);

EXPLAIN PLAN FOR
SELECT
  ex.TITLE,
  a.NAME AS artist_name,
  SUM(f.ESTIMATED_VALUE) AS total_value,
  SUM(f.REVIEW_COUNT)    AS total_reviews,
  AVG(f.AVG_RATING)      AS avg_rating
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
WHERE ex.EXHIBITION_KEY = 1
GROUP BY ex.TITLE, a.NAME
ORDER BY total_value DESC;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);


CREATE INDEX idx_fact_artist_key
  ON FACT_EXHIBITION_ACTIVITY (ARTIST_KEY);


EXPLAIN PLAN FOR
SELECT
  a.NAME,
  COUNT(DISTINCT f.EXHIBITION_KEY) AS exhibition_count,
  SUM(f.ESTIMATED_VALUE)          AS total_value
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
GROUP BY a.NAME
ORDER BY total_value DESC;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);



CREATE INDEX idx_fact_exhibition_artist
  ON FACT_EXHIBITION_ACTIVITY (EXHIBITION_KEY, ARTIST_KEY);

EXPLAIN PLAN FOR
SELECT
  ex.TITLE,
  a.NAME AS artist_name,
  SUM(f.ESTIMATED_VALUE) AS total_value
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
WHERE ex.EXHIBITION_KEY IN (1, 2, 3)
GROUP BY ex.TITLE, a.NAME
ORDER BY ex.TITLE, total_value DESC;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);


CREATE INDEX idx_fact_location_key
  ON FACT_EXHIBITION_ACTIVITY (LOCATION_KEY);

EXPLAIN PLAN FOR
SELECT
  l.NAME AS location_name,
  SUM(f.ESTIMATED_VALUE) AS total_value
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_LOCATION l
  ON l.LOCATION_KEY = f.LOCATION_KEY
GROUP BY l.NAME
ORDER BY total_value DESC;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);


----- 7
CREATE DIMENSION dim_date_dim
  LEVEL day_level     IS (DIM_DATE.DATE_KEY)
  LEVEL month_level   IS (DIM_DATE.CALENDAR_YEAR, DIM_DATE.CALENDAR_MONTH)
  LEVEL quarter_level IS (DIM_DATE.CALENDAR_YEAR, DIM_DATE.QUARTER)
  LEVEL year_level    IS (DIM_DATE.CALENDAR_YEAR)
  HIERARCHY cal_hierarchy (
    day_level     CHILD OF 
    month_level   CHILD OF 
    quarter_level CHILD OF year_level
  )
  ATTRIBUTE day_level
    DETERMINES (
      DIM_DATE.CALENDAR_DATE,
      DIM_DATE.CALENDAR_DAY,
      DIM_DATE.MONTH_NAME,
      DIM_DATE.IS_WEEKEND
    )
  ATTRIBUTE month_level
    DETERMINES (DIM_DATE.MONTH_NAME);
    
    CREATE DIMENSION dim_location_dim
  LEVEL location_level IS (DIM_LOCATION.LOCATION_KEY)
  LEVEL type_level     IS (DIM_LOCATION.TYPE)
  HIERARCHY loc_hierarchy (
    location_level CHILD OF type_level
  )
  ATTRIBUTE location_level
    DETERMINES (
      DIM_LOCATION.NAME,
      DIM_LOCATION.GALLERY_ROOM,
      DIM_LOCATION.CAPACITY
    );

    

---- 8

DROP TABLE FACT_EXHIBITION_ACTIVITY PURGE;
CREATE TABLE FACT_EXHIBITION_ACTIVITY (
  FACT_KEY               NUMBER        PRIMARY KEY,
  DATE_KEY               NUMBER(8)     NOT NULL,  -- yyyymmdd
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
)
PARTITION BY RANGE (DATE_KEY) (
  PARTITION p_2023 VALUES LESS THAN (20240101),
  PARTITION p_2024 VALUES LESS THAN (20250101),
  PARTITION p_max  VALUES LESS THAN (MAXVALUE)
);


EXPLAIN PLAN FOR
SELECT
  d.CALENDAR_YEAR,
  d.CALENDAR_MONTH,
  SUM(f.ESTIMATED_VALUE) AS total_value,
  SUM(f.REVIEW_COUNT)    AS total_reviews
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR = 2024
GROUP BY d.CALENDAR_YEAR, d.CALENDAR_MONTH
ORDER BY d.CALENDAR_YEAR, d.CALENDAR_MONTH;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);




EXPLAIN PLAN FOR
SELECT
  d.CALENDAR_YEAR,
  SUM(f.ESTIMATED_VALUE) AS total_value
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2023 AND 2024
GROUP BY d.CALENDAR_YEAR
ORDER BY d.CALENDAR_YEAR;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);


-----9


-- Varianta de baz? (f?r? optimiz?ri explicite)
SELECT
  ex.TITLE                    AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                      AS artist_name,
  SUM(f.ESTIMATED_VALUE)      AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)       AS total_insured_amount,
  COUNT(DISTINCT f.ARTWORK_KEY) AS artwork_count,
  AVG(f.AVG_RATING)           AS avg_rating
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2022 AND 2024
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME
ORDER BY
  d.CALENDAR_YEAR,
  ex.TITLE,
  total_insured_amount DESC;

EXPLAIN PLAN FOR
SELECT
  ex.TITLE                    AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                      AS artist_name,
  SUM(f.ESTIMATED_VALUE)      AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)       AS total_insured_amount,
  COUNT(DISTINCT f.ARTWORK_KEY) AS artwork_count,
  AVG(f.AVG_RATING)           AS avg_rating
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2022 AND 2024
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME
ORDER BY
  d.CALENDAR_YEAR,
  ex.TITLE,
  total_insured_amount DESC;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);


EXPLAIN PLAN FOR
SELECT
  ex.TITLE                    AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                      AS artist_name,
  SUM(f.ESTIMATED_VALUE)      AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)       AS total_insured_amount,
  COUNT(DISTINCT f.ARTWORK_KEY) AS artwork_count,
  AVG(f.AVG_RATING)           AS avg_rating
FROM (
  SELECT /*+ INDEX(f idx_fact_date_key) */
         *
  FROM FACT_EXHIBITION_ACTIVITY f
  WHERE DATE_KEY BETWEEN 20220101 AND 20241231
) f
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2022 AND 2024
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME
ORDER BY
  d.CALENDAR_YEAR,
  ex.TITLE,
  total_insured_amount DESC;

SELECT * FROM TABLE(DBMS_XPLAN.DISPLAY);



-----10
-- ============================================================================
-- Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
-- Implemented in DwAnalyticsService.cs and accessible via AnalyticsController
-- ============================================================================

-- 1. Top 10 Artiști după Număr de Opere Expuse
-- Cerință în limbaj natural (română):
-- "Afișați primii 10 artiști în funcție de numărul de opere din colecție, 
--  împreună cu valoarea totală și medie estimată a operelor lor."
--
-- Natural Language Request (English):
-- "Show me the top 10 artists with the most artworks in the collection,
--  along with their total and average estimated artwork value."

SELECT * FROM (
    SELECT
        da.NAME as artist_name,
        COUNT(DISTINCT daw.ARTWORK_KEY) as artwork_count,
        SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_value,
        AVG(NVL(f.ESTIMATED_VALUE, 0)) as average_value
    FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
    JOIN ART_GALLERY_DW.DIM_ARTIST da ON f.ARTIST_KEY = da.ARTIST_KEY
    JOIN ART_GALLERY_DW.DIM_ARTWORK daw ON f.ARTWORK_KEY = daw.ARTWORK_KEY
    GROUP BY da.NAME
    ORDER BY artwork_count DESC
)
WHERE ROWNUM <= 10;


-- 2. Valoarea Colecției pe Tip de Mediu și Colecție
-- Cerință în limbaj natural (română):
-- "Prezentați o analiză a valorii totale a colecției descompusă pe tipuri de mediu 
--  (pictură, sculptură, etc.) și pe colecții, incluzând numărul de opere și 
--  valoarea medie pe fiecare categorie."
--
-- Natural Language Request (English):
-- "Provide a breakdown of the collection's total value by art medium and collection type,
--  including the number of artworks and average value for each category."

SELECT
    NVL(daw.MEDIUM, 'Unknown') as medium_type,
    NVL(dc.NAME, 'Unknown') as collection_name,
    COUNT(DISTINCT f.ARTWORK_KEY) as artwork_count,
    SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_value,
    AVG(NVL(f.ESTIMATED_VALUE, 0)) as average_value
FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
JOIN ART_GALLERY_DW.DIM_ARTWORK daw ON f.ARTWORK_KEY = daw.ARTWORK_KEY
LEFT JOIN ART_GALLERY_DW.DIM_COLLECTION dc ON f.COLLECTION_KEY = dc.COLLECTION_KEY
GROUP BY daw.MEDIUM, dc.NAME
ORDER BY total_value DESC;


-- 3. Activitatea Lunară a Expozițiilor (Ultimele 12 Luni)
-- Cerință în limbaj natural (română):
-- "Analizați performanța expozițiilor lunar pentru ultimul an: afișați pentru fiecare lună 
--  numărul de expoziții active, numărul de opere expuse, valoarea totală a operelor 
--  și evaluarea medie primită de vizitatori."
--
-- Natural Language Request (English):
-- "Analyze monthly exhibition performance for the past year: show the number of active
--  exhibitions, artworks exhibited, total artwork value, and average visitor rating
--  for each month."

SELECT
    dd.MONTH_NAME as month_name,
    dd.CALENDAR_YEAR as year,
    COUNT(DISTINCT f.EXHIBITION_KEY) as exhibition_count,
    COUNT(DISTINCT f.ARTWORK_KEY) as artworks_exhibited,
    SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_artwork_value,
    AVG(f.AVG_RATING) as average_rating
FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
JOIN ART_GALLERY_DW.DIM_DATE dd ON f.DATE_KEY = dd.DATE_KEY
WHERE dd.CALENDAR_DATE >= ADD_MONTHS(SYSDATE, -12)
GROUP BY dd.CALENDAR_YEAR, dd.MONTH_NAME, dd.CALENDAR_MONTH
ORDER BY dd.CALENDAR_YEAR, dd.CALENDAR_MONTH;


-- 4. Distribuția Operelor pe Locații și Galerii
-- Cerință în limbaj natural (română):
-- "Prezentați distribuția operelor de artă în diferitele locații ale galeriei: 
--  pentru fiecare locație și sală afișați numărul de opere, valoarea totală 
--  și procentul pe care îl reprezintă din întreaga colecție expusă."
--
-- Natural Language Request (English):
-- "Show the distribution of artworks across different gallery locations:
--  for each location and room, display the number of artworks, total value,
--  and percentage they represent of the entire exhibited collection."

SELECT
    NVL(dl.NAME, 'Unknown') as location_name,
    NVL(dl.GALLERY_ROOM, 'N/A') as gallery_room,
    NVL(dl.TYPE, 'Unknown') as location_type,
    COUNT(DISTINCT f.ARTWORK_KEY) as artworks_count,
    SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_value,
    ROUND(COUNT(DISTINCT f.ARTWORK_KEY) * 100.0 / 
        NULLIF(SUM(COUNT(DISTINCT f.ARTWORK_KEY)) OVER(), 0), 2) as percentage
FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
JOIN ART_GALLERY_DW.DIM_LOCATION dl ON f.LOCATION_KEY = dl.LOCATION_KEY
GROUP BY dl.NAME, dl.GALLERY_ROOM, dl.TYPE
ORDER BY artworks_count DESC;


-- 5. Trendul Anual al Activității Expoziționale (Ultimii 5 Ani)
-- Cerință în limbaj natural (română):
-- "Afișați evoluția activității expoziționale pe ultimii 5 ani: pentru fiecare an 
--  prezentați numărul de expoziții organizate, numărul de opere expuse, valoarea 
--  totală și medie a operelor, precum și rata de creștere an-la-an (YoY) a valorii."
--
-- Natural Language Request (English):
-- "Show the trend of exhibition activity over the past 5 years: for each year
--  display the number of exhibitions held, artworks exhibited, total and average
--  artwork value, and year-over-year growth rate."

SELECT
    dd.CALENDAR_YEAR as year,
    COUNT(DISTINCT f.EXHIBITION_KEY) as exhibitions_count,
    COUNT(DISTINCT f.ARTWORK_KEY) as artworks_count,
    SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_artwork_value,
    AVG(NVL(f.ESTIMATED_VALUE, 0)) as average_artwork_value,
    LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) as previous_year_value,
    CASE 
        WHEN LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) IS NOT NULL 
             AND LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) > 0
        THEN ROUND(
            ((SUM(NVL(f.ESTIMATED_VALUE, 0)) - 
              LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR)) * 100.0 / 
              LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR)), 2)
        ELSE NULL
    END as yoy_growth_rate
FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
JOIN ART_GALLERY_DW.DIM_DATE dd ON f.DATE_KEY = dd.DATE_KEY
WHERE dd.CALENDAR_YEAR >= EXTRACT(YEAR FROM SYSDATE) - 5
GROUP BY dd.CALENDAR_YEAR
ORDER BY dd.CALENDAR_YEAR;





