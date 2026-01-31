/*
================================================================================
  ART GALLERY DW/BI - ETL TEST SCRIPTS
  
  This file contains comprehensive test scripts to:
  1. Verify ETL installation
  2. Simulate frontend inserts to OLTP
  3. Execute ETL sync
  4. Verify fact table population
  5. Validate measure calculations
  
  Author: Oracle PL/SQL ETL Specialist
  Version: 1.0.0 - Production Grade for Master's Thesis
================================================================================
*/

-- ============================================================================
-- SECTION 1: PRE-FLIGHT CHECKS
-- ============================================================================

-- Check if sequences exist
SELECT sequence_name, last_number 
FROM user_sequences 
WHERE sequence_name LIKE 'SEQ_%'
ORDER BY sequence_name;

-- Check if ETL procedures exist
SELECT object_name, object_type, status 
FROM user_objects 
WHERE object_name LIKE 'ETL_%'
ORDER BY object_type, object_name;

-- Check dimension table counts (should have data from initial load)
SELECT 'DIM_ARTIST' AS table_name, COUNT(*) AS row_count FROM DIM_ARTIST
UNION ALL SELECT 'DIM_COLLECTION', COUNT(*) FROM DIM_COLLECTION
UNION ALL SELECT 'DIM_LOCATION', COUNT(*) FROM DIM_LOCATION
UNION ALL SELECT 'DIM_EXHIBITOR', COUNT(*) FROM DIM_EXHIBITOR
UNION ALL SELECT 'DIM_EXHIBITION', COUNT(*) FROM DIM_EXHIBITION
UNION ALL SELECT 'DIM_ARTWORK', COUNT(*) FROM DIM_ARTWORK
UNION ALL SELECT 'DIM_POLICY', COUNT(*) FROM DIM_POLICY
UNION ALL SELECT 'FACT_EXHIBITION_ACTIVITY', COUNT(*) FROM FACT_EXHIBITION_ACTIVITY;

-- ============================================================================
-- SECTION 2: INITIAL ETL RUN (Full Sync)
-- ============================================================================

-- Run full ETL sync and capture results
SET SERVEROUTPUT ON SIZE UNLIMITED;

