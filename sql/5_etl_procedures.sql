/*
================================================================================
  ART GALLERY DW/BI - COMPREHENSIVE ETL PACKAGE
  
  PROBLEM DIAGNOSIS:
  ------------------
  1. Original ETL uses INSERT-only logic with ROW_NUMBER() for FACT_KEY
     - This fails for incremental loads (duplicates PK or skips new rows)
  2. Dimension lookups use direct OLTP IDs as surrogate keys 
     - Works only if OLTP IDs are never reassigned
  3. No sequence for FACT_KEY generation
  4. No MERGE logic for idempotent ETL runs
  5. Missing dimension refresh for new OLTP records (no surrogate resolution)
  6. No error handling or logging
  
  SOLUTION:
  ---------
  1. Create sequences for all surrogate keys
  2. Use MERGE for dimension tables (SCD Type 1)
  3. Use MERGE for fact table with proper business key matching
  4. Add ETL logging table for audit trail
  5. Wrap in stored procedures callable from .NET backend
  6. Support both full and incremental ETL modes
  
  Author: Oracle PL/SQL ETL Specialist
  Version: 1.0.0 - Production Grade for Master's Thesis
================================================================================
*/

-- ============================================================================
-- SECTION 1: ETL INFRASTRUCTURE (Sequences, Logging Tables, Grants)
-- ============================================================================

-- Sequences for surrogate key generation
CREATE SEQUENCE SEQ_DIM_ARTIST START WITH 1000 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_DIM_COLLECTION START WITH 1000 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_DIM_LOCATION START WITH 1000 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_DIM_EXHIBITOR START WITH 1000 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_DIM_EXHIBITION START WITH 1000 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_DIM_ARTWORK START WITH 1000 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_DIM_POLICY START WITH 1000 INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_FACT_EXHIBITION START WITH 1000 INCREMENT BY 1 NOCACHE;
/

-- ETL Log Table for audit and debugging
CREATE TABLE ETL_LOG (
    LOG_ID           NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ETL_RUN_ID       NUMBER           NOT NULL,
    PROCEDURE_NAME   VARCHAR2(128)    NOT NULL,
    STEP_NAME        VARCHAR2(256),
    STATUS           VARCHAR2(32)     NOT NULL, -- STARTED, COMPLETED, ERROR, WARNING
    RECORDS_AFFECTED NUMBER           DEFAULT 0,
    ERROR_MESSAGE    VARCHAR2(4000),
    START_TIME       TIMESTAMP        NOT NULL,
    END_TIME         TIMESTAMP,
    CREATED_AT       TIMESTAMP        DEFAULT SYSTIMESTAMP
);

CREATE INDEX IDX_ETL_LOG_RUN_ID ON ETL_LOG(ETL_RUN_ID);
CREATE INDEX IDX_ETL_LOG_STATUS ON ETL_LOG(STATUS);
CREATE INDEX IDX_ETL_LOG_PROC ON ETL_LOG(PROCEDURE_NAME);
/

-- ETL Run sequence for tracking runs
CREATE SEQUENCE SEQ_ETL_RUN START WITH 1 INCREMENT BY 1 NOCACHE;
/

-- ============================================================================
-- SECTION 2: UTILITY PROCEDURES
-- ============================================================================

CREATE OR REPLACE PROCEDURE ETL_LOG_ENTRY (
    p_run_id         IN NUMBER,
    p_procedure_name IN VARCHAR2,
    p_step_name      IN VARCHAR2,
    p_status         IN VARCHAR2,
    p_records        IN NUMBER DEFAULT 0,
    p_error_message  IN VARCHAR2 DEFAULT NULL,
    p_start_time     IN TIMESTAMP DEFAULT NULL,
    p_end_time       IN TIMESTAMP DEFAULT NULL
)
AUTHID CURRENT_USER
AS
    PRAGMA AUTONOMOUS_TRANSACTION;
BEGIN
    INSERT INTO ETL_LOG (
        ETL_RUN_ID, PROCEDURE_NAME, STEP_NAME, STATUS, 
        RECORDS_AFFECTED, ERROR_MESSAGE, START_TIME, END_TIME
    ) VALUES (
        p_run_id, p_procedure_name, p_step_name, p_status,
        p_records, p_error_message,
        NVL(p_start_time, SYSTIMESTAMP),
        p_end_time
    );
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        -- Silently fail logging to not disrupt ETL
        ROLLBACK;
END ETL_LOG_ENTRY;
/

-- ============================================================================
-- SECTION 3: DIMENSION ETL PROCEDURES (MERGE for SCD Type 1)
-- ============================================================================

