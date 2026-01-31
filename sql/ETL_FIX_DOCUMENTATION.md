# Art Gallery DW/BI - ETL Fix Documentation

## Problem Diagnosis

### Issue Description
When adding records in OLTP from frontend (e.g., linking artwork to exhibition, adding review/loan/restoration):
1. OLTP insert succeeds ✓
2. ETL sync runs ✓
3. **NO new row appears in FACT_EXHIBITION_ACTIVITY**, OR row is inserted but columns are **NULL/empty**

### Root Cause Analysis

After reviewing the codebase, the following issues were identified:

#### 1. **Missing MERGE Logic (INSERT-Only ETL)**
The original ETL used `INSERT INTO ... SELECT` with `ROW_NUMBER()` for `FACT_KEY`:
```sql
INSERT INTO FACT_EXHIBITION_ACTIVITY (FACT_KEY, ...)
SELECT ROW_NUMBER() OVER (ORDER BY ...) AS FACT_KEY, ...
```
**Problem**: This fails for incremental loads because:
- `ROW_NUMBER()` generates sequential IDs starting from 1 each time
- Primary key conflicts occur on subsequent runs
- New records are silently rejected

#### 2. **No Sequence for Surrogate Keys**
The DW tables lacked proper sequences for generating surrogate keys, leading to:
- Duplicate key errors
- Manual key assignment that breaks on concurrent inserts

#### 3. **Direct OLTP ID Usage as Surrogate Keys**
The original code directly used OLTP IDs as dimension keys:
```sql
ex.exhibition_id AS EXHIBITION_KEY  -- Wrong!
```
**Problem**: This violates star schema principles and breaks when:
- OLTP IDs are recycled
- Slowly Changing Dimensions (SCD) are implemented
- Multiple source systems feed the DW

#### 4. **Missing Dimension Refresh Before Fact Load**
New OLTP records (e.g., new artist, artwork) weren't synced to dimensions before fact table load:
- `JOIN DIM_ARTWORK daw ON daw.ARTWORK_ID_OLTP = aw.artwork_id` fails
- New records excluded from fact table

#### 5. **Backend Calling Non-Existent Procedure**
The .NET backend called `ART_GALLERY_DW.PKG_ETL.PROPAGATE_OLTP_TO_DW` which:
- Didn't exist in the database
- Failed silently or threw Oracle errors

---

## Solution Overview

### Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           ETL PIPELINE (Fixed)                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────┐    ┌───────────────────────────────────────────────────┐ │
│  │   Frontend   │───▶│                OLTP Tables                         │ │
│  │   (Vue.js)   │    │  • Artist, Artwork, Exhibition, etc.              │ │
│  └──────────────┘    │  • Artwork_Exhibition (junction table)            │ │
│                      │  • Gallery_Review, Loan, Restoration              │ │
│                      └───────────────────────┬───────────────────────────┘ │
│                                              │                             │
│                                              ▼                             │
│  ┌──────────────┐    ┌───────────────────────────────────────────────────┐ │
│  │   Backend    │───▶│           ETL_SYNC_ALL (Orchestrator)              │ │
│  │   (.NET)     │    │  • Generates ETL_RUN_ID from sequence             │ │
│  └──────────────┘    │  • Calls dimension syncs in dependency order      │ │
│                      │  • Calls fact sync with all dimensions ready      │ │
│                      │  • Logs everything to ETL_LOG table               │ │
│                      └───────────────────────┬───────────────────────────┘ │
│                                              │                             │
│                      ┌───────────────────────▼───────────────────────────┐ │
│                      │        DIMENSION SYNC (ETL_SYNC_DIM_*)            │ │
│                      │  • Uses MERGE for idempotent upserts              │ │
│                      │  • Sequences generate surrogate keys              │ │
│                      │  • SCD Type 1 (overwrite changes)                 │ │
│                      └───────────────────────┬───────────────────────────┘ │
│                                              │                             │
│                      ┌───────────────────────▼───────────────────────────┐ │
│                      │   FACT SYNC (ETL_SYNC_FACT_EXHIBITION_ACTIVITY)   │ │
│                      │  • MERGE on business key (ARTWORK+EXHIBITION)     │ │
│                      │  • Resolves surrogate keys via JOIN               │ │
│                      │  • Calculates measures from aggregates            │ │
│                      │  • Updates only when measures change              │ │
│                      └───────────────────────────────────────────────────┘ │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Key Fixes