DECLARE
    v_run_id       NUMBER;
    v_status       VARCHAR2(32);
    v_message      VARCHAR2(4000);
    v_records_sync NUMBER;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Starting Full ETL Sync ===');
    DBMS_OUTPUT.PUT_LINE('Timestamp: ' || TO_CHAR(SYSTIMESTAMP, 'YYYY-MM-DD HH24:MI:SS.FF3'));
    
    ETL_SYNC_ALL(
        p_run_id       => v_run_id,
        p_status       => v_status,
        p_message      => v_message,
        p_records_sync => v_records_sync
    );
    
    DBMS_OUTPUT.PUT_LINE('=== ETL Sync Completed ===');
    DBMS_OUTPUT.PUT_LINE('Run ID: ' || v_run_id);
    DBMS_OUTPUT.PUT_LINE('Status: ' || v_status);
    DBMS_OUTPUT.PUT_LINE('Message: ' || v_message);
    DBMS_OUTPUT.PUT_LINE('Records Synced: ' || v_records_sync);
    
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('=== ETL Sync FAILED ===');
        DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
        DBMS_OUTPUT.PUT_LINE('Backtrace: ' || DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
END;
/

-- View ETL log for the last run
SELECT * FROM ETL_LOG 
WHERE ETL_RUN_ID = (SELECT MAX(ETL_RUN_ID) FROM ETL_LOG)
ORDER BY LOG_ID;

-- Verify fact table population
SELECT COUNT(*) AS fact_row_count FROM FACT_EXHIBITION_ACTIVITY;

SELECT * FROM FACT_EXHIBITION_ACTIVITY 
ORDER BY DATE_KEY DESC, FACT_KEY DESC
FETCH FIRST 10 ROWS ONLY;

-- ============================================================================
-- SECTION 3: SIMULATE FRONTEND INSERT (New Exhibition Activity)
-- ============================================================================

/*
  Test Scenario: 
  Frontend adds a new artwork to an exhibition via ARTWORK_EXHIBITION table.
  This simulates the real-world workflow that was failing.
*/

-- Step 3.1: Record current fact table state
SELECT COUNT(*) AS before_count FROM FACT_EXHIBITION_ACTIVITY;

-- Step 3.2: Insert new artwork-exhibition link (simulating frontend)
-- Using artwork_id=6 (Impression, Sunrise) and exhibition_id=1 (Modern Icons)
INSERT INTO art_gallery_oltp.Artwork_Exhibition (
    artwork_id, 
    exhibition_id, 
    position_in_gallery, 
    featured_status
) VALUES (
    6,                      -- Impression, Sunrise by Monet
    1,                      -- Modern Icons exhibition
    'Room 1 - Right Wing',  
    'Featured'
);
COMMIT;

DBMS_OUTPUT.PUT_LINE('Inserted new ARTWORK_EXHIBITION record: artwork_id=6, exhibition_id=1');

-- Step 3.3: Verify OLTP insert succeeded
SELECT * FROM art_gallery_oltp.Artwork_Exhibition 
WHERE artwork_id = 6 AND exhibition_id = 1;

-- ============================================================================
-- SECTION 4: RUN ETL SYNC (Should Propagate to Fact)
-- ============================================================================

DECLARE
    v_run_id       NUMBER;
    v_status       VARCHAR2(32);
    v_message      VARCHAR2(4000);
    v_records_sync NUMBER;
BEGIN
    DBMS_OUTPUT.PUT_LINE('');
    DBMS_OUTPUT.PUT_LINE('=== Running ETL Sync After Frontend Insert ===');
    
    ETL_SYNC_ALL(
        p_run_id       => v_run_id,
        p_status       => v_status,
        p_message      => v_message,
        p_records_sync => v_records_sync
    );
    
    DBMS_OUTPUT.PUT_LINE('Run ID: ' || v_run_id);
    DBMS_OUTPUT.PUT_LINE('Status: ' || v_status);
    DBMS_OUTPUT.PUT_LINE('Records Synced: ' || v_records_sync);
END;
/

-- ============================================================================
-- SECTION 5: VERIFY FACT TABLE PROPAGATION
-- ============================================================================

-- Step 5.1: Check fact table count increased
SELECT COUNT(*) AS after_count FROM FACT_EXHIBITION_ACTIVITY;

-- Step 5.2: Find the new fact row (should exist with all columns populated)
SELECT 
    f.FACT_KEY,
    f.DATE_KEY,
    dex.TITLE AS exhibition_title,
    daw.TITLE AS artwork_title,
    dar.NAME AS artist_name,
    f.ESTIMATED_VALUE,
    f.INSURED_AMOUNT,
    f.LOAN_FLAG,
    f.RESTORATION_COUNT,
    f.REVIEW_COUNT,
    f.AVG_RATING,
    -- Verify dimension keys are NOT NULL
    f.EXHIBITION_KEY,
    f.EXHIBITOR_KEY,
    f.ARTWORK_KEY,
    f.ARTIST_KEY,
    f.COLLECTION_KEY,
    f.LOCATION_KEY,
    f.POLICY_KEY
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION dex ON dex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTWORK daw ON daw.ARTWORK_KEY = f.ARTWORK_KEY
JOIN DIM_ARTIST dar ON dar.ARTIST_KEY = f.ARTIST_KEY
WHERE daw.ARTWORK_ID_OLTP = 6 AND dex.EXHIBITION_ID_OLTP = 1;

-- Step 5.3: Verify no NULL dimension keys in fact table
SELECT 
    COUNT(*) AS total_rows,
    SUM(CASE WHEN EXHIBITION_KEY IS NULL THEN 1 ELSE 0 END) AS null_exhibition,
    SUM(CASE WHEN EXHIBITOR_KEY IS NULL THEN 1 ELSE 0 END) AS null_exhibitor,
    SUM(CASE WHEN ARTWORK_KEY IS NULL THEN 1 ELSE 0 END) AS null_artwork,
    SUM(CASE WHEN ARTIST_KEY IS NULL THEN 1 ELSE 0 END) AS null_artist,
    SUM(CASE WHEN DATE_KEY IS NULL THEN 1 ELSE 0 END) AS null_date
FROM FACT_EXHIBITION_ACTIVITY;

-- ============================================================================
-- SECTION 6: TEST MEASURE UPDATES (Reviews, Loans, Restorations)
-- ============================================================================

/*
  Test Scenario:
  Add a review for the artwork we just linked, then verify AVG_RATING updates
*/

-- Step 6.1: Add a review in OLTP
INSERT INTO art_gallery_oltp.Gallery_Review (
    visitor_id,
    artwork_id,
    exhibition_id,
    rating,
    review_text,
    review_date
) VALUES (
    1,      -- Alice Johnson
    6,      -- Impression, Sunrise
    1,      -- Modern Icons exhibition
    5,      -- Excellent rating
    'Absolutely stunning impressionist work!',
    SYSDATE
);
COMMIT;

-- Step 6.2: Run ETL sync
DECLARE
    v_run_id       NUMBER;
    v_status       VARCHAR2(32);
    v_message      VARCHAR2(4000);
    v_records_sync NUMBER;
BEGIN
    ETL_SYNC_ALL(v_run_id, v_status, v_message, v_records_sync);
    DBMS_OUTPUT.PUT_LINE('ETL Run ' || v_run_id || ': ' || v_status);
END;
/

-- Step 6.3: Verify REVIEW_COUNT and AVG_RATING updated
SELECT 
    daw.TITLE AS artwork,
    dex.TITLE AS exhibition,
    f.REVIEW_COUNT,
    f.AVG_RATING
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_ARTWORK daw ON daw.ARTWORK_KEY = f.ARTWORK_KEY
JOIN DIM_EXHIBITION dex ON dex.EXHIBITION_KEY = f.EXHIBITION_KEY
WHERE daw.ARTWORK_ID_OLTP = 6 AND dex.EXHIBITION_ID_OLTP = 1;

-- ============================================================================
-- SECTION 7: TEST NEW ARTWORK CREATION (Full Pipeline)
-- ============================================================================

/*
  Test Scenario:
  Create a completely new artwork from scratch and verify full propagation
*/

-- Step 7.1: Add new artist
INSERT INTO art_gallery_oltp.Artist (name, nationality, birth_year, death_year) 
VALUES ('Frida Kahlo', 'Mexican', 1907, 1954);

-- Step 7.2: Add new artwork
INSERT INTO art_gallery_oltp.Artwork (
    title, artist_id, year_created, medium, 
    collection_id, location_id, estimated_value
)
SELECT 
    'The Two Fridas', 
    artist_id, 
    1939, 
    'Oil on Canvas',
    1,  -- Modern Masters collection
    1,  -- Main Hall location
    2500000
FROM art_gallery_oltp.Artist 
WHERE name = 'Frida Kahlo';

-- Step 7.3: Add insurance
INSERT INTO art_gallery_oltp.Insurance (artwork_id, policy_id, insured_amount)
SELECT artwork_id, 1, 2000000
FROM art_gallery_oltp.Artwork 
WHERE title = 'The Two Fridas';

-- Step 7.4: Link to exhibition
INSERT INTO art_gallery_oltp.Artwork_Exhibition (
    artwork_id, exhibition_id, position_in_gallery, featured_status
)
SELECT artwork_id, 1, 'Room 1 - Highlight Wall', 'Featured'
FROM art_gallery_oltp.Artwork 
WHERE title = 'The Two Fridas';

COMMIT;

-- Step 7.5: Run ETL sync
DECLARE
    v_run_id       NUMBER;
    v_status       VARCHAR2(32);
    v_message      VARCHAR2(4000);
    v_records_sync NUMBER;
BEGIN
    ETL_SYNC_ALL(v_run_id, v_status, v_message, v_records_sync);
    DBMS_OUTPUT.PUT_LINE('=== Full Pipeline Test ===');
    DBMS_OUTPUT.PUT_LINE('Run ID: ' || v_run_id || ', Status: ' || v_status);
    DBMS_OUTPUT.PUT_LINE('Records Synced: ' || v_records_sync);
END;
/

-- Step 7.6: Verify new artwork appears in fact table with all measures
SELECT 
    f.FACT_KEY,
    f.DATE_KEY,
    dex.TITLE AS exhibition,
    daw.TITLE AS artwork,
    dar.NAME AS artist,
    dar.NATIONALITY,
    f.ESTIMATED_VALUE,
    f.INSURED_AMOUNT,
    f.LOAN_FLAG,
    dcol.NAME AS collection,
    dloc.NAME AS location
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_ARTWORK daw ON daw.ARTWORK_KEY = f.ARTWORK_KEY
JOIN DIM_ARTIST dar ON dar.ARTIST_KEY = f.ARTIST_KEY
JOIN DIM_EXHIBITION dex ON dex.EXHIBITION_KEY = f.EXHIBITION_KEY
LEFT JOIN DIM_COLLECTION dcol ON dcol.COLLECTION_KEY = f.COLLECTION_KEY
LEFT JOIN DIM_LOCATION dloc ON dloc.LOCATION_KEY = f.LOCATION_KEY
WHERE dar.NAME = 'Frida Kahlo';

-- ============================================================================
-- SECTION 8: IDEMPOTENCY TEST (Running ETL Multiple Times)
-- ============================================================================

/*
  Test Scenario:
  Run ETL multiple times without changes - should not duplicate records
*/

DECLARE
    v_count_before NUMBER;
    v_count_after  NUMBER;
    v_run_id       NUMBER;
    v_status       VARCHAR2(32);
    v_message      VARCHAR2(4000);
    v_records_sync NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count_before FROM FACT_EXHIBITION_ACTIVITY;
    
    -- Run ETL 3 times
    FOR i IN 1..3 LOOP
        ETL_SYNC_ALL(v_run_id, v_status, v_message, v_records_sync);
        DBMS_OUTPUT.PUT_LINE('Run ' || i || ' (ID: ' || v_run_id || '): ' || v_status);
    END LOOP;
    
    SELECT COUNT(*) INTO v_count_after FROM FACT_EXHIBITION_ACTIVITY;
    
    DBMS_OUTPUT.PUT_LINE('');
    DBMS_OUTPUT.PUT_LINE('=== Idempotency Test Results ===');
    DBMS_OUTPUT.PUT_LINE('Count Before: ' || v_count_before);
    DBMS_OUTPUT.PUT_LINE('Count After:  ' || v_count_after);
    
    IF v_count_before = v_count_after THEN
        DBMS_OUTPUT.PUT_LINE('TEST PASSED: No duplicate rows created');
    ELSE
        DBMS_OUTPUT.PUT_LINE('TEST FAILED: Row count changed!');
    END IF;
END;
/

-- ============================================================================
-- SECTION 9: ETL LOG ANALYSIS
-- ============================================================================

-- View all ETL runs
SELECT 
    ETL_RUN_ID,
    MIN(START_TIME) AS run_start,
    MAX(END_TIME) AS run_end,
    EXTRACT(SECOND FROM (MAX(END_TIME) - MIN(START_TIME))) AS duration_sec,
    SUM(RECORDS_AFFECTED) AS total_records,
    SUM(CASE WHEN STATUS = 'ERROR' THEN 1 ELSE 0 END) AS error_count
FROM ETL_LOG
GROUP BY ETL_RUN_ID
ORDER BY ETL_RUN_ID DESC
FETCH FIRST 10 ROWS ONLY;

-- View errors only
SELECT * FROM ETL_LOG 
WHERE STATUS = 'ERROR'
ORDER BY LOG_ID DESC;

-- View step-by-step breakdown of last run
SELECT 
    PROCEDURE_NAME,
    STEP_NAME,
    STATUS,
    RECORDS_AFFECTED,
    TO_CHAR(START_TIME, 'HH24:MI:SS.FF3') AS start_time,
    TO_CHAR(END_TIME, 'HH24:MI:SS.FF3') AS end_time,
    ERROR_MESSAGE
FROM ETL_LOG
WHERE ETL_RUN_ID = (SELECT MAX(ETL_RUN_ID) FROM ETL_LOG)
ORDER BY LOG_ID;

-- ============================================================================
-- SECTION 10: CLEANUP TEST DATA (Optional)
-- ============================================================================

/*
  Uncomment to remove test data added during tests
*/

/*
-- Remove test artwork-exhibition links
DELETE FROM art_gallery_oltp.Artwork_Exhibition 
WHERE artwork_id = 6 AND exhibition_id = 1;

-- Remove test review
DELETE FROM art_gallery_oltp.Gallery_Review 
WHERE artwork_id = 6 AND exhibition_id = 1 AND visitor_id = 1;

-- Remove Frida Kahlo test data
DELETE FROM art_gallery_oltp.Artwork_Exhibition 
WHERE artwork_id IN (SELECT artwork_id FROM art_gallery_oltp.Artwork WHERE title = 'The Two Fridas');

DELETE FROM art_gallery_oltp.Insurance 
WHERE artwork_id IN (SELECT artwork_id FROM art_gallery_oltp.Artwork WHERE title = 'The Two Fridas');

DELETE FROM art_gallery_oltp.Artwork WHERE title = 'The Two Fridas';

DELETE FROM art_gallery_oltp.Artist WHERE name = 'Frida Kahlo';

COMMIT;

-- Re-run ETL to clean fact table
DECLARE
    v_run_id NUMBER; v_status VARCHAR2(32); v_message VARCHAR2(4000); v_records NUMBER;
BEGIN
    ETL_SYNC_ALL(v_run_id, v_status, v_message, v_records);
END;
/
*/

-- ============================================================================
-- SECTION 11: FINAL VALIDATION QUERIES (For Thesis Defense)
-- ============================================================================

-- Comprehensive fact table validation
SELECT 
    'Total Fact Rows' AS metric, 
    TO_CHAR(COUNT(*)) AS value 
FROM FACT_EXHIBITION_ACTIVITY
UNION ALL
SELECT 
    'Rows with NULL Required Keys', 
    TO_CHAR(SUM(
        CASE WHEN EXHIBITION_KEY IS NULL OR EXHIBITOR_KEY IS NULL 
             OR ARTWORK_KEY IS NULL OR ARTIST_KEY IS NULL 
             OR DATE_KEY IS NULL THEN 1 ELSE 0 END
    ))
FROM FACT_EXHIBITION_ACTIVITY
UNION ALL
SELECT 
    'Total Estimated Value', 
    TO_CHAR(SUM(ESTIMATED_VALUE), 'FM$999,999,999')
FROM FACT_EXHIBITION_ACTIVITY
UNION ALL
SELECT 
    'Total Insured Amount', 
    TO_CHAR(SUM(INSURED_AMOUNT), 'FM$999,999,999')
FROM FACT_EXHIBITION_ACTIVITY
UNION ALL
SELECT 
    'Artworks on Loan', 
    TO_CHAR(SUM(LOAN_FLAG))
FROM FACT_EXHIBITION_ACTIVITY
UNION ALL
SELECT 
    'Total Restorations', 
    TO_CHAR(SUM(RESTORATION_COUNT))
FROM FACT_EXHIBITION_ACTIVITY
UNION ALL
SELECT 
    'Total Reviews', 
    TO_CHAR(SUM(REVIEW_COUNT))
FROM FACT_EXHIBITION_ACTIVITY
UNION ALL
SELECT 
    'Average Rating', 
    TO_CHAR(AVG(AVG_RATING), 'FM9.99')
FROM FACT_EXHIBITION_ACTIVITY
WHERE AVG_RATING > 0;

-- Star schema integrity check
SELECT 
    'DIM_ARTIST' AS dimension,
    COUNT(*) AS dim_rows,
    (SELECT COUNT(DISTINCT ARTIST_KEY) FROM FACT_EXHIBITION_ACTIVITY) AS used_in_fact
FROM DIM_ARTIST
UNION ALL
SELECT 'DIM_EXHIBITION', COUNT(*), (SELECT COUNT(DISTINCT EXHIBITION_KEY) FROM FACT_EXHIBITION_ACTIVITY) FROM DIM_EXHIBITION
UNION ALL
SELECT 'DIM_ARTWORK', COUNT(*), (SELECT COUNT(DISTINCT ARTWORK_KEY) FROM FACT_EXHIBITION_ACTIVITY) FROM DIM_ARTWORK
UNION ALL
SELECT 'DIM_EXHIBITOR', COUNT(*), (SELECT COUNT(DISTINCT EXHIBITOR_KEY) FROM FACT_EXHIBITION_ACTIVITY) FROM DIM_EXHIBITOR
UNION ALL
SELECT 'DIM_COLLECTION', COUNT(*), (SELECT COUNT(DISTINCT COLLECTION_KEY) FROM FACT_EXHIBITION_ACTIVITY) FROM DIM_COLLECTION
UNION ALL
SELECT 'DIM_LOCATION', COUNT(*), (SELECT COUNT(DISTINCT LOCATION_KEY) FROM FACT_EXHIBITION_ACTIVITY) FROM DIM_LOCATION
UNION ALL
SELECT 'DIM_POLICY', COUNT(*), (SELECT COUNT(DISTINCT POLICY_KEY) FROM FACT_EXHIBITION_ACTIVITY) FROM DIM_POLICY;

COMMIT;
/