-- ----------------------------------------
-- ETL_SYNC_DIM_ARTIST
-- ----------------------------------------
CREATE OR REPLACE PROCEDURE ETL_SYNC_DIM_ARTIST (
    p_run_id IN NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time  TIMESTAMP := SYSTIMESTAMP;
    v_rows_merged NUMBER := 0;
BEGIN
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_ARTIST', 'Started', 'STARTED', 0, NULL, v_start_time);
    
    -- MERGE: Insert new artists, update existing ones (SCD Type 1)
    MERGE INTO DIM_ARTIST tgt
    USING (
        SELECT 
            artist_id,
            name,
            nationality,
            birth_year,
            death_year
        FROM art_gallery_oltp.Artist
    ) src
    ON (tgt.ARTIST_ID_OLTP = src.artist_id)
    WHEN MATCHED THEN
        UPDATE SET
            tgt.NAME        = src.name,
            tgt.NATIONALITY = src.nationality,
            tgt.BIRTH_YEAR  = src.birth_year,
            tgt.DEATH_YEAR  = src.death_year
        WHERE tgt.NAME        != src.name
           OR NVL(tgt.NATIONALITY, '~') != NVL(src.nationality, '~')
           OR NVL(tgt.BIRTH_YEAR, -1)   != NVL(src.birth_year, -1)
           OR NVL(tgt.DEATH_YEAR, -1)   != NVL(src.death_year, -1)
    WHEN NOT MATCHED THEN
        INSERT (ARTIST_KEY, ARTIST_ID_OLTP, NAME, NATIONALITY, BIRTH_YEAR, DEATH_YEAR)
        VALUES (SEQ_DIM_ARTIST.NEXTVAL, src.artist_id, src.name, src.nationality, 
                src.birth_year, src.death_year);
    
    v_rows_merged := SQL%ROWCOUNT;
    COMMIT;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_ARTIST', 'Completed', 'COMPLETED', 
                  v_rows_merged, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_ARTIST', 'Error', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_DIM_ARTIST;
/

-- ----------------------------------------
-- ETL_SYNC_DIM_COLLECTION
-- ----------------------------------------
CREATE OR REPLACE PROCEDURE ETL_SYNC_DIM_COLLECTION (
    p_run_id IN NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time  TIMESTAMP := SYSTIMESTAMP;
    v_rows_merged NUMBER := 0;
BEGIN
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_COLLECTION', 'Started', 'STARTED', 0, NULL, v_start_time);
    
    MERGE INTO DIM_COLLECTION tgt
    USING (
        SELECT 
            collection_id,
            name,
            description,
            CASE 
                WHEN created_date IS NOT NULL 
                THEN TO_NUMBER(TO_CHAR(created_date, 'YYYYMMDD'))
                ELSE NULL 
            END AS created_date_key
        FROM art_gallery_oltp.Collection
    ) src
    ON (tgt.COLLECTION_ID_OLTP = src.collection_id)
    WHEN MATCHED THEN
        UPDATE SET
            tgt.NAME             = src.name,
            tgt.DESCRIPTION      = src.description,
            tgt.CREATED_DATE_KEY = src.created_date_key
        WHERE tgt.NAME != src.name
           OR NVL(tgt.DESCRIPTION, '~') != NVL(src.description, '~')
           OR NVL(tgt.CREATED_DATE_KEY, -1) != NVL(src.created_date_key, -1)
    WHEN NOT MATCHED THEN
        INSERT (COLLECTION_KEY, COLLECTION_ID_OLTP, NAME, DESCRIPTION, CREATED_DATE_KEY)
        VALUES (SEQ_DIM_COLLECTION.NEXTVAL, src.collection_id, src.name, 
                src.description, src.created_date_key);
    
    v_rows_merged := SQL%ROWCOUNT;
    COMMIT;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_COLLECTION', 'Completed', 'COMPLETED', 
                  v_rows_merged, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_COLLECTION', 'Error', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_DIM_COLLECTION;
/

-- ----------------------------------------
-- ETL_SYNC_DIM_LOCATION
-- ----------------------------------------
CREATE OR REPLACE PROCEDURE ETL_SYNC_DIM_LOCATION (
    p_run_id IN NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time  TIMESTAMP := SYSTIMESTAMP;
    v_rows_merged NUMBER := 0;
BEGIN
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_LOCATION', 'Started', 'STARTED', 0, NULL, v_start_time);
    
    MERGE INTO DIM_LOCATION tgt
    USING (
        SELECT 
            location_id,
            name,
            gallery_room,
            type,
            capacity
        FROM art_gallery_oltp.Location
    ) src
    ON (tgt.LOCATION_ID_OLTP = src.location_id)
    WHEN MATCHED THEN
        UPDATE SET
            tgt.NAME         = src.name,
            tgt.GALLERY_ROOM = src.gallery_room,
            tgt.TYPE         = src.type,
            tgt.CAPACITY     = src.capacity
        WHERE tgt.NAME != src.name
           OR NVL(tgt.GALLERY_ROOM, '~') != NVL(src.gallery_room, '~')
           OR NVL(tgt.TYPE, '~') != NVL(src.type, '~')
           OR NVL(tgt.CAPACITY, -1) != NVL(src.capacity, -1)
    WHEN NOT MATCHED THEN
        INSERT (LOCATION_KEY, LOCATION_ID_OLTP, NAME, GALLERY_ROOM, TYPE, CAPACITY)
        VALUES (SEQ_DIM_LOCATION.NEXTVAL, src.location_id, src.name, 
                src.gallery_room, src.type, src.capacity);
    
    v_rows_merged := SQL%ROWCOUNT;
    COMMIT;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_LOCATION', 'Completed', 'COMPLETED', 
                  v_rows_merged, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_LOCATION', 'Error', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_DIM_LOCATION;
/

-- ----------------------------------------
-- ETL_SYNC_DIM_EXHIBITOR
-- ----------------------------------------
CREATE OR REPLACE PROCEDURE ETL_SYNC_DIM_EXHIBITOR (
    p_run_id IN NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time  TIMESTAMP := SYSTIMESTAMP;
    v_rows_merged NUMBER := 0;
BEGIN
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_EXHIBITOR', 'Started', 'STARTED', 0, NULL, v_start_time);
    
    MERGE INTO DIM_EXHIBITOR tgt
    USING (
        SELECT 
            exhibitor_id,
            name,
            address,
            city,
            contact_info
        FROM art_gallery_oltp.Exhibitor
    ) src
    ON (tgt.EXHIBITOR_ID_OLTP = src.exhibitor_id)
    WHEN MATCHED THEN
        UPDATE SET
            tgt.NAME         = src.name,
            tgt.ADDRESS      = src.address,
            tgt.CITY         = src.city,
            tgt.CONTACT_INFO = src.contact_info
        WHERE tgt.NAME != src.name
           OR NVL(tgt.ADDRESS, '~') != NVL(src.address, '~')
           OR NVL(tgt.CITY, '~') != NVL(src.city, '~')
           OR NVL(tgt.CONTACT_INFO, '~') != NVL(src.contact_info, '~')
    WHEN NOT MATCHED THEN
        INSERT (EXHIBITOR_KEY, EXHIBITOR_ID_OLTP, NAME, ADDRESS, CITY, CONTACT_INFO)
        VALUES (SEQ_DIM_EXHIBITOR.NEXTVAL, src.exhibitor_id, src.name, 
                src.address, src.city, src.contact_info);
    
    v_rows_merged := SQL%ROWCOUNT;
    COMMIT;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_EXHIBITOR', 'Completed', 'COMPLETED', 
                  v_rows_merged, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_EXHIBITOR', 'Error', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_DIM_EXHIBITOR;
/

-- ----------------------------------------
-- ETL_SYNC_DIM_EXHIBITION
-- ----------------------------------------
CREATE OR REPLACE PROCEDURE ETL_SYNC_DIM_EXHIBITION (
    p_run_id IN NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time  TIMESTAMP := SYSTIMESTAMP;
    v_rows_merged NUMBER := 0;
BEGIN
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_EXHIBITION', 'Started', 'STARTED', 0, NULL, v_start_time);
    
    -- First ensure exhibitors are synced
    MERGE INTO DIM_EXHIBITION tgt
    USING (
        SELECT 
            ex.exhibition_id,
            ex.title,
            TO_NUMBER(TO_CHAR(ex.start_date, 'YYYYMMDD')) AS start_date_key,
            TO_NUMBER(TO_CHAR(ex.end_date, 'YYYYMMDD'))   AS end_date_key,
            de.EXHIBITOR_KEY,  -- Lookup surrogate key
            ex.description
        FROM art_gallery_oltp.Exhibition ex
        JOIN DIM_EXHIBITOR de ON de.EXHIBITOR_ID_OLTP = ex.exhibitor_id
    ) src
    ON (tgt.EXHIBITION_ID_OLTP = src.exhibition_id)
    WHEN MATCHED THEN
        UPDATE SET
            tgt.TITLE          = src.title,
            tgt.START_DATE_KEY = src.start_date_key,
            tgt.END_DATE_KEY   = src.end_date_key,
            tgt.EXHIBITOR_KEY  = src.EXHIBITOR_KEY,
            tgt.DESCRIPTION    = src.description
        WHERE tgt.TITLE != src.title
           OR NVL(tgt.START_DATE_KEY, -1) != NVL(src.start_date_key, -1)
           OR NVL(tgt.END_DATE_KEY, -1) != NVL(src.end_date_key, -1)
           OR NVL(tgt.EXHIBITOR_KEY, -1) != NVL(src.EXHIBITOR_KEY, -1)
           OR NVL(tgt.DESCRIPTION, '~') != NVL(src.description, '~')
    WHEN NOT MATCHED THEN
        INSERT (EXHIBITION_KEY, EXHIBITION_ID_OLTP, TITLE, START_DATE_KEY, 
                END_DATE_KEY, EXHIBITOR_KEY, DESCRIPTION)
        VALUES (SEQ_DIM_EXHIBITION.NEXTVAL, src.exhibition_id, src.title,
                src.start_date_key, src.end_date_key, src.EXHIBITOR_KEY, src.description);
    
    v_rows_merged := SQL%ROWCOUNT;
    COMMIT;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_EXHIBITION', 'Completed', 'COMPLETED', 
                  v_rows_merged, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_EXHIBITION', 'Error', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_DIM_EXHIBITION;
/

-- ----------------------------------------
-- ETL_SYNC_DIM_ARTWORK
-- ----------------------------------------
CREATE OR REPLACE PROCEDURE ETL_SYNC_DIM_ARTWORK (
    p_run_id IN NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time  TIMESTAMP := SYSTIMESTAMP;
    v_rows_merged NUMBER := 0;
BEGIN
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_ARTWORK', 'Started', 'STARTED', 0, NULL, v_start_time);
    
    -- Artwork requires lookups for ARTIST_KEY, COLLECTION_KEY, LOCATION_KEY
    MERGE INTO DIM_ARTWORK tgt
    USING (
        SELECT 
            aw.artwork_id,
            aw.title,
            da.ARTIST_KEY,     -- Surrogate from DIM_ARTIST
            aw.year_created,
            aw.medium,
            dc.COLLECTION_KEY, -- Surrogate from DIM_COLLECTION (nullable)
            dl.LOCATION_KEY,   -- Surrogate from DIM_LOCATION (nullable)
            aw.estimated_value
        FROM art_gallery_oltp.Artwork aw
        JOIN DIM_ARTIST da ON da.ARTIST_ID_OLTP = aw.artist_id
        LEFT JOIN DIM_COLLECTION dc ON dc.COLLECTION_ID_OLTP = aw.collection_id
        LEFT JOIN DIM_LOCATION dl ON dl.LOCATION_ID_OLTP = aw.location_id
    ) src
    ON (tgt.ARTWORK_ID_OLTP = src.artwork_id)
    WHEN MATCHED THEN
        UPDATE SET
            tgt.TITLE           = src.title,
            tgt.ARTIST_KEY      = src.ARTIST_KEY,
            tgt.YEAR_CREATED    = src.year_created,
            tgt.MEDIUM          = src.medium,
            tgt.COLLECTION_KEY  = src.COLLECTION_KEY,
            tgt.LOCATION_KEY    = src.LOCATION_KEY,
            tgt.ESTIMATED_VALUE = src.estimated_value
        WHERE tgt.TITLE != src.title
           OR tgt.ARTIST_KEY != src.ARTIST_KEY
           OR NVL(tgt.YEAR_CREATED, -1) != NVL(src.year_created, -1)
           OR NVL(tgt.MEDIUM, '~') != NVL(src.medium, '~')
           OR NVL(tgt.COLLECTION_KEY, -1) != NVL(src.COLLECTION_KEY, -1)
           OR NVL(tgt.LOCATION_KEY, -1) != NVL(src.LOCATION_KEY, -1)
           OR NVL(tgt.ESTIMATED_VALUE, -1) != NVL(src.estimated_value, -1)
    WHEN NOT MATCHED THEN
        INSERT (ARTWORK_KEY, ARTWORK_ID_OLTP, TITLE, ARTIST_KEY, YEAR_CREATED,
                MEDIUM, COLLECTION_KEY, LOCATION_KEY, ESTIMATED_VALUE)
        VALUES (SEQ_DIM_ARTWORK.NEXTVAL, src.artwork_id, src.title, src.ARTIST_KEY,
                src.year_created, src.medium, src.COLLECTION_KEY, 
                src.LOCATION_KEY, src.estimated_value);
    
    v_rows_merged := SQL%ROWCOUNT;
    COMMIT;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_ARTWORK', 'Completed', 'COMPLETED', 
                  v_rows_merged, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_ARTWORK', 'Error', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_DIM_ARTWORK;
/

-- ----------------------------------------
-- ETL_SYNC_DIM_POLICY
-- ----------------------------------------
CREATE OR REPLACE PROCEDURE ETL_SYNC_DIM_POLICY (
    p_run_id IN NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time  TIMESTAMP := SYSTIMESTAMP;
    v_rows_merged NUMBER := 0;
BEGIN
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_POLICY', 'Started', 'STARTED', 0, NULL, v_start_time);
    
    MERGE INTO DIM_POLICY tgt
    USING (
        SELECT 
            policy_id,
            provider,
            TO_NUMBER(TO_CHAR(start_date, 'YYYYMMDD')) AS start_date_key,
            TO_NUMBER(TO_CHAR(end_date, 'YYYYMMDD'))   AS end_date_key,
            total_coverage_amount
        FROM art_gallery_oltp.Insurance_Policy
    ) src
    ON (tgt.POLICY_ID_OLTP = src.policy_id)
    WHEN MATCHED THEN
        UPDATE SET
            tgt.PROVIDER           = src.provider,
            tgt.START_DATE_KEY     = src.start_date_key,
            tgt.END_DATE_KEY       = src.end_date_key,
            tgt.TOTAL_COVERAGE_AMT = src.total_coverage_amount
        WHERE tgt.PROVIDER != src.provider
           OR NVL(tgt.START_DATE_KEY, -1) != NVL(src.start_date_key, -1)
           OR NVL(tgt.END_DATE_KEY, -1) != NVL(src.end_date_key, -1)
           OR NVL(tgt.TOTAL_COVERAGE_AMT, -1) != NVL(src.total_coverage_amount, -1)
    WHEN NOT MATCHED THEN
        INSERT (POLICY_KEY, POLICY_ID_OLTP, PROVIDER, START_DATE_KEY, 
                END_DATE_KEY, TOTAL_COVERAGE_AMT)
        VALUES (SEQ_DIM_POLICY.NEXTVAL, src.policy_id, src.provider,
                src.start_date_key, src.end_date_key, src.total_coverage_amount);
    
    v_rows_merged := SQL%ROWCOUNT;
    COMMIT;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_POLICY', 'Completed', 'COMPLETED', 
                  v_rows_merged, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_DIM_POLICY', 'Error', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_DIM_POLICY;
/

-- ============================================================================
-- SECTION 4: FACT TABLE ETL PROCEDURE (Core Solution)
-- ============================================================================

/*
  ETL_SYNC_FACT_EXHIBITION_ACTIVITY
  
  This is the CORE procedure that resolves the data propagation issue.
  
  BUSINESS KEY: (artwork_id + exhibition_id) from ARTWORK_EXHIBITION table
  
  For each combination of artwork+exhibition in OLTP:
  1. Lookup all dimension surrogate keys
  2. Calculate measures (aggregates from related OLTP tables)
  3. MERGE into fact table (INSERT if new, UPDATE if measures changed)
*/
CREATE OR REPLACE PROCEDURE ETL_SYNC_FACT_EXHIBITION_ACTIVITY (
    p_run_id IN NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time    TIMESTAMP := SYSTIMESTAMP;
    v_rows_merged   NUMBER := 0;
    v_rows_inserted NUMBER := 0;
    v_rows_updated  NUMBER := 0;
BEGIN
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_FACT_EXHIBITION_ACTIVITY', 'Started', 'STARTED', 
                  0, NULL, v_start_time);
    
    /*
      MERGE INTO FACT_EXHIBITION_ACTIVITY
      
      Business Key: ARTWORK_KEY + EXHIBITION_KEY (resolved from OLTP IDs)
      
      The source query:
      - Joins ARTWORK_EXHIBITION (driving table) with all dimension lookups
      - Calculates measures from OLTP aggregate tables
    */
    MERGE INTO FACT_EXHIBITION_ACTIVITY tgt
    USING (
        SELECT 
            -- Surrogate Keys from Dimension Tables
            TO_NUMBER(TO_CHAR(ex.start_date, 'YYYYMMDD'))  AS DATE_KEY,
            dex.EXHIBITION_KEY,
            deb.EXHIBITOR_KEY,
            daw.ARTWORK_KEY,
            dar.ARTIST_KEY,
            daw.COLLECTION_KEY,
            daw.LOCATION_KEY,
            dpol.POLICY_KEY,
            
            -- Measures
            aw.estimated_value                              AS ESTIMATED_VALUE,
            NVL(ins_agg.total_insured, 0)                   AS INSURED_AMOUNT,
            CASE WHEN ln_agg.loan_count > 0 THEN 1 ELSE 0 END AS LOAN_FLAG,
            NVL(res_agg.restoration_count, 0)               AS RESTORATION_COUNT,
            NVL(rv_agg.review_count, 0)                     AS REVIEW_COUNT,
            NVL(rv_agg.avg_rating, 0)                       AS AVG_RATING,
            
            -- OLTP IDs for debugging/audit
            ax.artwork_id    AS OLTP_ARTWORK_ID,
            ax.exhibition_id AS OLTP_EXHIBITION_ID
            
        FROM art_gallery_oltp.Artwork_Exhibition ax
        
        -- Join OLTP tables
        JOIN art_gallery_oltp.Artwork aw 
            ON aw.artwork_id = ax.artwork_id
        JOIN art_gallery_oltp.Exhibition ex 
            ON ex.exhibition_id = ax.exhibition_id
        
        -- Lookup Dimension Surrogate Keys (CRITICAL: These must exist!)
        JOIN DIM_ARTWORK daw 
            ON daw.ARTWORK_ID_OLTP = aw.artwork_id
        JOIN DIM_ARTIST dar 
            ON dar.ARTIST_ID_OLTP = aw.artist_id
        JOIN DIM_EXHIBITION dex 
            ON dex.EXHIBITION_ID_OLTP = ex.exhibition_id
        JOIN DIM_EXHIBITOR deb 
            ON deb.EXHIBITOR_ID_OLTP = ex.exhibitor_id
        
        -- Optional dimension lookups
        LEFT JOIN DIM_COLLECTION dcol 
            ON dcol.COLLECTION_ID_OLTP = aw.collection_id
        LEFT JOIN DIM_LOCATION dloc 
            ON dloc.LOCATION_ID_OLTP = aw.location_id
            
        -- Insurance aggregation (artwork-level)
        LEFT JOIN (
            SELECT 
                i.artwork_id,
                SUM(i.insured_amount) AS total_insured,
                MIN(i.policy_id)      AS primary_policy_id
            FROM art_gallery_oltp.Insurance i
            GROUP BY i.artwork_id
        ) ins_agg ON ins_agg.artwork_id = aw.artwork_id
        
        -- Policy lookup (from insurance)
        LEFT JOIN DIM_POLICY dpol 
            ON dpol.POLICY_ID_OLTP = ins_agg.primary_policy_id
        
        -- Loan count aggregation
        LEFT JOIN (
            SELECT 
                l.artwork_id,
                COUNT(*) AS loan_count
            FROM art_gallery_oltp.Loan l
            GROUP BY l.artwork_id
        ) ln_agg ON ln_agg.artwork_id = aw.artwork_id
        
        -- Restoration count aggregation
        LEFT JOIN (
            SELECT 
                r.artwork_id,
                COUNT(*) AS restoration_count
            FROM art_gallery_oltp.Restoration r
            GROUP BY r.artwork_id
        ) res_agg ON res_agg.artwork_id = aw.artwork_id
        
        -- Review aggregation (exhibition+artwork specific)
        LEFT JOIN (
            SELECT 
                gr.exhibition_id,
                gr.artwork_id,
                COUNT(*)       AS review_count,
                AVG(gr.rating) AS avg_rating
            FROM art_gallery_oltp.Gallery_Review gr
            GROUP BY gr.exhibition_id, gr.artwork_id
        ) rv_agg ON rv_agg.exhibition_id = ex.exhibition_id
                AND (rv_agg.artwork_id = aw.artwork_id OR rv_agg.artwork_id IS NULL)
                
    ) src
    -- Match on business key: ARTWORK_KEY + EXHIBITION_KEY (surrogate)
    ON (tgt.ARTWORK_KEY = src.ARTWORK_KEY AND tgt.EXHIBITION_KEY = src.EXHIBITION_KEY)
    
    WHEN MATCHED THEN
        UPDATE SET
            tgt.DATE_KEY          = src.DATE_KEY,
            tgt.EXHIBITOR_KEY     = src.EXHIBITOR_KEY,
            tgt.ARTIST_KEY        = src.ARTIST_KEY,
            tgt.COLLECTION_KEY    = src.COLLECTION_KEY,
            tgt.LOCATION_KEY      = src.LOCATION_KEY,
            tgt.POLICY_KEY        = src.POLICY_KEY,
            tgt.ESTIMATED_VALUE   = src.ESTIMATED_VALUE,
            tgt.INSURED_AMOUNT    = src.INSURED_AMOUNT,
            tgt.LOAN_FLAG         = src.LOAN_FLAG,
            tgt.RESTORATION_COUNT = src.RESTORATION_COUNT,
            tgt.REVIEW_COUNT      = src.REVIEW_COUNT,
            tgt.AVG_RATING        = src.AVG_RATING
        -- Only update if measures have changed (avoid unnecessary writes)
        WHERE NVL(tgt.ESTIMATED_VALUE, -1)   != NVL(src.ESTIMATED_VALUE, -1)
           OR NVL(tgt.INSURED_AMOUNT, -1)    != NVL(src.INSURED_AMOUNT, -1)
           OR NVL(tgt.LOAN_FLAG, -1)         != NVL(src.LOAN_FLAG, -1)
           OR NVL(tgt.RESTORATION_COUNT, -1) != NVL(src.RESTORATION_COUNT, -1)
           OR NVL(tgt.REVIEW_COUNT, -1)      != NVL(src.REVIEW_COUNT, -1)
           OR NVL(tgt.AVG_RATING, -1)        != NVL(src.AVG_RATING, -1)
           OR NVL(tgt.POLICY_KEY, -1)        != NVL(src.POLICY_KEY, -1)
           OR NVL(tgt.COLLECTION_KEY, -1)    != NVL(src.COLLECTION_KEY, -1)
           OR NVL(tgt.LOCATION_KEY, -1)      != NVL(src.LOCATION_KEY, -1)
    
    WHEN NOT MATCHED THEN
        INSERT (
            FACT_KEY, DATE_KEY, EXHIBITION_KEY, EXHIBITOR_KEY, ARTWORK_KEY,
            ARTIST_KEY, COLLECTION_KEY, LOCATION_KEY, POLICY_KEY,
            ESTIMATED_VALUE, INSURED_AMOUNT, LOAN_FLAG,
            RESTORATION_COUNT, REVIEW_COUNT, AVG_RATING
        )
        VALUES (
            SEQ_FACT_EXHIBITION.NEXTVAL,
            src.DATE_KEY, src.EXHIBITION_KEY, src.EXHIBITOR_KEY, src.ARTWORK_KEY,
            src.ARTIST_KEY, src.COLLECTION_KEY, src.LOCATION_KEY, src.POLICY_KEY,
            src.ESTIMATED_VALUE, src.INSURED_AMOUNT, src.LOAN_FLAG,
            src.RESTORATION_COUNT, src.REVIEW_COUNT, src.AVG_RATING
        );
    
    v_rows_merged := SQL%ROWCOUNT;
    COMMIT;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_FACT_EXHIBITION_ACTIVITY', 'Completed', 'COMPLETED', 
                  v_rows_merged, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_FACT_EXHIBITION_ACTIVITY', 'Error', 'ERROR', 
                      0, SQLERRM || ' - ' || DBMS_UTILITY.FORMAT_ERROR_BACKTRACE, 
                      v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_FACT_EXHIBITION_ACTIVITY;
/

-- ============================================================================
-- SECTION 5: MASTER ETL ORCHESTRATION PROCEDURE
-- ============================================================================

/*
  ETL_SYNC_ALL - Main entry point for full ETL sync
  
  This procedure orchestrates the complete ETL pipeline:
  1. Sync all dimension tables (in dependency order)
  2. Sync fact table
  3. Log overall status
  
  Call this from .NET backend or scheduler.
*/
CREATE OR REPLACE PROCEDURE ETL_SYNC_ALL (
    p_run_id       OUT NUMBER,
    p_status       OUT VARCHAR2,
    p_message      OUT VARCHAR2,
    p_records_sync OUT NUMBER
)
AUTHID CURRENT_USER
AS
    v_start_time TIMESTAMP := SYSTIMESTAMP;
    v_total_records NUMBER := 0;
BEGIN
    -- Generate new run ID
    p_run_id := SEQ_ETL_RUN.NEXTVAL;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_ALL', 'Starting Full ETL Sync', 'STARTED', 
                  0, NULL, v_start_time);
    
    -- Step 1: Sync independent dimensions first
    ETL_SYNC_DIM_ARTIST(p_run_id);
    ETL_SYNC_DIM_COLLECTION(p_run_id);
    ETL_SYNC_DIM_LOCATION(p_run_id);
    ETL_SYNC_DIM_EXHIBITOR(p_run_id);
    ETL_SYNC_DIM_POLICY(p_run_id);
    
    -- Step 2: Sync dependent dimensions
    ETL_SYNC_DIM_EXHIBITION(p_run_id);  -- Depends on DIM_EXHIBITOR
    ETL_SYNC_DIM_ARTWORK(p_run_id);     -- Depends on DIM_ARTIST, DIM_COLLECTION, DIM_LOCATION
    
    -- Step 3: Sync fact table (depends on ALL dimensions)
    ETL_SYNC_FACT_EXHIBITION_ACTIVITY(p_run_id);
    
    -- Calculate total records processed
    SELECT NVL(SUM(RECORDS_AFFECTED), 0) INTO v_total_records
    FROM ETL_LOG
    WHERE ETL_RUN_ID = p_run_id AND STATUS = 'COMPLETED';
    
    p_status := 'SUCCESS';
    p_message := 'ETL completed successfully';
    p_records_sync := v_total_records;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_ALL', 'Full ETL Sync Completed', 'COMPLETED', 
                  v_total_records, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        p_status := 'ERROR';
        p_message := SQLERRM;
        p_records_sync := 0;
        
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_ALL', 'Full ETL Sync Failed', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_ALL;
/

/*
  ETL_SYNC_INCREMENTAL - For delta/incremental loads
  
  Syncs only records changed since last successful run.
  Uses a watermark approach based on OLTP audit columns.
  
  Note: Requires OLTP tables to have UPDATED_AT/CREATED_AT columns
        for optimal incremental detection.
*/
CREATE OR REPLACE PROCEDURE ETL_SYNC_INCREMENTAL (
    p_since_date   IN  DATE DEFAULT SYSDATE - 1,
    p_run_id       OUT NUMBER,
    p_status       OUT VARCHAR2,
    p_message      OUT VARCHAR2
)
AUTHID CURRENT_USER
AS
    v_start_time TIMESTAMP := SYSTIMESTAMP;
BEGIN
    p_run_id := SEQ_ETL_RUN.NEXTVAL;
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_INCREMENTAL', 
                  'Starting Incremental ETL since ' || TO_CHAR(p_since_date, 'YYYY-MM-DD'), 
                  'STARTED', 0, NULL, v_start_time);
    
    -- For this project, since OLTP doesn't have audit columns,
    -- we run full sync but the MERGE logic ensures idempotency
    -- Only changed records are actually updated
    
    ETL_SYNC_DIM_ARTIST(p_run_id);
    ETL_SYNC_DIM_COLLECTION(p_run_id);
    ETL_SYNC_DIM_LOCATION(p_run_id);
    ETL_SYNC_DIM_EXHIBITOR(p_run_id);
    ETL_SYNC_DIM_POLICY(p_run_id);
    ETL_SYNC_DIM_EXHIBITION(p_run_id);
    ETL_SYNC_DIM_ARTWORK(p_run_id);
    ETL_SYNC_FACT_EXHIBITION_ACTIVITY(p_run_id);
    
    p_status := 'SUCCESS';
    p_message := 'Incremental ETL completed';
    
    ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_INCREMENTAL', 'Completed', 'COMPLETED', 
                  0, NULL, v_start_time, SYSTIMESTAMP);
                  
EXCEPTION
    WHEN OTHERS THEN
        p_status := 'ERROR';
        p_message := SQLERRM;
        ETL_LOG_ENTRY(p_run_id, 'ETL_SYNC_INCREMENTAL', 'Error', 'ERROR', 
                      0, SQLERRM, v_start_time, SYSTIMESTAMP);
        RAISE;
END ETL_SYNC_INCREMENTAL;
/

-- ============================================================================
-- SECTION 6: UTILITY FUNCTIONS FOR API/BACKEND INTEGRATION
-- ============================================================================

/*
  GET_ETL_RUN_STATUS - Check status of an ETL run
*/
CREATE OR REPLACE FUNCTION GET_ETL_RUN_STATUS (
    p_run_id IN NUMBER
) RETURN VARCHAR2
AUTHID CURRENT_USER
AS
    v_status VARCHAR2(32);
    v_error_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_error_count
    FROM ETL_LOG
    WHERE ETL_RUN_ID = p_run_id AND STATUS = 'ERROR';
    
    IF v_error_count > 0 THEN
        RETURN 'ERROR';
    END IF;
    
    SELECT COUNT(*) INTO v_error_count
    FROM ETL_LOG
    WHERE ETL_RUN_ID = p_run_id AND STATUS = 'STARTED'
      AND END_TIME IS NULL;
    
    IF v_error_count > 0 THEN
        RETURN 'RUNNING';
    END IF;
    
    RETURN 'COMPLETED';
END GET_ETL_RUN_STATUS;
/

/*
  GET_ETL_RUN_SUMMARY - Returns JSON summary for API consumption
*/
CREATE OR REPLACE FUNCTION GET_ETL_RUN_SUMMARY (
    p_run_id IN NUMBER
) RETURN CLOB
AUTHID CURRENT_USER
AS
    v_result CLOB;
BEGIN
    SELECT JSON_OBJECT(
        'runId'           VALUE p_run_id,
        'status'          VALUE GET_ETL_RUN_STATUS(p_run_id),
        'totalRecords'    VALUE NVL(SUM(RECORDS_AFFECTED), 0),
        'startTime'       VALUE TO_CHAR(MIN(START_TIME), 'YYYY-MM-DD"T"HH24:MI:SS'),
        'endTime'         VALUE TO_CHAR(MAX(END_TIME), 'YYYY-MM-DD"T"HH24:MI:SS'),
        'errorCount'      VALUE SUM(CASE WHEN STATUS = 'ERROR' THEN 1 ELSE 0 END),
        'steps' VALUE (
            SELECT JSON_ARRAYAGG(
                JSON_OBJECT(
                    'procedure' VALUE PROCEDURE_NAME,
                    'step'      VALUE STEP_NAME,
                    'status'    VALUE STATUS,
                    'records'   VALUE RECORDS_AFFECTED,
                    'error'     VALUE ERROR_MESSAGE
                )
            )
            FROM ETL_LOG
            WHERE ETL_RUN_ID = p_run_id
        )
    ) INTO v_result
    FROM ETL_LOG
    WHERE ETL_RUN_ID = p_run_id;
    
    RETURN v_result;
END GET_ETL_RUN_SUMMARY;
/

-- ============================================================================
-- SECTION 7: INDEXES FOR OPTIMAL ETL PERFORMANCE
-- ============================================================================

-- Indexes on dimension OLTP ID columns for fast lookups
CREATE INDEX IDX_DIM_ARTIST_OLTP ON DIM_ARTIST(ARTIST_ID_OLTP);
CREATE INDEX IDX_DIM_COLLECTION_OLTP ON DIM_COLLECTION(COLLECTION_ID_OLTP);
CREATE INDEX IDX_DIM_LOCATION_OLTP ON DIM_LOCATION(LOCATION_ID_OLTP);
CREATE INDEX IDX_DIM_EXHIBITOR_OLTP ON DIM_EXHIBITOR(EXHIBITOR_ID_OLTP);
CREATE INDEX IDX_DIM_EXHIBITION_OLTP ON DIM_EXHIBITION(EXHIBITION_ID_OLTP);
CREATE INDEX IDX_DIM_ARTWORK_OLTP ON DIM_ARTWORK(ARTWORK_ID_OLTP);
CREATE INDEX IDX_DIM_POLICY_OLTP ON DIM_POLICY(POLICY_ID_OLTP);

-- Composite index on fact table business key for MERGE performance
CREATE UNIQUE INDEX IDX_FACT_BUSINESS_KEY 
    ON FACT_EXHIBITION_ACTIVITY(ARTWORK_KEY, EXHIBITION_KEY);

-- Index for fact table date-based queries
CREATE INDEX IDX_FACT_DATE_EXHIBITION 
    ON FACT_EXHIBITION_ACTIVITY(DATE_KEY, EXHIBITION_KEY);
/

-- ============================================================================
-- SECTION 8: GRANTS FOR .NET BACKEND ACCESS
-- ============================================================================

-- Grant execute on procedures to DW schema and API user
GRANT EXECUTE ON ETL_SYNC_ALL TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_INCREMENTAL TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_DIM_ARTIST TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_DIM_COLLECTION TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_DIM_LOCATION TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_DIM_EXHIBITOR TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_DIM_EXHIBITION TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_DIM_ARTWORK TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_DIM_POLICY TO ART_GALLERY_DW;
GRANT EXECUTE ON ETL_SYNC_FACT_EXHIBITION_ACTIVITY TO ART_GALLERY_DW;
GRANT EXECUTE ON GET_ETL_RUN_STATUS TO ART_GALLERY_DW;
GRANT EXECUTE ON GET_ETL_RUN_SUMMARY TO ART_GALLERY_DW;

-- Grant SELECT on ETL_LOG for monitoring
GRANT SELECT ON ETL_LOG TO ART_GALLERY_DW;
/

COMMIT;
/

-- ============================================================================
-- END OF ETL PACKAGE
-- ============================================================================
