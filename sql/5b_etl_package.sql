/*
================================================================================
  ART GALLERY DW/BI - ETL PACKAGE (Alternative Approach)
  
  This package wraps the standalone ETL procedures to provide a unified
  interface compatible with the existing .NET backend service.
  
  The backend calls: ART_GALLERY_DW.PKG_ETL.PROPAGATE_OLTP_TO_DW
  
  This package provides that interface while delegating to the robust
  MERGE-based procedures in 5_etl_procedures.sql.
  
  Author: Oracle PL/SQL ETL Specialist
  Version: 1.0.0 - Production Grade for Master's Thesis
================================================================================
*/

-- ============================================================================
-- PACKAGE SPECIFICATION
-- ============================================================================

CREATE OR REPLACE PACKAGE PKG_ETL AS
    /*
      Main entry point for ETL propagation from OLTP to DW.
      Called by .NET backend via OracleProcedureService.
      
      Parameters:
        p_mode           - 'FULL' or 'INCREMENTAL'
        p_records_loaded - OUT: Total records processed
        p_status         - OUT: 'Success' or 'Error'
    */
    PROCEDURE PROPAGATE_OLTP_TO_DW (
        p_mode           IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records_loaded OUT NUMBER,
        p_status         OUT VARCHAR2
    );
    
    /*
      Individual table sync procedures called by .NET backend.
      Parameters:
        p_mode - 'FULL' or 'INCREMENTAL'
      Returns: Number of records processed
    */
    PROCEDURE SYNC_DIM_ARTIST (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    );
    
    PROCEDURE SYNC_DIM_ARTWORK (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    );
    
    PROCEDURE SYNC_DIM_EXHIBITION (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    );
    
    PROCEDURE SYNC_DIM_LOCATION (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    );
    
    PROCEDURE SYNC_FACT_EXHIBITION_ACTIVITY (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    );
    
    /*
      Get ETL run status.
    */
    FUNCTION GET_RUN_STATUS (
        p_run_id IN NUMBER
    ) RETURN VARCHAR2;
    
    /*
      Get ETL run summary as JSON.
    */
    FUNCTION GET_RUN_SUMMARY (
        p_run_id IN NUMBER
    ) RETURN CLOB;
    
END PKG_ETL;
/

-- ============================================================================
-- PACKAGE BODY
-- ============================================================================

