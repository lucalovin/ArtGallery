-- =============================================================================
-- Art Gallery DW/BI System - Cross-Schema Grants
-- =============================================================================
-- This script grants SELECT permissions on OLTP tables to the DW user.
-- Required for the code-based ETL to work from the backend.
-- 
-- RUN THIS SCRIPT AS: SYS or ART_GALLERY_OLTP user
-- =============================================================================

-- Connect as SYS or OLTP owner
-- ALTER SESSION SET CONTAINER = orclpdb;

-- Option 1: Run as SYS (DBA)
-- GRANT SELECT ON ART_GALLERY_OLTP.Artist TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Artwork TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Collection TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Location TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Exhibitor TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Exhibition TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Artwork_Exhibition TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Insurance TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Insurance_Policy TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Loan TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Restoration TO ART_GALLERY_DW;
-- GRANT SELECT ON ART_GALLERY_OLTP.Gallery_Review TO ART_GALLERY_DW;

-- Option 2: Run as ART_GALLERY_OLTP user (table owner)
GRANT SELECT ON Artist TO ART_GALLERY_DW;
GRANT SELECT ON Artwork TO ART_GALLERY_DW;
GRANT SELECT ON Collection TO ART_GALLERY_DW;
GRANT SELECT ON Location TO ART_GALLERY_DW;
GRANT SELECT ON Exhibitor TO ART_GALLERY_DW;
GRANT SELECT ON Exhibition TO ART_GALLERY_DW;
GRANT SELECT ON Artwork_Exhibition TO ART_GALLERY_DW;
GRANT SELECT ON Insurance TO ART_GALLERY_DW;
GRANT SELECT ON Insurance_Policy TO ART_GALLERY_DW;
GRANT SELECT ON Loan TO ART_GALLERY_DW;
GRANT SELECT ON Restoration TO ART_GALLERY_DW;
GRANT SELECT ON Gallery_Review TO ART_GALLERY_DW;

-- Create public synonyms (optional but helps with cleaner SQL)
-- Run as SYS:
-- CREATE OR REPLACE PUBLIC SYNONYM oltp_artist FOR ART_GALLERY_OLTP.Artist;
-- ... etc.

-- Verify grants were successful
-- Run as ART_GALLERY_DW:
-- SELECT COUNT(*) FROM ART_GALLERY_OLTP.Artist;
-- SELECT COUNT(*) FROM ART_GALLERY_OLTP.Artwork;
-- SELECT COUNT(*) FROM ART_GALLERY_OLTP.Exhibition;

PROMPT Grants completed successfully!