#### 1. Sequences for All Surrogate Keys
```sql
CREATE SEQUENCE SEQ_DIM_ARTIST START WITH 1000 INCREMENT BY 1;
CREATE SEQUENCE SEQ_DIM_ARTWORK START WITH 1000 INCREMENT BY 1;
...
CREATE SEQUENCE SEQ_FACT_EXHIBITION START WITH 1000 INCREMENT BY 1;
```

#### 2. MERGE-Based Dimension ETL
```sql
MERGE INTO DIM_ARTIST tgt
USING (SELECT * FROM art_gallery_oltp.Artist) src
ON (tgt.ARTIST_ID_OLTP = src.artist_id)
WHEN MATCHED THEN
    UPDATE SET tgt.NAME = src.name, ...
WHEN NOT MATCHED THEN
    INSERT (ARTIST_KEY, ARTIST_ID_OLTP, NAME, ...)
    VALUES (SEQ_DIM_ARTIST.NEXTVAL, src.artist_id, src.name, ...);
```

#### 3. MERGE-Based Fact ETL with Surrogate Lookup
```sql
MERGE INTO FACT_EXHIBITION_ACTIVITY tgt
USING (
    SELECT 
        daw.ARTWORK_KEY,       -- Surrogate from DIM_ARTWORK
        dex.EXHIBITION_KEY,    -- Surrogate from DIM_EXHIBITION
        dar.ARTIST_KEY,        -- Surrogate from DIM_ARTIST
        ...
        -- Calculated measures
        NVL(ins_agg.total_insured, 0) AS INSURED_AMOUNT,
        NVL(res_agg.restoration_count, 0) AS RESTORATION_COUNT,
        ...
    FROM art_gallery_oltp.Artwork_Exhibition ax
    JOIN DIM_ARTWORK daw ON daw.ARTWORK_ID_OLTP = ax.artwork_id
    JOIN DIM_EXHIBITION dex ON dex.EXHIBITION_ID_OLTP = ax.exhibition_id
    ...
) src
ON (tgt.ARTWORK_KEY = src.ARTWORK_KEY AND tgt.EXHIBITION_KEY = src.EXHIBITION_KEY)
WHEN MATCHED THEN UPDATE ...
WHEN NOT MATCHED THEN INSERT (FACT_KEY, ...) VALUES (SEQ_FACT_EXHIBITION.NEXTVAL, ...);
```

#### 4. ETL Logging for Audit Trail
```sql
CREATE TABLE ETL_LOG (
    LOG_ID           NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ETL_RUN_ID       NUMBER NOT NULL,
    PROCEDURE_NAME   VARCHAR2(128),
    STATUS           VARCHAR2(32),  -- STARTED, COMPLETED, ERROR
    RECORDS_AFFECTED NUMBER,
    ERROR_MESSAGE    VARCHAR2(4000),
    START_TIME       TIMESTAMP,
    END_TIME         TIMESTAMP
);
```

---

## Deployment Instructions

### Step 1: Deploy ETL Infrastructure
Run in SQL*Plus or SQL Developer connected as `ART_GALLERY_DW`:

```sql
@5_etl_procedures.sql
```

### Step 2: Initialize Sequences (If Existing Data)
If you have existing data in dimension/fact tables:

```sql
@5a_etl_migration.sql
```

### Step 3: Deploy Package Wrapper (Optional)
For backward compatibility with `.NET` backend:

```sql
@5b_etl_package.sql
```

### Step 4: Run Initial ETL Sync

```sql
SET SERVEROUTPUT ON;
DECLARE
    v_run_id NUMBER;
    v_status VARCHAR2(32);
    v_message VARCHAR2(4000);
    v_records NUMBER;
BEGIN
    ETL_SYNC_ALL(v_run_id, v_status, v_message, v_records);
    DBMS_OUTPUT.PUT_LINE('Run ID: ' || v_run_id);
    DBMS_OUTPUT.PUT_LINE('Status: ' || v_status);
    DBMS_OUTPUT.PUT_LINE('Records: ' || v_records);
END;
/
```