CREATE OR REPLACE PACKAGE BODY PKG_ETL AS

    -- Private variable to track current run ID
    g_current_run_id NUMBER;

    /*
      PROPAGATE_OLTP_TO_DW - Main entry point
      
      This procedure orchestrates the complete ETL pipeline by calling
      the robust MERGE-based procedures defined in 5_etl_procedures.sql.
    */
    PROCEDURE PROPAGATE_OLTP_TO_DW (
        p_mode           IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records_loaded OUT NUMBER,
        p_status         OUT VARCHAR2
    )
    AS
        v_run_id       NUMBER;
        v_status       VARCHAR2(32);
        v_message      VARCHAR2(4000);
        v_records_sync NUMBER;
    BEGIN
        -- Call the main ETL_SYNC_ALL procedure
        ETL_SYNC_ALL(
            p_run_id       => v_run_id,
            p_status       => v_status,
            p_message      => v_message,
            p_records_sync => v_records_sync
        );
        
        -- Store run ID for subsequent calls
        g_current_run_id := v_run_id;
        
        -- Return results
        p_records_loaded := v_records_sync;
        p_status := v_status;
        
    EXCEPTION
        WHEN OTHERS THEN
            p_records_loaded := 0;
            p_status := 'Error: ' || SQLERRM;
            RAISE;
    END PROPAGATE_OLTP_TO_DW;
    
    /*
      SYNC_DIM_ARTIST
    */
    PROCEDURE SYNC_DIM_ARTIST (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    )
    AS
        v_run_id NUMBER;
    BEGIN
        v_run_id := NVL(g_current_run_id, SEQ_ETL_RUN.NEXTVAL);
        g_current_run_id := v_run_id;
        
        ETL_SYNC_DIM_ARTIST(v_run_id);
        
        SELECT RECORDS_AFFECTED INTO p_records
        FROM ETL_LOG
        WHERE ETL_RUN_ID = v_run_id 
          AND PROCEDURE_NAME = 'ETL_SYNC_DIM_ARTIST'
          AND STATUS = 'COMPLETED'
        ORDER BY LOG_ID DESC
        FETCH FIRST 1 ROW ONLY;
        
        p_status := 'Success';
        
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_records := 0;
            p_status := 'Success';
        WHEN OTHERS THEN
            p_records := 0;
            p_status := 'Error: ' || SQLERRM;
    END SYNC_DIM_ARTIST;
    
    /*
      SYNC_DIM_ARTWORK
    */
    PROCEDURE SYNC_DIM_ARTWORK (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    )
    AS
        v_run_id NUMBER;
    BEGIN
        v_run_id := NVL(g_current_run_id, SEQ_ETL_RUN.NEXTVAL);
        g_current_run_id := v_run_id;
        
        -- Ensure dependencies are synced first
        ETL_SYNC_DIM_ARTIST(v_run_id);
        ETL_SYNC_DIM_COLLECTION(v_run_id);
        ETL_SYNC_DIM_LOCATION(v_run_id);
        
        -- Sync artwork
        ETL_SYNC_DIM_ARTWORK(v_run_id);
        
        SELECT RECORDS_AFFECTED INTO p_records
        FROM ETL_LOG
        WHERE ETL_RUN_ID = v_run_id 
          AND PROCEDURE_NAME = 'ETL_SYNC_DIM_ARTWORK'
          AND STATUS = 'COMPLETED'
        ORDER BY LOG_ID DESC
        FETCH FIRST 1 ROW ONLY;
        
        p_status := 'Success';
        
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_records := 0;
            p_status := 'Success';
        WHEN OTHERS THEN
            p_records := 0;
            p_status := 'Error: ' || SQLERRM;
    END SYNC_DIM_ARTWORK;
    
    /*
      SYNC_DIM_EXHIBITION
    */
    PROCEDURE SYNC_DIM_EXHIBITION (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    )
    AS
        v_run_id NUMBER;
    BEGIN
        v_run_id := NVL(g_current_run_id, SEQ_ETL_RUN.NEXTVAL);
        g_current_run_id := v_run_id;
        
        -- Ensure dependency is synced first
        ETL_SYNC_DIM_EXHIBITOR(v_run_id);
        
        -- Sync exhibition
        ETL_SYNC_DIM_EXHIBITION(v_run_id);
        
        SELECT RECORDS_AFFECTED INTO p_records
        FROM ETL_LOG
        WHERE ETL_RUN_ID = v_run_id 
          AND PROCEDURE_NAME = 'ETL_SYNC_DIM_EXHIBITION'
          AND STATUS = 'COMPLETED'
        ORDER BY LOG_ID DESC
        FETCH FIRST 1 ROW ONLY;
        
        p_status := 'Success';
        
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_records := 0;
            p_status := 'Success';
        WHEN OTHERS THEN
            p_records := 0;
            p_status := 'Error: ' || SQLERRM;
    END SYNC_DIM_EXHIBITION;
    
    /*
      SYNC_DIM_LOCATION
    */
    PROCEDURE SYNC_DIM_LOCATION (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    )
    AS
        v_run_id NUMBER;
    BEGIN
        v_run_id := NVL(g_current_run_id, SEQ_ETL_RUN.NEXTVAL);
        g_current_run_id := v_run_id;
        
        ETL_SYNC_DIM_LOCATION(v_run_id);
        
        SELECT RECORDS_AFFECTED INTO p_records
        FROM ETL_LOG
        WHERE ETL_RUN_ID = v_run_id 
          AND PROCEDURE_NAME = 'ETL_SYNC_DIM_LOCATION'
          AND STATUS = 'COMPLETED'
        ORDER BY LOG_ID DESC
        FETCH FIRST 1 ROW ONLY;
        
        p_status := 'Success';
        
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_records := 0;
            p_status := 'Success';
        WHEN OTHERS THEN
            p_records := 0;
            p_status := 'Error: ' || SQLERRM;
    END SYNC_DIM_LOCATION;
    
    /*
      SYNC_FACT_EXHIBITION_ACTIVITY - Core fact table sync
    */
    PROCEDURE SYNC_FACT_EXHIBITION_ACTIVITY (
        p_mode    IN  VARCHAR2 DEFAULT 'INCREMENTAL',
        p_records OUT NUMBER,
        p_status  OUT VARCHAR2
    )
    AS
        v_run_id NUMBER;
    BEGIN
        v_run_id := NVL(g_current_run_id, SEQ_ETL_RUN.NEXTVAL);
        g_current_run_id := v_run_id;
        
        -- Sync all dimensions first (fact depends on all)
        ETL_SYNC_DIM_ARTIST(v_run_id);
        ETL_SYNC_DIM_COLLECTION(v_run_id);
        ETL_SYNC_DIM_LOCATION(v_run_id);
        ETL_SYNC_DIM_EXHIBITOR(v_run_id);
        ETL_SYNC_DIM_POLICY(v_run_id);
        ETL_SYNC_DIM_EXHIBITION(v_run_id);
        ETL_SYNC_DIM_ARTWORK(v_run_id);
        
        -- Sync fact table
        ETL_SYNC_FACT_EXHIBITION_ACTIVITY(v_run_id);
        
        SELECT RECORDS_AFFECTED INTO p_records
        FROM ETL_LOG
        WHERE ETL_RUN_ID = v_run_id 
          AND PROCEDURE_NAME = 'ETL_SYNC_FACT_EXHIBITION_ACTIVITY'
          AND STATUS = 'COMPLETED'
        ORDER BY LOG_ID DESC
        FETCH FIRST 1 ROW ONLY;
        
        p_status := 'Success';
        
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_records := 0;
            p_status := 'Success';
        WHEN OTHERS THEN
            p_records := 0;
            p_status := 'Error: ' || SQLERRM;
    END SYNC_FACT_EXHIBITION_ACTIVITY;
    
    /*
      GET_RUN_STATUS - Returns status of ETL run
    */
    FUNCTION GET_RUN_STATUS (
        p_run_id IN NUMBER
    ) RETURN VARCHAR2
    AS
    BEGIN
        RETURN GET_ETL_RUN_STATUS(p_run_id);
    END GET_RUN_STATUS;
    
    /*
      GET_RUN_SUMMARY - Returns JSON summary of ETL run
    */
    FUNCTION GET_RUN_SUMMARY (
        p_run_id IN NUMBER
    ) RETURN CLOB
    AS
    BEGIN
        RETURN GET_ETL_RUN_SUMMARY(p_run_id);
    END GET_RUN_SUMMARY;

END PKG_ETL;
/

-- Grant execute to application user
GRANT EXECUTE ON PKG_ETL TO ART_GALLERY_OLTP;
/

COMMIT;
/

/*
================================================================================
  END OF PKG_ETL
================================================================================
*/