### Step 5: Verify
```sql
SELECT COUNT(*) FROM FACT_EXHIBITION_ACTIVITY;

SELECT * FROM ETL_LOG ORDER BY LOG_ID DESC FETCH FIRST 20 ROWS ONLY;
```

---

## Testing the Fix

### Test 1: Frontend Insert → ETL → Verify Fact

```sql
-- 1. Check current count
SELECT COUNT(*) AS before_count FROM FACT_EXHIBITION_ACTIVITY;

-- 2. Simulate frontend insert
INSERT INTO art_gallery_oltp.Artwork_Exhibition 
    (artwork_id, exhibition_id, position_in_gallery, featured_status)
VALUES (6, 1, 'New Position', 'Featured');
COMMIT;

-- 3. Run ETL
DECLARE
    v_run_id NUMBER; v_status VARCHAR2(32); 
    v_message VARCHAR2(4000); v_records NUMBER;
BEGIN
    ETL_SYNC_ALL(v_run_id, v_status, v_message, v_records);
    DBMS_OUTPUT.PUT_LINE('Status: ' || v_status || ', Records: ' || v_records);
END;
/

-- 4. Verify new row exists with all columns populated
SELECT f.*, daw.TITLE AS artwork, dex.TITLE AS exhibition
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_ARTWORK daw ON daw.ARTWORK_KEY = f.ARTWORK_KEY
JOIN DIM_EXHIBITION dex ON dex.EXHIBITION_KEY = f.EXHIBITION_KEY
WHERE daw.ARTWORK_ID_OLTP = 6 AND dex.EXHIBITION_ID_OLTP = 1;
```

### Test 2: Idempotency (Run Multiple Times)

```sql
DECLARE
    v_count_before NUMBER;
    v_count_after NUMBER;
    v_run_id NUMBER; v_status VARCHAR2(32); 
    v_message VARCHAR2(4000); v_records NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count_before FROM FACT_EXHIBITION_ACTIVITY;
    
    -- Run ETL 3 times
    FOR i IN 1..3 LOOP
        ETL_SYNC_ALL(v_run_id, v_status, v_message, v_records);
    END LOOP;
    
    SELECT COUNT(*) INTO v_count_after FROM FACT_EXHIBITION_ACTIVITY;
    
    IF v_count_before = v_count_after THEN
        DBMS_OUTPUT.PUT_LINE('✓ PASSED: No duplicates');
    ELSE
        DBMS_OUTPUT.PUT_LINE('✗ FAILED: Count changed');
    END IF;
END;
/
```

### Test 3: Measure Updates

```sql
-- Add a review
INSERT INTO art_gallery_oltp.Gallery_Review 
    (visitor_id, artwork_id, exhibition_id, rating, review_text, review_date)
VALUES (1, 6, 1, 5, 'Excellent!', SYSDATE);
COMMIT;

-- Run ETL
EXEC ETL_SYNC_ALL(:run_id, :status, :message, :records);

-- Verify REVIEW_COUNT updated
SELECT f.REVIEW_COUNT, f.AVG_RATING
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_ARTWORK daw ON daw.ARTWORK_KEY = f.ARTWORK_KEY
WHERE daw.ARTWORK_ID_OLTP = 6;
```

---

## Files Created/Modified

| File | Description |
|------|-------------|
| `sql/5_etl_procedures.sql` | Main ETL procedures with MERGE logic |
| `sql/5a_etl_migration.sql` | Migration script for existing data |
| `sql/5b_etl_package.sql` | Package wrapper for .NET compatibility |
| `sql/6_etl_test_scripts.sql` | Comprehensive test scripts |
| `OracleProcedureService.cs` | Updated .NET backend service |
| `EtlPropagationDtos.cs` | Added EtlRunId property |

---

## Best Practices for Master's Thesis

1. **Star Schema Integrity**: All fact foreign keys reference dimension surrogate keys, not OLTP natural keys
2. **Idempotent ETL**: MERGE ensures reruns don't create duplicates
3. **Audit Trail**: ETL_LOG captures every operation for debugging and compliance
4. **Error Handling**: All procedures have exception handlers with logging
5. **Performance**: Indexes on OLTP ID columns in dimensions for fast lookups
6. **Scalability**: Partitioned fact table by DATE_KEY for large datasets

---

## Contact

For questions about this implementation, refer to the ETL procedures' inline comments or the ETL_LOG table for operational debugging.
